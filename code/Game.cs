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
public partial class MyGame : GameManager
{
	public new static MyGame Current => GameManager.Current as MyGame;
	
	public HUD Hud { get; private set; }

	public PlayerCitizen LocalPlayer => Sandbox.Game.LocalClient.Pawn as PlayerCitizen; // ONLY FOR CLIENT USE
	
	public readonly List<PlayerCitizen> PlayerList = new();

	public int EnemyCount { get; private set; }
	public const float MAX_ENEMY_COUNT = 350;
	//public const float MAX_ENEMY_COUNT = 30;

	public int CoinCount { get; private set; }
	public const float MAX_COIN_COUNT = 200;

	private readonly List<Thing> _things = new();
	public record struct GridSquare(int x, int y);
	public Dictionary<GridSquare, List<Thing>> ThingGridPositions = new Dictionary<GridSquare, List<Thing>>();

	public float GRID_SIZE = 1f;
	public Vector2 BOUNDS_MIN;
	public Vector2 BOUNDS_MAX;
	public Vector2 BOUNDS_MIN_SPAWN;
	public Vector2 BOUNDS_MAX_SPAWN;

	private TimeSince _enemySpawnTime;

	public TimeSince ElapsedTime { get; set; }

	[Net] public bool IsGameOver { get; private set; }

	public StatusManager StatusManager { get; private set; }

	public BackgroundManager BackgroundManager { get; private set; } // client only

	public List<Sprite> _bloodSplatters;
	public List<Sprite> _clouds;
	public List<Sprite> _explosions;

	public bool HasSpawnedBoss { get; private set; }

	public MyGame()
	{
		BOUNDS_MIN = new Vector2(-16f, -12f);
		BOUNDS_MAX = new Vector2(16f, 12f);
		BOUNDS_MIN_SPAWN = new Vector2(-15.5f, -11.5f);
		BOUNDS_MAX_SPAWN = new Vector2(15.5f, 11.5f);

        if ( Sandbox.Game.IsServer )
		{
			for (float x = BOUNDS_MIN.x; x < BOUNDS_MAX.x; x += GRID_SIZE)
			{
				for (float y = BOUNDS_MIN.y; y < BOUNDS_MAX.y; y += GRID_SIZE)
				{
					ThingGridPositions.Add(GetGridSquareForPos(new Vector2(x, y)), new List<Thing>());
				}
			}

			ElapsedTime = 0f;
			StatusManager = new StatusManager();

			SpawnStartingThings();
		}

		if (Sandbox.Game.IsClient)
		{
			//_ = new MainHud();
			Hud = new HUD();
			BackgroundManager = new BackgroundManager();

			_bloodSplatters = new List<Sprite>();
			_clouds = new List<Sprite>();
			_explosions = new List<Sprite>();
		}
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

        Camera2D.Current = new Camera2D();

        BackgroundManager?.Restart();
	}

	public void SpawnStartingThings()
	{
		for(int i = 0; i < 3; i++)
		{
			var pos = new Vector2(Sandbox.Game.Random.Float(BOUNDS_MIN_SPAWN.x, BOUNDS_MAX_SPAWN.x), Sandbox.Game.Random.Float(BOUNDS_MIN_SPAWN.y, BOUNDS_MAX_SPAWN.y));
			SpawnCrate(pos);
		}

		//SpawnBoss(new Vector2(3, 3f));
	}

	[Event.Tick.Server]
	public void ServerTick()
	{
		if (IsGameOver)
			return;

		var dt = Time.Delta;

		for(int i = _things.Count - 1; i >= 0; i--)
		{
			var thing = _things[i];
			if (!thing.IsRemoved)
				thing.Update(dt * thing.TimeScale);
		}

		//DebugOverlay.Line(BOUNDS_MIN, new Vector2(BOUNDS_MAX.x, BOUNDS_MIN.y), 0f, false);
		//DebugOverlay.Line(BOUNDS_MIN, new Vector2(BOUNDS_MIN.x, BOUNDS_MAX.y), 0f, false);
		//DebugOverlay.Line(BOUNDS_MAX, new Vector2(BOUNDS_MAX.x, BOUNDS_MIN.y), 0f, false);
		//DebugOverlay.Line(BOUNDS_MAX, new Vector2(BOUNDS_MIN.x, BOUNDS_MAX.y), 0f, false);

		HandleEnemySpawn();

		if(!HasSpawnedBoss && !IsGameOver && ElapsedTime > 15f * 60f - 1f)
		{
			SpawnBoss(new Vector2(0f, 0f));
			HasSpawnedBoss = true;
        }
	}

