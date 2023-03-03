using System;
using System.Collections.Generic;
using System.Linq;
using static Test2D.MyGame;
using Sandbox;
using Sandbox.Diagnostics;

namespace Test2D;

public abstract partial class Enemy : Thing
{
	public float MoveTimeOffset { get; set; }
	//public float MoveTimeSpeed { get; set; }

	private float _flashTimer;
	private bool _isFlashing;

	[Net] public float MaxHealth { get; protected set; }

	public bool IsSpawning { get; private set; }
	public float ElapsedTime { get; private set; }
	public bool IsDying { get; private set; }
	public float DeathTimeElapsed { get; private set; }
	public float DeathTime { get; protected set; }
	[Net] public float DeathProgress { get; private set; }
	private Vector2 _deathScale;

	public bool IsAttacking { get; private set; }
	private float _aggroTimer;
	public bool CanAttack { get; set; }
	public bool CanAttackAnim { get; set; }
	public bool CanTurn { get; set; }
	public bool CanBleedClient { get; set; }
	public float AggroRange { get; protected set; }
	protected const float AGGRO_START_TIME = 0.2f;
	protected const float AGGRO_LOSE_TIME = 0.4f;

	public float DamageToPlayer { get; protected set; }

	public float ScaleFactor { get; protected set; }
	public float PushStrength { get; protected set; }

	public float SpawnTime { get; protected set; }
	public float ShadowFullOpacity { get; protected set; }

	public string AnimSpawnPath { get; protected set; }
	public string AnimIdlePath { get; protected set; }
	public string AnimAttackPath { get; protected set; }
	public string AnimDiePath { get; protected set; }

	public float Deceleration { get; protected set; }
	public float DecelerationAttacking { get; protected set; }

	private TimeSince _spawnCloudTime;

	public Dictionary<TypeDescription, EnemyStatus> EnemyStatuses = new Dictionary<TypeDescription, EnemyStatus>();

	private BurningVfx _burningVfx;
	private FrozenVfx _frozenVfx;
    private FearVfx _fearVfx;
    public bool IsFrozen { get; set; }
    public bool IsFeared { get; set; }

    private float _animSpeed;
	public float AnimSpeed { get { return _animSpeed; } set { _animSpeed = value; AnimationSpeed = _animSpeed * _animSpeedModifier; } }
	private float _animSpeedModifier;
	public float AnimSpeedModifier { get { return _animSpeedModifier; } set { _animSpeedModifier = value; AnimationSpeed = _animSpeed * _animSpeedModifier; } }
	public int CoinValueMin { get; protected set; }
	public int CoinValueMax { get; protected set; }

