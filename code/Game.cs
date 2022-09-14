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
	public Dictionary<(int x, int y), List<Enemy>> _enemyGridPositions = new Dictionary<(int, int), List<Enemy>>();
	//public record struct GridSquare (int x, int y);

	public float GRID_SIZE = 1f;
	public Vector2 BOUNDS_MIN = new Vector2(-16f, -12f);
	public Vector2 BOUNDS_MAX = new Vector2(16f, 12f);

	public MyGame()
	{
		if ( Host.IsServer )
		{
			for (float x = BOUNDS_MIN.x; x < BOUNDS_MAX.x; x += GRID_SIZE)
			{
				for (float y = BOUNDS_MIN.y; y < BOUNDS_MAX.y; y += GRID_SIZE)
				{
					_enemyGridPositions.Add(GetGridSquareForPos(new Vector2(x, y)), new List<Enemy>());
				}
			}

			for (var i = 0; i < 750; ++i)
			//for (var i = 0; i < 5; ++i)
			{
				var pos = new Vector2(Rand.Float(-18f, 18f), Rand.Float(-14f, 14f));
				var gridPos = GetGridSquareForPos(pos);

				var enemy = new Enemy
                {
                    Position = pos,
					//Depth = Rand.Float(-128f, 128f),
					GridPos = gridPos,
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

		var xMin = BOUNDS_MIN.x + 0.3f;
		var xMax = BOUNDS_MAX.x - 0.3f;
		var yMin = BOUNDS_MIN.y + 0.3f;
		var yMax = BOUNDS_MAX.y - 0.3f;

		foreach (var enemy in _enemies)
        {
			enemy.Velocity += (closestPlayer.Position - enemy.Position).Normal * 0.5f * dt;
            enemy.Position += enemy.Velocity * dt;
			enemy.Position = new Vector2(MathX.Clamp(enemy.Position.x, xMin, xMax), MathX.Clamp(enemy.Position.y, yMin, yMax));
			enemy.Velocity *= 0.975f;

			//DebugOverlay.Line(enemy.Position, enemy.Position + enemy.Radius, 0f, false);

			enemy.Scale = new Vector2(1f * enemy.Velocity.x < 0f ? 1f : -1f, 1f) * 0.8f;
			enemy.Depth = -enemy.Position.y * 10f;

			var gridPos = GetGridSquareForPos(enemy.Position);
			if (gridPos != enemy.GridPos)
            {
				if(_enemyGridPositions.ContainsKey(enemy.GridPos) && _enemyGridPositions[enemy.GridPos].Contains(enemy))
                {
					_enemyGridPositions[enemy.GridPos].Remove(enemy);
                }

				enemy.GridPos = gridPos;

				if(IsGridSquareInArena(gridPos))
					_enemyGridPositions[gridPos].Add(enemy);
            }

			for(int dx = -1; dx <= 1; dx++)
            {
				for (int dy = -1; dy <= 1; dy++)
				{
					HandleCollisionForGridSquare(enemy, (enemy.GridPos.x + dx, enemy.GridPos.y + dy), dt);
				}
			}
        }

        //for (float x = BOUNDS_MIN.x; x < BOUNDS_MAX.x; x += GRID_SIZE)
        //{
        //    for (float y = BOUNDS_MIN.y; y < BOUNDS_MAX.y; y += GRID_SIZE)
        //    {
        //        DebugOverlay.Box(new Vector2(x, y), new Vector2(x + GRID_SIZE, y + GRID_SIZE), Color.White, 0f, false);
        //        DebugOverlay.Text((new Vector2(x, y)).ToString(), new Vector2(x + 0.1f, y + 0.1f));
        //    }
        //}

        DebugOverlay.Line(BOUNDS_MIN, new Vector2(BOUNDS_MAX.x, BOUNDS_MIN.y), 0f, false);
		DebugOverlay.Line(BOUNDS_MIN, new Vector2(BOUNDS_MIN.x, BOUNDS_MAX.y), 0f, false);
		DebugOverlay.Line(BOUNDS_MAX, new Vector2(BOUNDS_MAX.x, BOUNDS_MIN.y), 0f, false);
		DebugOverlay.Line(BOUNDS_MAX, new Vector2(BOUNDS_MIN.x, BOUNDS_MAX.y), 0f, false);
	}

	void HandleCollisionForGridSquare(Enemy enemy, (int, int) gridSquare, float dt)
    {
		if (!_enemyGridPositions.ContainsKey(gridSquare))
			return;

		//Log.Info("HandleCollisionForGridSquare - " + _enemyGridPositions[gridSquare].Count);
		foreach(Enemy other in _enemyGridPositions[gridSquare])
        {
			if (other == enemy)
				continue;

			var dist_sqr = (enemy.Position - other.Position).LengthSquared;
			var total_radius_sqr = MathF.Pow(enemy.Radius + other.Radius, 2f);
			if (dist_sqr < total_radius_sqr)
			{
				enemy.Velocity += (enemy.Position - other.Position).Normal * Utils.Map(dist_sqr, total_radius_sqr, 0f, 0f, 10f) * dt;
			}
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

	public (int x, int y) GetGridSquareForPos(Vector2 pos)
    {
		return ((int)MathF.Floor(pos.x), (int)MathF.Floor(pos.y));
    }

	public List<Enemy> GetEnemiesInGridSquare((int, int) gridSquare)
    {
		if(_enemyGridPositions.ContainsKey(gridSquare))
        {
			return _enemyGridPositions[gridSquare];
        }

		return null;
    }

	public bool IsGridSquareInArena((int, int) gridSquare)
    {
		return _enemyGridPositions.ContainsKey(gridSquare);
    }

	public void RemoveEnemy(Enemy enemy)
    {
		_enemies.Remove(enemy);

		if (_enemyGridPositions.ContainsKey(enemy.GridPos))
		{
			_enemyGridPositions[enemy.GridPos].Remove(enemy);
		}
	}
}