	void HandleEnemySpawn()
	{
		var spawnTime = Utils.Map(EnemyCount, 0, MAX_ENEMY_COUNT, 0.05f, 0.33f, EasingType.QuadOut) * Utils.Map(ElapsedTime, 0f, 80f, 1.5f, 1f) * Utils.Map(ElapsedTime, 0f, 250f, 3f, 1f);
		if (_enemySpawnTime > spawnTime)
		{
			SpawnEnemy();
			_enemySpawnTime = 0f;
		}
	}

	void SpawnEnemy()
	{
		if (EnemyCount >= MAX_ENEMY_COUNT)
			return;

		var pos = new Vector2(Sandbox.Game.Random.Float(BOUNDS_MIN_SPAWN.x, BOUNDS_MAX_SPAWN.x), Sandbox.Game.Random.Float(BOUNDS_MIN_SPAWN.y, BOUNDS_MAX_SPAWN.y));

		TypeDescription type = TypeLibrary.GetType(typeof(Zombie));

		float crateChance = ElapsedTime < 20f ? 0f : Utils.Map(ElapsedTime, 20f, 200f, 0.005f, 0.01f);
		if (type == TypeLibrary.GetType(typeof(Zombie)) && Sandbox.Game.Random.Float(0f, 1f) < crateChance)
			type = TypeLibrary.GetType(typeof(Crate));

		// EXPLODER
		float exploderChance = ElapsedTime < 35f ? 0f : Utils.Map(ElapsedTime, 35f, 700f, 0.03f, 0.08f);
		if (type == TypeLibrary.GetType(typeof(Zombie)) && Sandbox.Game.Random.Float(0f, 1f) < exploderChance)
		{
			float eliteChance = ElapsedTime < 480f ? 0f : Utils.Map(ElapsedTime, 480f, 1200f, 0.05f, 1f);
            type = Sandbox.Game.Random.Float(0f, 1f) < eliteChance ? TypeLibrary.GetType(typeof(ExploderElite)) : TypeLibrary.GetType(typeof(Exploder));
        }

        // SPITTER
        float spitterChance = ElapsedTime < 90f ? 0f : Utils.Map(ElapsedTime, 90f, 800f, 0.02f, 0.1f);
        if (type == TypeLibrary.GetType(typeof(Zombie)) && Sandbox.Game.Random.Float(0f, 1f) < spitterChance)
        {
            float eliteChance = ElapsedTime < 540f ? 0f : Utils.Map(ElapsedTime, 540f, 1400f, 0.05f, 1f);
            type = Sandbox.Game.Random.Float(0f, 1f) < eliteChance ? TypeLibrary.GetType(typeof(SpitterElite)) : TypeLibrary.GetType(typeof(Spitter));
        }

        // SPIKER
        float spikerChance = ElapsedTime < 320f ? 0f : Utils.Map(ElapsedTime, 320f, 800f, 0.02f, 0.1f, EasingType.SineIn);
        if (type == TypeLibrary.GetType(typeof(Zombie)) && Sandbox.Game.Random.Float(0f, 1f) < spikerChance)
        {
            float eliteChance = ElapsedTime < 640f ? 0f : Utils.Map(ElapsedTime, 640f, 1500f, 0.04f, 0.75f);
            type = Sandbox.Game.Random.Float(0f, 1f) < eliteChance ? TypeLibrary.GetType(typeof(SpikerElite)) : TypeLibrary.GetType(typeof(Spiker));
        }

        // CHARGER
        float chargerChance = ElapsedTime < 420f ? 0f : Utils.Map(ElapsedTime, 420f, 800f, 0.03f, 0.075f);
        if (type == TypeLibrary.GetType(typeof(Zombie)) && Sandbox.Game.Random.Float(0f, 1f) < chargerChance)
        {
            float eliteChance = ElapsedTime < 740f ? 0f : Utils.Map(ElapsedTime, 740f, 1500f, 0.04f, 0.75f);
            type = Sandbox.Game.Random.Float(0f, 1f) < eliteChance ? TypeLibrary.GetType(typeof(ChargerElite)) : TypeLibrary.GetType(typeof(Charger));
        }
        
        // RUNNER
        float runnerChance = ElapsedTime < 500f ? 0f : Utils.Map(ElapsedTime, 500f, 800f, 0.05f, 0.15f, EasingType.QuadIn);
		if (type == TypeLibrary.GetType(typeof(Zombie)) && Sandbox.Game.Random.Float(0f, 1f) < runnerChance)
			type = TypeLibrary.GetType(typeof(Runner));


		var zombieEliteChance = ElapsedTime < 380f ? 0f : Utils.Map(ElapsedTime, 380f, 1100f, 0.05f, 1f);
        if (type == TypeLibrary.GetType(typeof(Zombie)) && Sandbox.Game.Random.Float(0f, 1f) < zombieEliteChance)
        {
            float eliteChance = ElapsedTime < 480f ? 0f : Utils.Map(ElapsedTime, 480f, 1200f, 0.05f, 1f);
			type = TypeLibrary.GetType(typeof(ZombieElite));
        }

		//type = Game.Random.Int(0, 2) == 0 ? TypeLibrary.GetType(typeof(ChargerElite)) : TypeLibrary.GetType(typeof(Charger));

        SpawnEnemy(type, pos);
	}

