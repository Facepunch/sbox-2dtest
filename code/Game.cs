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
	//public const float MAX_ENEMY_COUNT = 350;
	public const float MAX_ENEMY_COUNT = 30;

    public int CoinCount { get; private set; }
	public const float MAX_COIN_COUNT = 200;

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

	public List<Sprite> _bloodSplatters;
	public List<Sprite> _clouds;
	public List<Sprite> _explosions;

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

			ElapsedTime = 0f;
			StatusManager = new StatusManager();

			SpawnBoss(new Vector2(2f, 2f));
		}

		if (Host.IsClient)
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

		BackgroundManager?.Restart();
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
	}

	void HandleEnemySpawn()
    {
		if(_enemySpawnTime > Utils.Map(EnemyCount, 0, MAX_ENEMY_COUNT, 0.05f, 0.25f, EasingType.QuadOut) * Utils.Map(ElapsedTime, 0f, 60f, 1.5f, 1f) * Utils.Map(ElapsedTime, 0f, 180f, 3f, 1f))
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

		TypeDescription type;

		Enemy enemy = null;

		if (enemy == null && Rand.Float(0f, 1f) < 0.5f)
			type = TypeLibrary.GetDescription(typeof(Runner));
		if (enemy == null && Rand.Float(0f, 1f) < 0.5f)
			type = TypeLibrary.GetDescription(typeof(Spitter));
		if (enemy == null && Rand.Float(0f, 1f) < 0.5f)
			type = TypeLibrary.GetDescription(typeof(Spiker));
		else
			type = TypeLibrary.GetDescription(typeof(Zombie));

		float exploderChance = ElapsedTime < 20f ? 0f : Utils.Map(ElapsedTime, 20f, 180f, 0.05f, 0.1f);
		if (enemy == null && Rand.Float(0f, 1f) < exploderChance)
			type = TypeLibrary.GetDescription(typeof(Exploder));

		float spitterChance = ElapsedTime < 60f ? 0f : Utils.Map(ElapsedTime, 60f, 600f, 0.05f, 0.1f);
		if (enemy == null && Rand.Float(0f, 1f) < spitterChance)
			type = TypeLibrary.GetDescription(typeof(Spitter));

		float runnerChance = ElapsedTime < 120f ? 0f : Utils.Map(ElapsedTime, 120f, 800f, 0.05f, 0.2f, EasingType.QuadIn);
		if (enemy == null && Rand.Float(0f, 1f) < runnerChance)
			type = TypeLibrary.GetDescription(typeof(Runner));

		float chargerChance = ElapsedTime < 240f ? 0f : Utils.Map(ElapsedTime, 240f, 800f, 0.05f, 0.1f);
		if (enemy == null && Rand.Float(0f, 1f) < chargerChance)
			type = TypeLibrary.GetDescription(typeof(Charger));

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

		PlaySfxNearby("zombie.dirt", pos, pitch: Rand.Float(0.6f, 0.8f), volume: 0.7f, maxDist: 7.5f);
	}

	public void SpawnCoin(Vector2 pos)
    {
		// todo: spawn larger amounts less often if reaching max coin cap
		if (CoinCount >= MAX_COIN_COUNT)
			return;

		var coin = new Coin()
		{
			Position = pos,
		};

		//coin.SetValue( 

		AddThing(coin);
		CoinCount++;
	}

	public void SpawnBoss(Vector2 pos)
    {
		SpawnEnemy(TypeLibrary.GetDescription(typeof(Boss)), pos);
		PlaySfxNearby("boss.fanfare", pos, pitch: Rand.Float(0.7f, 0.75f), volume: 1.3f, maxDist: 15f);
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
	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		var player = new PlayerCitizen();
		client.Pawn = player;

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
		MyGame.Current.PlaySfxTarget(To.Everyone, "restart", Vector2.Zero, Rand.Float(0.95f, 1.05f), 0.66f);
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

		SpawnBoss(new Vector2(2f, 2f));

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

	public BloodSplatter SpawnBloodSplatter(Vector2 pos)
    {
		Host.AssertClient();

		var bloodSplatter = new BloodSplatter()
		{
			Position = pos,
			Lifetime = Utils.Map(_bloodSplatters.Count, 0, 100, 10f, 1f) * Rand.Float(0.8f, 1.2f),
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
		Host.AssertClient();

		var cloud = new Cloud()
		{
			Position = pos,
			Lifetime = 0.7f * Rand.Float(0.8f, 1.2f),
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
		Host.AssertClient();

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
}
