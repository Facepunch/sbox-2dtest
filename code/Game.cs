using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace Sandbox;

/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client. 
/// 
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
public partial class MyGame : Sandbox.Game
{
	public new static MyGame Current => Sandbox.Game.Current as MyGame;

	public OrthoCamera MainCamera { get; } = new OrthoCamera();

	private readonly List<Enemy> _enemies = new();

	public MyGame()
	{
		if ( Host.IsServer )
		{
			for (var i = 0; i < 250; ++i)
            {
                var enemy = new Enemy
                {
                    Position = new Vector2(Rand.Float(-18f, 18f), Rand.Float(-14f, 14f)),
                    Depth = Rand.Float(-128f, 128f)
                };

				_enemies.Add(enemy);
			}
        }

		if (Host.IsClient)
        {
			_ = new MainHud();
        }
	}

	[Event.Tick.Server]
	public void ServerTick()
    {
		var dt = Time.Delta;

		var closestPlayer = GetClosestPlayer(new Vector2(0, 0));
		if (closestPlayer == null)
			return;

		foreach (var enemy in _enemies)
        {
			enemy.Velocity += (closestPlayer.Position - enemy.Position).Normal * 0.5f * dt;

            foreach (var other in _enemies)
            {
                if (enemy == other)
                    continue;

                var dist_sqr = (enemy.Position - other.Position).LengthSquared;
                var total_radius_sqr = MathF.Pow(enemy.Radius + other.Radius, 2f);
                if (dist_sqr < total_radius_sqr)
                {
                    enemy.Velocity += (enemy.Position - other.Position).Normal * Utils.Map(dist_sqr, total_radius_sqr, 0f, 0f, 1.99f) * dt;
                }
            }

            enemy.Position += enemy.Velocity * dt;
			enemy.Velocity *= 0.975f;

			//DebugOverlay.Line(enemy.Position, enemy.Position + enemy.Radius, 0f, false);

			//enemy.Scale = new Vector2(1f * enemy.Velocity.x < 0f ? -1f : 1f, 1f);
        }
    }

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var player = new PlayerCitizen();
		client.Pawn = player;

		// Get all of the spawnpoints
		var spawnpoints = Entity.All.OfType<SpawnPoint>();

		// chose a random one
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		// if it exists, place the pawn there
		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 50.0f; // raise it up
			player.Transform = tx;
		}
	}

	public override CameraMode FindActiveCamera()
	{
		return MainCamera;
	}

	public IEnumerable<PlayerCitizen> Players => Client.All
		.Select(x => x.Pawn)
		.OfType<PlayerCitizen>();

	public IEnumerable<PlayerCitizen> AlivePlayers => Players
		.Where(x => x.IsAlive);

	private T GetClosest<T>(IEnumerable<T> enumerable, Vector3 pos, float maxRange, bool ignoreZ, T except)
		where T : Entity
	{
		var dists = ignoreZ
			? enumerable.Select(x => (Entity: x, DistSq: (x.Position - pos).WithZ(0f).LengthSquared))
			: enumerable.Select(x => (Entity: x, DistSq: (x.Position - pos).LengthSquared));

		return dists.OrderBy(x => x.DistSq)
			.FirstOrDefault(x => x.DistSq <= maxRange * maxRange && x.Entity != except && (!ignoreZ || x.Entity.Parent == null))
			.Entity;
	}

	public PlayerCitizen GetClosestPlayer(Vector3 pos, float maxRange = float.PositiveInfinity, bool alive = true, bool ignoreZ = true, PlayerCitizen except = null)
	{
		var players = alive ? AlivePlayers : Players;
		return GetClosest(players, pos, maxRange, ignoreZ, except);
	}
}