	void SpawnEnemy(TypeDescription type, Vector2 pos)
	{
		if (EnemyCount >= MAX_ENEMY_COUNT)
			return;

		var enemy = type.Create<Enemy>();

		enemy.Position = pos;

		var closestPlayer = GetClosestPlayer(pos);
		if (closestPlayer?.Position.x > pos.x)
			enemy.Scale = new Vector2(-1f, 1f) * enemy.ScaleFactor;

		AddThing(enemy);
		EnemyCount++;

		PlaySfxNearby("zombie.dirt", pos, pitch: Sandbox.Game.Random.Float(0.6f, 0.8f), volume: 0.7f, maxDist: 7.5f);
	}

	public Coin SpawnCoin(Vector2 pos, int value = 1)
	{
		// todo: spawn larger amounts less often if reaching max coin cap
		if (CoinCount >= MAX_COIN_COUNT)
			return null;

		var coin = new Coin()
		{
			Position = pos,
		};

		coin.SetValue(value);

		AddThing(coin);
		CoinCount++;

		return coin;
	}

	public void SpawnBoss(Vector2 pos)
	{
		SpawnEnemy(TypeLibrary.GetType(typeof(Boss)), pos);
		PlaySfxNearby("boss.fanfare", pos, pitch: Sandbox.Game.Random.Float(0.7f, 0.75f), volume: 1.3f, maxDist: 15f);
	}

