﻿using Sandbox;
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

	public readonly List<PlayerCitizen> PlayerList = new();

	public int EnemyCount { get; private set; }
	public const float MAX_ENEMY_COUNT = 750;

	private readonly List<Thing> _things = new();
	public record struct GridSquare(int x, int y);
	public Dictionary<GridSquare, List<Thing>> ThingGridPositions = new Dictionary<GridSquare, List<Thing>>();

	public float GRID_SIZE = 1f;
	public Vector2 BOUNDS_MIN = new Vector2(-16f, -12f);
	public Vector2 BOUNDS_MAX = new Vector2(16f, 12f);

	private TimeSince _enemySpawnTime;

	public MyGame()
	{
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

            _ = new Background()
			{
				Position = Vector2.Zero
			};

			//var animTest = new Sprite
			//{
			//	SpriteTexture = SpriteTexture.Atlas("textures/sprites/tile_test.png", 4, 4),
			//	AnimationPath = "textures/sprites/tile_test.frames",

			//	Filter = SpriteFilter.Pixelated,
			//	Scale = 8f
			//};
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

        DebugOverlay.Line(BOUNDS_MIN, new Vector2(BOUNDS_MAX.x, BOUNDS_MIN.y), 0f, false);
		DebugOverlay.Line(BOUNDS_MIN, new Vector2(BOUNDS_MIN.x, BOUNDS_MAX.y), 0f, false);
		DebugOverlay.Line(BOUNDS_MAX, new Vector2(BOUNDS_MAX.x, BOUNDS_MIN.y), 0f, false);
		DebugOverlay.Line(BOUNDS_MAX, new Vector2(BOUNDS_MIN.x, BOUNDS_MAX.y), 0f, false);

		HandleEnemySpawn();
	}

	void HandleEnemySpawn()
    {
		if(_enemySpawnTime > 0.1f)
        {
			SpawnEnemy();
			_enemySpawnTime = 0f;
        }
    }

	void SpawnEnemy()
    {
		if (EnemyCount >= MAX_ENEMY_COUNT)
			return;

		var pos = new Vector2(Rand.Float(-18f, 18f), Rand.Float(-14f, 14f));

		var enemy = new Enemy
		{
			Position = pos,
			//Depth = Rand.Float(-128f, 128f),
		};

		AddThing(enemy);
		EnemyCount++;
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

			var other = things[i];
			if (other == thing || other.IsRemoved)
				continue;

			var dist_sqr = (thing.Position - other.Position).LengthSquared;
			var total_radius_sqr = MathF.Pow(thing.Radius + other.Radius, 2f);
			if (dist_sqr < total_radius_sqr)
			{
				float percent = Utils.Map(dist_sqr, total_radius_sqr, 0f, 0f, 1f);
				//thing.Velocity += (thing.Position - other.Position).Normal * Utils.Map(dist_sqr, total_radius_sqr, 0f, 0f, 10f) * (1f + other.TempWeight) * dt;
				thing.Collide(other, percent, dt);
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
	}
}
