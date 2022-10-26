using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public abstract partial class Enemy : Thing
{
	public float MoveTimeOffset { get; set; }
	//public float MoveTimeSpeed { get; set; }

	private float _flashTimer;
	private bool _isFlashing;

	public float MaxHealth { get; protected set; }

	[Net] public bool IsSpawning { get; private set; }
	[Net] public float ElapsedTime { get; private set; }
	[Net] public bool IsDying { get; private set; }
	[Net] public float DeathTimeElapsed { get; private set; }
	public float DeathTime { get; protected set; }
	[Net] public float DeathProgress { get; private set; }
	private Vector2 _deathScale;

	public bool IsAttacking { get; private set; }
	private float _aggroTimer;
	protected const float AGGRO_RANGE = 1.4f;
	protected const float AGGRO_START_TIME = 0.2f;
	protected const float AGGRO_LOSE_TIME = 0.4f;

	public float DamageToPlayer { get; protected set; }

	public float ScaleFactor { get; protected set; }

	public float SpawnTime { get; protected set; }
	private float SHADOW_FULL_OPACITY = 0.8f;

	public string AnimSpawnPath { get; protected set; }
	public string AnimIdlePath { get; protected set; }
	public string AnimAttackPath { get; protected set; }
	public string AnimDiePath { get; protected set; }

	public float Deceleration { get; protected set; }
	public float DecelerationAttacking { get; protected set; }

	private TimeSince _spawnCloudTime;

	public Dictionary<TypeDescription, EnemyStatus> EnemyStatuses = new Dictionary<TypeDescription, EnemyStatus>();

	public override void Spawn()
	{
		base.Spawn();

		if (Host.IsServer)
		{
			MoveTimeOffset = Rand.Float(0f, 4f);
			IsSpawning = true;
			ElapsedTime = 0f;
			SpawnTime = 1.75f;
			Deceleration = 1.47f;
			DecelerationAttacking = 1.33f;
			DeathTime = 0.3f;

			AnimSpawnPath = "textures/sprites/zombie_spawn.frames";
			AnimIdlePath = "textures/sprites/zombie_walk.frames";
			AnimAttackPath = "textures/sprites/zombie_attack.frames";
			AnimDiePath = "textures/sprites/zombie_die.frames";

			//ColorTint = new Color(Rand.Float(0.45f, 1f), Rand.Float(0.45f, 1f), Rand.Float(0.45f, 1f));
		}

		Filter = SpriteFilter.Pixelated;
		ColorFill = new ColorHsv(0f, 0f, 0f, 0f);
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		SpawnShadow(ShadowScale);
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		//DebugText(ShadowOpacity.ToString());
	}

	[Event.Tick.Server]
	public void ServerTick()
	{
		//DebugText(_damageTime.ToString());
		//DebugText(SinceSpawning.Absolute.ToString("#.##"));
	}

	public override void Update(float dt)
	{
		if (Game.IsGameOver)
			return;

		base.Update(dt);
		ElapsedTime += dt;

		HandleFlashing(dt);
		HandleStatuses(dt);

		if (IsDying)
		{
			HandleDying(dt);
			return;
		}

		if (IsSpawning)
		{
			HandleSpawning(dt);
			return;
		}

		UpdatePosition(dt);
		ClampToBounds();
		HandleDeceleration(dt);
		Depth = -Position.y * 10f;

		UpdateGridPos();
		CheckCollisions(dt);

		TempWeight *= (1f - dt * 4.7f);

		var closestPlayer = Game.GetClosestPlayer(Position);
		if (closestPlayer == null)
			return;

		HandleAttacking(closestPlayer, dt);
		UpdateSprite(closestPlayer);
	}

	protected virtual void HandleStatuses(float dt)
	{
		for (int i = EnemyStatuses.Count - 1; i >= 0; i--)
        {
			var status = EnemyStatuses.Values.ElementAt(i);
			if (status.ShouldUpdate)
				status.Update(dt);
		}
	}

	protected virtual void HandleDeceleration(float dt)
    {
		Velocity *= (1f - dt * (IsAttacking ? DecelerationAttacking : Deceleration));
	}

	protected virtual void HandleAttacking(PlayerCitizen targetPlayer, float dt)
    {
		float dist_sqr = (targetPlayer.Position - Position).LengthSquared;
		float attack_dist_sqr = MathF.Pow(AGGRO_RANGE, 2f);

		if (!IsAttacking)
		{
			if (dist_sqr < attack_dist_sqr)
			{
				_aggroTimer += dt;
				if (_aggroTimer > AGGRO_START_TIME)
				{
					IsAttacking = true;
					AnimationPath = AnimAttackPath;
					_aggroTimer = 0f;
				}
			}
			else
			{
				_aggroTimer = 0f;
			}
		}
		else
		{
			if (dist_sqr > attack_dist_sqr)
			{
				_aggroTimer += dt;
				if (_aggroTimer > AGGRO_LOSE_TIME)
				{
					IsAttacking = false;
					AnimationPath = AnimIdlePath;
				}
			}
			else
			{
				AnimationSpeed = Utils.Map(dist_sqr, attack_dist_sqr, 0f, 1f, 4f, EasingType.Linear);
				_aggroTimer = 0f;
			}
		}
	}

	protected virtual void UpdateSprite(PlayerCitizen targetPlayer)
    {
		if(!IsAttacking)
        {
			AnimationSpeed = Utils.Map(Utils.FastSin(MoveTimeOffset + Time.Now * 7.5f), -1f, 1f, 0.75f, 3f, EasingType.ExpoIn);

			if (MathF.Abs(Velocity.x) > 0.175f)
				Scale = new Vector2(1f * Velocity.x < 0f ? 1f : -1f, 1f) * ScaleFactor;
		}
		else
        {
			float dist_sqr = (targetPlayer.Position - Position).LengthSquared;
			float attack_dist_sqr = MathF.Pow(AGGRO_RANGE, 2f);
			AnimationSpeed = Utils.Map(dist_sqr, attack_dist_sqr, 0f, 1f, 4f, EasingType.Linear);

			Scale = new Vector2(1f * targetPlayer.Position.x < Position.x ? 1f : -1f, 1f) * ScaleFactor;
		}
	}

	void HandleFlashing(float dt)
	{
		if (_isFlashing)
		{
			_flashTimer -= dt;
			if (_flashTimer < 0f)
			{
				_isFlashing = false;
				ColorFill = new ColorHsv(0f, 0f, 0f, 0f);
			}
		}
	}

	void HandleDying(float dt)
	{
		DeathTimeElapsed += dt;
		Scale = _deathScale * Utils.Map(DeathTimeElapsed, 0f, DeathTime, 1f, 1.2f);

		if (DeathTimeElapsed > DeathTime)
		{
			DeathProgress = 1f;
			FinishDying();
		}
		else
		{
			DeathProgress = Utils.Map(DeathTimeElapsed, 0f, DeathTime, 0f, 1f);
			ShadowOpacity = Utils.Map(DeathProgress, 0f, 1f, SHADOW_FULL_OPACITY, 0f, EasingType.QuadIn);
		}
	}

	void HandleSpawning(float dt)
	{
		Depth = -Position.y * 10f;

		if (ElapsedTime > SpawnTime)
		{
			IsSpawning = false;
			AnimationPath = AnimIdlePath;

			ShadowOpacity = SHADOW_FULL_OPACITY;
		}
		else
		{
			if(_spawnCloudTime > 0.3f)
            {
				SpawnCloudClient(Position + new Vector2(0f, 0.25f), new Vector2(Rand.Float(-1f, 1f), Rand.Float(-1f, 1f)).Normal * Rand.Float(0.2f, 0.6f));
				_spawnCloudTime = Rand.Float(0f, 0.15f);
            }

			ShadowOpacity = Utils.Map(ElapsedTime, 0f, SpawnTime, 0f, SHADOW_FULL_OPACITY);
		}
	}

	void ClampToBounds()
	{
		var x_min = Game.BOUNDS_MIN.x + Radius / 2f;
		var x_max = Game.BOUNDS_MAX.x - Radius / 2f;
		var y_min = Game.BOUNDS_MIN.y;
		var y_max = Game.BOUNDS_MAX.y - Radius * 4.2f;
		Position = new Vector2(MathX.Clamp(Position.x, x_min, x_max), MathX.Clamp(Position.y, y_min, y_max));
	}

	protected virtual void UpdatePosition(float dt)
	{

	}

	public virtual void Damage(float damage, PlayerCitizen player, bool isCrit = false)
	{
		if (IsDying)
			return;

		Health -= damage;
		DamageNumbers.Create(Position + new Vector2(Rand.Float(1.25f, 2.55f), Rand.Float(4f, 8f)) * 0.1f, damage, isCrit ? DamageType.Crit : DamageType.Normal);

		if (Health <= 0f)
		{
			StartDying(player);
			Flash(0.05f);
		} 
		else
        {
			Flash(0.12f);
		}
	}

	public virtual void StartDying(PlayerCitizen player)
	{
		IsDying = true;
		DeathProgress = 0f;
		DeathTimeElapsed = 0f;
		AnimationPath = AnimDiePath;
		AnimationSpeed = 5.5f;

		_isFlashing = false;
		//ColorFill = new ColorHsv(0f, 0f, 0f, 0f);

		_deathScale = Scale;

		var coin_chance = player != null ? Utils.Map(player.Luck, 0f, 10f, 0.5f, 1f) : 0.5f;
		if (Rand.Float(0f, 1f) < coin_chance)
        {
			Game.SpawnCoin(Position);
		}
		else
        {
			var lowest_hp_percent = 1f;
			foreach (PlayerCitizen p in Game.AlivePlayers)
				lowest_hp_percent = MathF.Min(lowest_hp_percent, p.Health / p.MaxHp);

			var health_pack_chance = Utils.Map(lowest_hp_percent, 1f, 0f, 0f, 0.1f);
			if (Rand.Float(0f, 1f) < health_pack_chance)
			{
				var healthPack = new HealthPack() { Position = Position };
				Game.AddThing(healthPack);
			}
		}

		for (int i = EnemyStatuses.Count - 1; i >= 0; i--)
			EnemyStatuses.Values.ElementAt(i).StartDying();

		Game.PlaySfxNearby("enemy.die", Position, pitch: 1f, volume: 1f, maxDist: 5.5f);
		StartDyingClient();
	}

	[ClientRpc]
	public virtual void StartDyingClient()
	{
		Game.SpawnBloodSplatter(Position);
	}

	public virtual void FinishDying()
	{
		Remove();
	}

    public override void Remove()
    {
		for (int i = EnemyStatuses.Count - 1; i >= 0; i--)
			EnemyStatuses.Values.ElementAt(i).Remove();

		EnemyStatuses.Clear();

        base.Remove();
    }

    public void Flash(float time)
	{
		if (_isFlashing)
			return;

		ColorFill = new ColorHsv(1f, 0f, 15f, 1f);
		_isFlashing = true;
		_flashTimer = time;
	}

	void UpdateGridPos()
	{
		var gridPos = Game.GetGridSquareForPos(Position);
		if (gridPos != GridPos)
		{
			Game.DeregisterThingGridSquare(this, GridPos);
			Game.RegisterThingGridSquare(this, gridPos);
			GridPos = gridPos;
		}
	}

	void CheckCollisions(float dt)
    {
		for (int dx = -1; dx <= 1; dx++)
		{
			for (int dy = -1; dy <= 1; dy++)
			{
				Game.HandleThingCollisionForGridSquare(this, new GridSquare(GridPos.x + dx, GridPos.y + dy), dt);
			}
		}
	}

	public override void Colliding(Thing other, float percent, float dt)
	{
		for (int i = EnemyStatuses.Count - 1; i >= 0; i--)
			EnemyStatuses.Values.ElementAt(i).Colliding(other, percent, dt);
	}

	public EnemyStatus AddEnemyStatus(TypeDescription type)
	{
		if(EnemyStatuses.ContainsKey(type))
        {
			EnemyStatuses[type].Refresh();
			return EnemyStatuses[type];
        }
		else
        {
            var enemyStatus = type.Create<EnemyStatus>();
			EnemyStatuses.Add(type, enemyStatus);
			enemyStatus.Init(this);
			return enemyStatus;
		}
	}

	public void RemoveEnemyStatus(TypeDescription type)
    {
		if (EnemyStatuses.ContainsKey(type))
        {
			EnemyStatuses[type].Remove();
			EnemyStatuses.Remove(type);
		}
	}

	public EnemyStatus GetEnemyStatus(TypeDescription type)
	{
		if (EnemyStatuses.ContainsKey(type))
			return EnemyStatuses[type];

		return null;
	}


	public bool HasEnemyStatus(TypeDescription type)
    {
		return EnemyStatuses.ContainsKey(type);
    }
}
