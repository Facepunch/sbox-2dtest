using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Test2D;

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
	
	public HUD Hud { get; private set; }

	public PlayerCitizen LocalPlayer => Local.Client.Pawn as PlayerCitizen; // ONLY FOR CLIENT USE

	public OrthoCamera MainCamera { get; } = new OrthoCamera();

	public readonly List<PlayerCitizen> PlayerList = new();

	public int EnemyCount { get; private set; }
	public const float MAX_ENEMY_COUNT = 350;
	//public const float MAX_ENEMY_COUNT = 55;

    public int CoinCount { get; private set; }
	public const float MAX_COIN_COUNT = 100;

	private readonly List<Thing> _things = new();
	public record struct GridSquare(int x, int y);
	public Dictionary<GridSquare, List<Thing>> ThingGridPositions = new Dictionary<GridSquare, List<Thing>>();

	public float GRID_SIZE = 1f;
	public Vector2 BOUNDS_MIN;
	public Vector2 BOUNDS_MAX;

	private TimeSince _enemySpawnTime;

	public TimeSince ElapsedTime { get; set; }

	[Net] public bool IsGameOver { get; private set; }

	public StatusManager StatusManager { get; private set; }

	public BackgroundManager BackgroundManager { get; private set; } // client only

	public MyGame()
	{
		BOUNDS_MIN = new Vector2(-16f, -12f);
		BOUNDS_MAX = new Vector2(16f, 12f);

		if ( Host.IsServer )
		{
			for (float x = BOUNDS_MIN.x; x < BOUNDS_MAX.x; x += GRID_SIZE)
			{
				for (float y = BOUNDS_MIN.y; y < BOUNDS_MAX.y; y += GRID_SIZE)
				{
					ThingGridPositions.Add(GetGridSquareForPos(new Vector2(x, y)), new List<Thing>());
				}
			}

			//for (var i = 0; i < 750; ++i)
			for (var i = 0; i < 5; ++i)
			{
                SpawnEnemy();
            }


   //         _ = new BackgroundTile()
			//{
			//	Position = Vector2.Zero
			//};

			//var animTest = new Sprite
			//         {
			//             SpriteTexture = SpriteTexture.Atlas("textures/sprites/tile_test.png", 4, 4),
			//             AnimationPath = "textures/sprites/tile_test.frames",

			//             Filter = SpriteFilter.Pixelated,
			//             Scale = 4f
			//         };

			ElapsedTime = 0f;
			StatusManager = new StatusManager();
		}

		if (Host.IsClient)
        {
			//_ = new MainHud();
            Hud = new HUD();
			BackgroundManager = new BackgroundManager();
		}
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		BackgroundManager?.Restart();
    }

	[Event.Tick.Server]
	public void ServerTick()
    {
		var dt = Time.Delta;

		for(int i = _things.Count - 1; i >= 0; i--)
        {
			var thing = _things[i];
			if (!thing.IsRemoved)
				thing.Update(dt);
		}

			//for (float x = BOUNDS_MIN.x; x < BOUNDS_MAX.x; x += GRID_SIZE)
			//{
			//    for (float y = BOUNDS_MIN.y; y < BOUNDS_MAX.y; y += GRID_SIZE)
			//    {
			//        DebugOverlay.Box(new Vector2(x, y), new Vector2(x + GRID_SIZE, y + GRID_SIZE), Color.White, 0f, false);
			//        DebugOverlay.Text((new Vector2(x, y)).ToString(), new Vector2(x + 0.1f, y + 0.1f));
			//    }
			//}

			//      DebugOverlay.Line(BOUNDS_MIN, new Vector2(BOUNDS_MAX.x, BOUNDS_MIN.y), 0f, false);
			//DebugOverlay.Line(BOUNDS_MIN, new Vector2(BOUNDS_MIN.x, BOUNDS_MAX.y), 0f, false);
			//DebugOverlay.Line(BOUNDS_MAX, new Vector2(BOUNDS_MAX.x, BOUNDS_MIN.y), 0f, false);
			//DebugOverlay.Line(BOUNDS_MAX, new Vector2(BOUNDS_MIN.x, BOUNDS_MAX.y), 0f, false);

			HandleEnemySpawn();
	}

	void HandleEnemySpawn()
    {
		if(_enemySpawnTime > Utils.Map(EnemyCount, 0, MAX_ENEMY_COUNT, 0.05f, 0.25f, EasingType.QuadOut) * Utils.Map(ElapsedTime, 0f, 60f, 3f, 1f))
        {
			SpawnEnemy();
			_enemySpawnTime = 0f;
        }
    }

	void SpawnEnemy()
    {
		if (EnemyCount >= MAX_ENEMY_COUNT)
			return;

		var pos = new Vector2(Rand.Float(BOUNDS_MIN.x, BOUNDS_MAX.x), Rand.Float(BOUNDS_MIN.y, BOUNDS_MAX.y));

		var enemy = new Enemy
		{
			Position = pos,
			//Depth = Rand.Float(-128f, 128f),
		};

		var closestPlayer = GetClosestPlayer(pos);
		if (closestPlayer?.Position.x > pos.x)
			enemy.Scale = new Vector2(-1f, 1f) * Enemy.SCALE_FACTOR;

		AddThing(enemy);
		EnemyCount++;
	}

	public void SpawnCoin(Vector2 pos)
    {
		if (CoinCount >= MAX_ENEMY_COUNT)
			return;

		var coin = new Coin()
		{
			Position = pos,
		};

		AddThing(coin);
		CoinCount++;
	}

	public void HandleThingCollisionForGridSquare(Thing thing, GridSquare gridSquare, float dt)
	{
		if (!ThingGridPositions.ContainsKey(gridSquare))
			return;

		var things = ThingGridPositions[gridSquare];
		if (things.Count == 0)
			return;

		for(int i = things.Count - 1; i >= 0; i--)
        {
			if (i >= things.Count)
				continue;
			//Log.Info("!!! " + thing.Name + " --- " + i.ToString() + " count: " + things.Count);

			if (thing == null || !thing.IsValid || thing.IsRemoved)
				return;

			var other = things[i];
			if (other == thing || other.IsRemoved || !other.IsValid || !thing.CollideWith.Contains(other.GetType()))
				continue;

			var dist_sqr = (thing.Position - other.Position).LengthSquared;
			var total_radius_sqr = MathF.Pow(thing.Radius + other.Radius, 2f);
			if (dist_sqr < total_radius_sqr)
			{
				float percent = Utils.Map(dist_sqr, total_radius_sqr, 0f, 0f, 1f);
				//thing.Velocity += (thing.Position - other.Position).Normal * Utils.Map(dist_sqr, total_radius_sqr, 0f, 0f, 10f) * (1f + other.TempWeight) * dt;
				thing.Colliding(other, percent, dt);
			}
		}
	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		Log.Info("ClientJoined");

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

		PlayerList.Add(player);
		AddThing(player);
	}

	public override CameraMode FindActiveCamera()
	{
		return MainCamera;
	}

	public IEnumerable<PlayerCitizen> Players => Client.All
		.Select(x => x.Pawn)
		.OfType<PlayerCitizen>();

	public IEnumerable<PlayerCitizen> AlivePlayers => Players
		.Where(x => !x.IsDead);

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

	public GridSquare GetGridSquareForPos(Vector2 pos)
    {
		return new GridSquare((int)MathF.Floor(pos.x), (int)MathF.Floor(pos.y));
    }

	public List<Thing> GetThingsInGridSquare(GridSquare gridSquare)
    {
		if(ThingGridPositions.ContainsKey(gridSquare))
        {
			return ThingGridPositions[gridSquare];
        }

		return null;
    }

	public bool IsGridSquareInArena(GridSquare gridSquare)
    {
		return ThingGridPositions.ContainsKey(gridSquare);
    }

	public void RegisterThingGridSquare(Thing thing, GridSquare gridSquare)
	{
		if (IsGridSquareInArena(gridSquare))
			ThingGridPositions[gridSquare].Add(thing);
	}

	public void DeregisterThingGridSquare(Thing thing, GridSquare gridSquare)
	{
		if (ThingGridPositions.ContainsKey(gridSquare) && ThingGridPositions[gridSquare].Contains(thing))
		{
			ThingGridPositions[gridSquare].Remove(thing);
		}
	}

	public void AddThing(Thing thing)
    {
		_things.Add(thing);
		thing.GridPos = GetGridSquareForPos(thing.Position);
		RegisterThingGridSquare(thing, thing.GridPos);
	}

	public void RemoveThing(Thing thing)
    {
		if (ThingGridPositions.ContainsKey(thing.GridPos))
		{
			ThingGridPositions[thing.GridPos].Remove(thing);
		}

		if (thing is Enemy)
			EnemyCount--;
		else if (thing is Coin)
			CoinCount--;
	}

	[ConCmd.Server]
	public static void RestartCmd()
	{
		Current.Restart();
	}
	public void Restart()
	{
		for (int i = _things.Count - 1; i >= 0; i--)
		{
			var thing = _things[i];
			if(thing is not PlayerCitizen)
            {
				thing.Delete();
            }
		}

		_things.Clear();

		for (int i = 0; i < PlayerList.Count; i++)
		{
			var player = PlayerList[i];
			player.InitializeStats();
			player.Position = new Vector2(Utils.Map(i, 0, PlayerList.Count - 1, -2f, 2f), 0f);
			_things.Add(player);
		}

		foreach (KeyValuePair<GridSquare, List<Thing>> pair in ThingGridPositions)
        {
			pair.Value.Clear();
        }

		EnemyCount = 0;
		CoinCount = 0;
		_enemySpawnTime = 0f;
		ElapsedTime = 0f;
		IsGameOver = false;

		RestartClient();
	}

	[ClientRpc]
	public void RestartClient()
	{
		Hud.Restart();
		BackgroundManager.Restart();
	}

	public void PlayerDied(PlayerCitizen player)
    {
		int numPlayersAlive = Players.Where(x => !x.IsDead).Count();
		if(numPlayersAlive == 0)
        {
			GameOver();
        }
	}

	public void GameOver()
    {
		if (IsGameOver)
			return;

		IsGameOver = true;
		GameOverClient();
	}

	[ClientRpc]
	public void GameOverClient()
	{
		Hud.GameOver();
	}
}