	public override void Spawn()
	{
		base.Spawn();

		if (Sandbox.Game.IsServer)
		{
			MoveTimeOffset = Sandbox.Game.Random.Float(0f, 4f);
			IsSpawning = true;
			ElapsedTime = 0f;
			SpawnTime = 1.75f;
			Deceleration = 1.47f;
			DecelerationAttacking = 1.33f;
			DeathTime = 0.3f;
			AggroRange = 1.4f;
			CanAttack = true;
			CanAttackAnim = true;
			CanTurn = true;
			ShadowFullOpacity = 0.8f;

			AnimSpawnPath = "textures/sprites/zombie_spawn.frames";
			AnimIdlePath = "textures/sprites/zombie_walk.frames";
			AnimAttackPath = "textures/sprites/zombie_attack.frames";
			AnimDiePath = "textures/sprites/zombie_die.frames";

			_animSpeed = 1f;
			_animSpeedModifier = 1f;

			CoinValueMin = 1;
			CoinValueMax = 1;

			//ColorTint = new Color(Sandbox.Game.Random.Float(0.45f, 1f), Sandbox.Game.Random.Float(0.45f, 1f), Sandbox.Game.Random.Float(0.45f, 1f));
		}

		Filter = SpriteFilter.Pixelated;
		ColorFill = new ColorHsv(0f, 0f, 0f, 0f);
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		CanBleedClient = true;

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

		HandleFlashing(Time.Delta);
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
		float attack_dist_sqr = MathF.Pow(AggroRange, 2f);

		if (!IsAttacking)
		{
			if(CanAttack)
			{
				if (dist_sqr < attack_dist_sqr)
				{
					_aggroTimer += dt;
					if (_aggroTimer > AGGRO_START_TIME)
					{
						StartAttacking();
						_aggroTimer = 0f;
					}
				}
				else
				{
					_aggroTimer = 0f;
				}
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

					if (CanAttackAnim)
						AnimationPath = AnimIdlePath;
				}
			}
			else
			{
				AnimSpeed = Utils.Map(dist_sqr, attack_dist_sqr, 0f, 1f, 4f, EasingType.Linear);
				_aggroTimer = 0f;
			}
		}
	}

	public virtual void StartAttacking()
	{
		IsAttacking = true;

		if(CanAttackAnim)
			AnimationPath = AnimAttackPath;
	}

	protected virtual void UpdateSprite(PlayerCitizen targetPlayer)
	{
		if(!IsAttacking)
		{
			AnimSpeed = Utils.Map(Utils.FastSin(MoveTimeOffset + Time.Now * 7.5f), -1f, 1f, 0.75f, 3f, EasingType.ExpoIn);

			if (MathF.Abs(Velocity.x) > 0.175f && !IsFrozen && CanTurn)
				Scale = new Vector2(1f * Velocity.x < 0f ? 1f : -1f, 1f) * ScaleFactor;
		}
		else
		{
			float dist_sqr = (targetPlayer.Position - Position).LengthSquared;
			float attack_dist_sqr = MathF.Pow(AggroRange, 2f);
			AnimSpeed = Utils.Map(dist_sqr, attack_dist_sqr, 0f, 1f, 4f, EasingType.Linear);

			if(!IsFrozen && CanTurn)
				Scale = new Vector2((IsFeared ? -1f : 1f) * (targetPlayer.Position.x < Position.x ? 1f : -1f), 1f) * ScaleFactor;
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
			ShadowOpacity = Utils.Map(DeathProgress, 0f, 1f, ShadowFullOpacity, 0f, EasingType.QuadIn);
		}
	}

	void HandleSpawning(float dt)
	{
		Depth = -Position.y * 10f;

		if (ElapsedTime > SpawnTime)
		{
			IsSpawning = false;
			AnimationPath = AnimIdlePath;

			ShadowOpacity = ShadowFullOpacity;
		}
		else
		{
			if(_spawnCloudTime > (0.3f / TimeScale))
			{
				SpawnCloudClient(Position + new Vector2(0f, 0.25f), new Vector2(Sandbox.Game.Random.Float(-1f, 1f), Sandbox.Game.Random.Float(-1f, 1f)).Normal * Sandbox.Game.Random.Float(0.2f, 0.6f));
				_spawnCloudTime = Sandbox.Game.Random.Float(0f, 0.15f);
			}

			ShadowOpacity = Utils.Map(ElapsedTime, 0f, SpawnTime, 0f, ShadowFullOpacity);
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

		if(player != null)
		{
            player.ForEachStatus(status => status.OnHit(this, isCrit));

            if (IsFeared)
			{
                damage *= player.Stats[PlayerStat.FearDamageMultiplier];

				if(player.Stats[PlayerStat.FearDrainPercent] > 0f)
					player.RegenHealth(damage * player.Stats[PlayerStat.FearDrainPercent]);
            }
        }
		
		Health -= damage;
		DamageNumbers.Create(Position + new Vector2(Sandbox.Game.Random.Float(1.25f, 2.55f), Sandbox.Game.Random.Float(4f, 8f)) * 0.1f, damage, isCrit ? DamageNumberType.Crit : DamageNumberType.Normal);

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

	public virtual void DamageFire(float damage, PlayerCitizen player)
	{
		if (IsFrozen)
			damage *= player.Stats[PlayerStat.FreezeFireDamageMultiplier];

		Damage(damage, player);
	}

	public virtual void StartDying(PlayerCitizen player)
	{
		IsDying = true;
		DeathProgress = 0f;
		DeathTimeElapsed = 0f;
		AnimationPath = AnimDiePath;
		AnimSpeed = 5.5f;

		_isFlashing = false;
		//ColorFill = new ColorHsv(0f, 0f, 0f, 0f);

		_deathScale = Scale;

		if(player != null)
			player.ForEachStatus(status => status.OnKill(this));

        DropLoot(player);

		for (int i = EnemyStatuses.Count - 1; i >= 0; i--)
			EnemyStatuses.Values.ElementAt(i).StartDying();

		Game.PlaySfxNearby("enemy.die", Position, pitch: 1f, volume: 1f, maxDist: 5.5f);
		StartDyingClient();
	}

	public virtual void DropLoot(PlayerCitizen player)
	{
		var coin_chance = player != null ? Utils.Map(player.Stats[PlayerStat.Luck], 0f, 10f, 0.5f, 1f) : 0.5f;
		if (Sandbox.Game.Random.Float(0f, 1f) < coin_chance)
		{
			Game.SpawnCoin(Position, Sandbox.Game.Random.Int(CoinValueMin, CoinValueMax));
		}
		else
		{
			var lowest_hp_percent = 1f;
			foreach (PlayerCitizen p in Game.AlivePlayers)
				lowest_hp_percent = MathF.Min(lowest_hp_percent, p.Health / p.Stats[PlayerStat.MaxHp]);

			var health_pack_chance = Utils.Map(lowest_hp_percent, 1f, 0f, 0f, 0.1f);
			if (Sandbox.Game.Random.Float(0f, 1f) < health_pack_chance)
			{
				var healthPack = new HealthPack() { Position = Position };
				Game.AddThing(healthPack);
			}
		}
	}

	[ClientRpc]
	public virtual void StartDyingClient()
	{
		if(CanBleedClient)
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

	public TStatus AddEnemyStatus<TStatus>()
		where TStatus : EnemyStatus
    {
        var type = TypeLibrary.GetType<TStatus>();

		if(EnemyStatuses.TryGetValue( type, out var status ) )
		{
            status.Refresh();
			return (TStatus) status;
		}
		else
		{
			status = type.Create<EnemyStatus>();
			EnemyStatuses.Add(type, status);
            status.Init(this);
			return (TStatus) status;
		}
	}

    public void RemoveEnemyStatus<TStatus>( TStatus status )
        where TStatus : EnemyStatus
    {
        if ( EnemyStatuses.Remove( TypeLibrary.GetType<TStatus>(), out var existing ) )
        {
			Assert.AreEqual( existing, status );
            status.Remove();
        }
    }

    private void RemoveEnemyStatus<TStatus>()
        where TStatus : EnemyStatus
    {
        if ( EnemyStatuses.Remove( TypeLibrary.GetType<TStatus>(), out var status ) )
        {
			status.Remove();
        }
	}

	private TStatus GetEnemyStatus<TStatus>()
	    where TStatus : EnemyStatus
    {
        return EnemyStatuses.TryGetValue( TypeLibrary.GetType<TStatus>(), out var status )
            ? (TStatus)status
            : null;
    }

    public bool HasEnemyStatus<TStatus>(TStatus status)
        where TStatus : EnemyStatus
    {
        return EnemyStatuses.TryGetValue( TypeLibrary.GetType<TStatus>(), out var existing ) && existing == status;
    }

    public bool HasEnemyStatus<TStatus>()
        where TStatus : EnemyStatus
    {
        return EnemyStatuses.ContainsKey( TypeLibrary.GetType<TStatus>() );
    }

	[ClientRpc]
	public void CreateBurningVfx()
	{
		_burningVfx = new BurningVfx(this);
	}

	[ClientRpc]
	public void RemoveBurningVfx()
	{
		if(_burningVfx != null)
		{
			_burningVfx.Delete();
			_burningVfx = null;
		}
	}

	[ClientRpc]
	public void CreateFrozenVfx()
	{
		_frozenVfx = new FrozenVfx(this);
	}

	[ClientRpc]
	public void RemoveFrozenVfx()
	{
		if (_frozenVfx != null)
		{
			_frozenVfx.Delete();
			_frozenVfx = null;
		}
	}

    [ClientRpc]
    public void CreateFearVfx()
    {
        _fearVfx = new FearVfx(this);
    }

    [ClientRpc]
    public void RemoveFearVfx()
    {
        if (_fearVfx != null)
        {
            _fearVfx.Delete();
            _fearVfx = null;
        }
    }

    public void Burn(PlayerCitizen player, float damage, float lifetime, float spreadChance)
    {
        var burning = AddEnemyStatus<BurningEnemyStatus>();
        burning.Player = player;
		burning.Damage = damage;
		burning.Lifetime = lifetime;
		burning.SpreadChance = spreadChance;

		if(player != null)
			player.ForEachStatus(status => status.OnBurn(this));
    }

	public void Freeze(PlayerCitizen player)
	{
		if (IsDying)
			return;

		var frozen = AddEnemyStatus<FrozenEnemyStatus>();
		frozen.Player = player;
		frozen.SetLifetime(player.Stats[PlayerStat.FreezeLifetime]);
		frozen.SetTimeScale(player.Stats[PlayerStat.FreezeTimeScale]);

        if (player != null)
            player.ForEachStatus(status => status.OnFreeze(this));
    }

    public void Fear(PlayerCitizen player)
    {
        if (IsDying)
            return;

        var fear = AddEnemyStatus<FearEnemyStatus>();
        fear.Player = player;
        fear.SetLifetime(player.Stats[PlayerStat.FearLifetime]);

		if (player.Stats[PlayerStat.FearPainPercent] > 0f)
			fear.PainPercent = player.Stats[PlayerStat.FearPainPercent];

        if (player != null)
            player.ForEachStatus(status => status.OnFear(this));
    }

	protected virtual void OnDamagePlayer(PlayerCitizen player, float damage)
	{
		if (player.Stats[PlayerStat.ThornsPercent] > 0f)
			Damage(damage * player.Stats[PlayerStat.ThornsPercent] * player.GetDamageMultiplier(), player, false);

		if (Sandbox.Game.Random.Float(0f, 1f) < player.Stats[PlayerStat.FreezeOnMeleeChance])
		{
			if (!HasEnemyStatus<FrozenEnemyStatus>())
				Game.PlaySfxNearby("frozen", Position, pitch: Sandbox.Game.Random.Float(1.1f, 1.2f), volume: 1.5f, maxDist: 5f);

			Freeze(player);
		}

        if (Sandbox.Game.Random.Float(0f, 1f) < player.Stats[PlayerStat.FearOnMeleeChance])
        {
            if (!HasEnemyStatus<FearEnemyStatus>())
				Game.PlaySfxNearby("fear", Position, pitch: Sandbox.Game.Random.Float(0.95f, 1.05f), volume: 0.6f, maxDist: 5f);

            Fear(player);
        }
    }
}