	public void SpawnCrate(Vector2 pos)
	{
		SpawnEnemy(TypeLibrary.GetType(typeof(Crate)), pos);
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
			if (other == thing || other.IsRemoved || !other.IsValid)
				continue;

			bool isValidType = false;
			foreach(var t in thing.CollideWith)
			{
				if(t.IsAssignableFrom(other.GetType()))
				{
					isValidType = true;
					break;
				}
			}

			if (!isValidType)
				continue;

			var dist_sqr = (thing.Position - other.Position).LengthSquared;
			var total_radius_sqr = MathF.Pow(thing.Radius + other.Radius, 2f);
			if (dist_sqr < total_radius_sqr)
			{
				float percent = Utils.Map(dist_sqr, total_radius_sqr, 0f, 0f, 1f);
				//thing.Velocity += (thing.Position - other.Position).Normal * Utils.Map(dist_sqr, total_radius_sqr, 0f, 0f, 10f) * (1f + other.TempWeight) * dt;
				thing.Colliding(other, percent, dt * thing.TimeScale);
			}
		}
	}

	public void AddThingsInGridSquare(GridSquare gridSquare, List<Thing> things)
	{
		if (!ThingGridPositions.ContainsKey(gridSquare))
			return;

		things.AddRange(ThingGridPositions[gridSquare]);
	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		var player = new PlayerCitizen();
		client.Pawn = player;

		PlayerList.Add(player);
		AddThing(player);
	}

	public IEnumerable<PlayerCitizen> Players => Sandbox.Game.Clients
		.Select(x => x.Pawn)
		.OfType<PlayerCitizen>();

	public IEnumerable<PlayerCitizen> AlivePlayers => Players
		.Where(x => !x.IsDead);

	public IEnumerable<PlayerCitizen> DeadPlayers => Players
		.Where(x => x.IsDead);

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
		MyGame.Current.PlaySfxTarget(To.Everyone, "restart", Vector2.Zero, Sandbox.Game.Random.Float(0.95f, 1.05f), 0.66f);
	}

	public void Restart()
	{
		for (int i = _things.Count - 1; i >= 0; i--)
		{
			var thing = _things[i];
			if(thing is not PlayerCitizen)
			{
				//thing.Delete();
				thing.Remove();
			}
		}

		_things.Clear();

		for (int i = PlayerList.Count - 1; i >= 0; i--)
		{
			var player = PlayerList[i];
			if(player == null || !player.IsValid)
			{
				PlayerList.Remove(player);
				continue;
			}

			player.InitializeStats();
			player.Position = new Vector2(0f + i * 0.5f, 0f);
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
		HasSpawnedBoss = false;

        SpawnStartingThings();

		RestartClient();
	}

	[ClientRpc]
	public void RestartClient()
	{
		Hud.Restart();
		BackgroundManager.Restart();
		
		foreach(var blood in _bloodSplatters)
			blood.Delete();
		_bloodSplatters.Clear();

		foreach (var cloud in _clouds)
			cloud.Delete();
		_clouds.Clear();

		foreach (var explosion in _explosions)
			explosion.Delete();
		_explosions.Clear();
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

	public void Victory()
	{
		if (IsGameOver)
			return;

		IsGameOver = true;
		VictoryClient();
	}

	[ClientRpc]
	public void VictoryClient()
	{
		Hud.Victory();
	}

	public BloodSplatter SpawnBloodSplatter(Vector2 pos)
	{
		Sandbox.Game.AssertClient();

		var bloodSplatter = new BloodSplatter()
		{
			Position = pos,
			Lifetime = Utils.Map(_bloodSplatters.Count, 0, 100, 10f, 1f) * Sandbox.Game.Random.Float(0.8f, 1.2f),
		};

		_bloodSplatters.Add(bloodSplatter);
		return bloodSplatter;
	}

	public void RemoveBloodSplatter(BloodSplatter blood)
	{
		if(_bloodSplatters.Contains(blood))
			_bloodSplatters.Remove(blood);
	}

	public Cloud SpawnCloud(Vector2 pos)
	{
		Sandbox.Game.AssertClient();

		var cloud = new Cloud()
		{
			Position = pos,
			Lifetime = 0.7f * Sandbox.Game.Random.Float(0.8f, 1.2f),
		};

		_clouds.Add(cloud);
		return cloud;
	}

	public void RemoveCloud(Cloud cloud)
	{
		if (_clouds.Contains(cloud))
			_clouds.Remove(cloud);
	}

	public ExplosionEffect SpawnExplosionEffect(Vector2 pos)
	{
		Sandbox.Game.AssertClient();

		var explosion = new ExplosionEffect()
		{
			Position = pos,
			Lifetime = 0.5f
		};

		_explosions.Add(explosion);
		return explosion;
	}

	public void RemoveExplosionEffect(ExplosionEffect explosion)
	{
		if (_explosions.Contains(explosion))
			_explosions.Remove(explosion);
	}

	public void PlaySfxNearby(string name, Vector2 worldPos, float pitch, float volume, float maxDist)
	{
		foreach (PlayerCitizen player in Players)
		{
			var playerPos = player.Position;

			var distSqr = (player.Position - worldPos).LengthSquared;
			if (distSqr < maxDist * maxDist)
			{
				var dist = (player.Position - worldPos).Length;
				var falloff = Utils.Map(dist, 0f, maxDist, 1f, 0f, EasingType.SineIn);
				var pos = playerPos + (worldPos - playerPos) * 0.1f;

				var sound = Sound.FromWorld(To.Single(player.Client), name, new Vector3(pos.x, pos.y, 512f));
				sound.SetPitch(pitch);
				sound.SetVolume(volume * falloff);
			}
		}
	}

	public void PlaySfxTarget(To toTarget, string name, Vector2 worldPos, float pitch, float volume)
	{
		var sound = Sound.FromWorld(toTarget, name, new Vector3(worldPos.x, worldPos.y, 512f));
		sound.SetPitch(pitch);
		sound.SetVolume(volume);
	}

	//[ClientRpc]
	public void PlaySfx(string name, Vector2 worldPos)
	{
		worldPos = OffsetSoundPos(worldPos);
		Sound.FromWorld(name, new Vector3(worldPos.x, worldPos.y, 512f));
	}

	////[ClientRpc]
	//public void PlaySfx(string name, Vector2 worldPos, float pitch)
	//{
	//	worldPos = OffsetSoundPos(worldPos);
	//	var sound = Sound.FromWorld(name, new Vector3(worldPos.x, worldPos.y, 512f));
	//	sound.SetPitch(pitch);
	//}

	////[ClientRpc]
	//public void PlaySfx(string name, Vector2 worldPos, float pitch, float volume)
	//{
	//	worldPos = OffsetSoundPos(worldPos);
	//	var sound = Sound.FromWorld(name, new Vector3(worldPos.x, worldPos.y, 512f));
	//	sound.SetPitch(pitch);
	//	sound.SetVolume(volume);
	//}

	Vector2 OffsetSoundPos(Vector2 worldPos)
	{
		if (Sound.Listener == null)
			return worldPos;

		Vector2 listenerPos = (Vector2)Sound.Listener.Value.Position;
		return listenerPos + (worldPos - listenerPos) * 0.1f;
	}

	[Event.Client.Frame]
    public void ClientFrame()
    {
        Camera2D.Current?.Update();
    }
}
