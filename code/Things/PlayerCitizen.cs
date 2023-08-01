using Sandbox;
using System;
using System.Diagnostics;
using System.Linq;
using static Test2D.MyGame;
using System.Collections.Generic;
using Sandbox.Diagnostics;
using System.Numerics;

namespace Test2D;

public enum ModifierType { Set, Add, Mult }

public class ModifierData
{
	public float value;
	public ModifierType type;
	public float priority;

	public ModifierData(float _value, ModifierType _type, float _priority = 0f)
	{
		value = _value;
		type = _type;
		priority = _priority;
	}
}

public enum PlayerStat { 
	AttackTime, AttackSpeed, ReloadTime, ReloadSpeed, MaxAmmoCount, BulletDamage, BulletForce, Recoil, MoveSpeed, NumProjectiles, BulletSpread, BulletInaccuracy, BulletSpeed, BulletLifetime,
    BulletNumPiercing, CritChance, CritMultiplier, LowHealthDamageMultiplier, NumUpgradeChoices, HealthRegen, HealthRegenStill, DamageReductionPercent, PushStrength, CoinAttractRange, CoinAttractStrength, Luck, MaxHp,
    NumDashes, DashInvulnTime, DashCooldown, DashProgress, DashStrength, ThornsPercent, ShootFireIgniteChance, FireDamage, FireLifetime, FireSpreadChance, ShootFreezeChance, FreezeLifetime,
    FreezeTimeScale, FreezeOnMeleeChance, FreezeFireDamageMultiplier, LastAmmoDamageMultiplier, FearLifetime, FearDamageMultiplier, FearOnMeleeChance, BulletDamageGrow, BulletDamageShrink,
	BulletDistanceDamage, NumRerollsPerLevel, FullHealthDamageMultiplier, DamagePerEarlierShot, DamageForSpeed, OverallDamageMultiplier, ExplosionSizeMultiplier, GrenadeVelocity, ExplosionDamageMultiplier,
    BulletDamageMultiplier, ExplosionDamageReductionPercent, NonExplosionDamageIncreasePercent, GrenadeStickyPercent, GrenadeFearChance, FearDrainPercent, FearPainPercent, CrateChanceAdditional,
	AttackSpeedStill, FearDropGrenadeChance, FrozenShardsNum, NoDashInvuln, BulletFlatDamageAddition, GrenadesCanCrit, BulletHealTeammateAmount,
}

public enum DamageType { Melee, Ranged, Explosion, Fire, }

public partial class PlayerCitizen : Thing
{
	[ClientInput]
	public Vector2 MouseOffset { get; private set; }

	public Arrow ArrowAimer { get; private set; }
	public Vector2 AimDir { get; private set; }

	[Net] public bool IsDead { get; private set; }

	public float Timer { get; protected set; }
	[Net] public bool IsReloading { get; protected set; }
	[Net] public float ReloadProgress { get; protected set; }

	public const float BASE_MOVE_SPEED = 15f;
	private int _shotNum;

	[Net] public int Level { get; protected set; }
	public int ExperienceTotal { get; protected set; }
	[Net] public int ExperienceCurrent { get; protected set; }
	[Net] public int ExperienceRequired { get; protected set; }
	public bool IsChoosingLevelUpReward { get; protected set; }

    [Net, Predicted] public float DashTimer { get; private set; }
	[Net, Predicted] public bool IsDashing { get; private set; }
    [Net, Predicted] public Vector2 DashVelocity { get; private set; }
    [Net, Predicted] public float DashInvulnTimer { get; private set; }
    private TimeSince _dashCloudTime;
	public float DashProgress { get; protected set; }
	[Net, Predicted] public float DashRechargeProgress { get; protected set; }
	[Net, Predicted] public int NumDashesAvailable { get; private set; }
	public int AmmoCount { get; protected set; }

	public bool IsMoving => Velocity.LengthSquared > 0.01f && !IsDashing;
    public bool IsInvulnerable => IsDashing && Stats[PlayerStat.NoDashInvuln] <= 0f;

    private float _flashTimer;
	private bool _isFlashing;
    public float TimeSinceHurt { get; private set; }

    public Nametag Nametag { get; private set; }

    private ShieldVfx _shieldVfx;

    [Net] public int NumRerollAvailable { get; set; }

	// STATS
	[Net] public IDictionary<PlayerStat, float> Stats { get; private set; }

	// STATUS
	[Net] public IDictionary<int, Status> Statuses { get; private set; }

	//private List<Status> _statusesToRemove = new List<Status>();


	// MODIFIERS
	private Dictionary<Status, Dictionary<PlayerStat, ModifierData>> _modifiers_stat = new Dictionary<Status, Dictionary<PlayerStat, ModifierData>>();
	private Dictionary<PlayerStat, float> _original_properties_stat = new Dictionary<PlayerStat, float>();

	public override void Spawn()
	{
		base.Spawn();

		//TexturePath = "textures/sprites/head.png";
		//SpriteTexture = "textures/sprites/citizen.png";
		Filter = SpriteFilter.Pixelated;
		//Scale = new Vector2(1f, 142f / 153f);
		//Scale = new Vector2(1f, 35f / 16f) * 0.5f;

		if (Sandbox.Game.IsServer)
		{
            SpriteTexture = SpriteTexture.Atlas($"textures/sprites/player_spritesheet_{Sandbox.Game.Random.Int(1, 5)}.png", 5, 4);
            BasePivotY = 0.05f;
			HeightZ = 0f;
			//Pivot = new Vector2(0.5f, 0.05f);

			CollideWith.Add(typeof(Enemy));
			CollideWith.Add(typeof(PlayerCitizen));

			Stats = new Dictionary<PlayerStat, float>();
			Statuses = new Dictionary<int, Status>();
			//ClientStatuses = new List<Status>();

			InitializeStats();

			Predictable = true;
		}
	}

	public void InitializeStats()
	{
		AnimationPath = "textures/sprites/player_idle.frames";
		AnimationSpeed = 0.66f;

		_original_properties_stat.Clear();

		RemoveShieldVfx();

        Level = 0;
		ExperienceRequired = GetExperienceReqForLevel(Level + 1);
		ExperienceTotal = 0;
		ExperienceCurrent = 0;
		Stats[PlayerStat.AttackTime] = 0.15f;
		Timer = Stats[PlayerStat.AttackTime];
		AmmoCount = 5;
		Stats[PlayerStat.MaxAmmoCount] = AmmoCount;
		Stats[PlayerStat.ReloadTime] = 1.5f;
		Stats[PlayerStat.ReloadSpeed] = 1f;
		Stats[PlayerStat.AttackSpeed] = 1f;
		Stats[PlayerStat.BulletDamage] = 5f;
		Stats[PlayerStat.BulletForce] = 0.55f;
		Stats[PlayerStat.Recoil] = 0f;
		Stats[PlayerStat.MoveSpeed] = 1f;
		Stats[PlayerStat.NumProjectiles] = 1f;
		Stats[PlayerStat.BulletSpread] = 35f;
		Stats[PlayerStat.BulletInaccuracy] = 5f;
		Stats[PlayerStat.BulletSpeed] = 4.5f;
		Stats[PlayerStat.BulletLifetime] = 0.8f;
		Stats[PlayerStat.Luck] = 1f;
		Stats[PlayerStat.CritChance] = 0.05f;
		Stats[PlayerStat.CritMultiplier] = 1.5f;
		Stats[PlayerStat.LowHealthDamageMultiplier] = 1f;
		Stats[PlayerStat.FullHealthDamageMultiplier] = 1f;
		Stats[PlayerStat.ThornsPercent] = 0f;

		Stats[PlayerStat.NumDashes] = 1f;
		NumDashesAvailable = (int)MathF.Round(Stats[PlayerStat.NumDashes]);
		Stats[PlayerStat.DashCooldown] = 3f;
		Stats[PlayerStat.DashInvulnTime] = 0.25f;
		Stats[PlayerStat.DashStrength] = 3f;
		Stats[PlayerStat.BulletNumPiercing] = 0f;

		Health = 100f;
		Stats[PlayerStat.MaxHp] = 100f;
		IsDead = false;
		Radius = 0.1f;
		GridPos = Game.GetGridSquareForPos(Position);
		AimDir = Vector2.Up;
		NumRerollAvailable = 2;

		Stats[PlayerStat.FireDamage] = 1.0f;
		Stats[PlayerStat.FireLifetime] = 2.0f;
		Stats[PlayerStat.ShootFireIgniteChance] = 0f;
		Stats[PlayerStat.FireSpreadChance] = 0f;
		Stats[PlayerStat.ShootFreezeChance] = 0f;
		Stats[PlayerStat.FreezeLifetime] = 3f;
		Stats[PlayerStat.FreezeTimeScale] = 0.6f;
		Stats[PlayerStat.FreezeOnMeleeChance] = 0f;
		Stats[PlayerStat.FreezeFireDamageMultiplier] = 1f;
		Stats[PlayerStat.FearLifetime] = 4f;
		Stats[PlayerStat.FearDamageMultiplier] = 1f;
		Stats[PlayerStat.FearOnMeleeChance] = 0f;

		Stats[PlayerStat.CoinAttractRange] = 1.7f;
		Stats[PlayerStat.CoinAttractStrength] = 3.1f;

		Stats[PlayerStat.NumUpgradeChoices] = 3f;
		Stats[PlayerStat.HealthRegen] = 0f;
		Stats[PlayerStat.HealthRegenStill] = 0f;
		Stats[PlayerStat.DamageReductionPercent] = 0f;
		Stats[PlayerStat.PushStrength] = 50f;
		Stats[PlayerStat.LastAmmoDamageMultiplier] = 1f;
		Stats[PlayerStat.BulletDamageGrow] = 0f;
		Stats[PlayerStat.BulletDamageShrink] = 0f;
		Stats[PlayerStat.BulletDistanceDamage] = 0f;
		Stats[PlayerStat.NumRerollsPerLevel] = 1f;
		Stats[PlayerStat.DamagePerEarlierShot] = 0f;
		Stats[PlayerStat.DamageForSpeed] = 0f;
        Stats[PlayerStat.OverallDamageMultiplier] = 1f;
		Stats[PlayerStat.ExplosionSizeMultiplier] = 1f;
        Stats[PlayerStat.GrenadeVelocity] = 8f;
        Stats[PlayerStat.ExplosionDamageMultiplier] = 1f;
        Stats[PlayerStat.BulletDamageMultiplier] = 1f;
        Stats[PlayerStat.ExplosionDamageReductionPercent] = 0f;
        Stats[PlayerStat.NonExplosionDamageIncreasePercent] = 0f;
        Stats[PlayerStat.GrenadeStickyPercent] = 0f;
        Stats[PlayerStat.GrenadeFearChance] = 0f;
        Stats[PlayerStat.FearDrainPercent] = 0f;
        Stats[PlayerStat.FearPainPercent] = 0f;
        Stats[PlayerStat.CrateChanceAdditional] = 0f;
        Stats[PlayerStat.AttackSpeedStill] = 1f;
        Stats[PlayerStat.FearDropGrenadeChance] = 0f;
        Stats[PlayerStat.FrozenShardsNum] = 0f;
        Stats[PlayerStat.NoDashInvuln] = 0f;
        Stats[PlayerStat.BulletFlatDamageAddition] = 0f;
        Stats[PlayerStat.GrenadesCanCrit] = 0f;
        Stats[PlayerStat.BulletHealTeammateAmount] = 0f;

        Statuses.Clear();
		//_statusesToRemove.Clear();
		_modifiers_stat.Clear();

		_isFlashing = false;
		ColorTint = Color.White;
		EnableDrawing = true;
		IsChoosingLevelUpReward = false;
		IsDashing = false;
		IsReloading = false;
		ReloadProgress = 0f;
		DashProgress = 0f;
		DashRechargeProgress = 1f;
		TempWeight = 0f;
		_shotNum = 0;
		TimeSinceHurt = 999f;

		ShadowOpacity = 0.8f;
		ShadowScale = 1.12f;

		//AddStatus("MovespeedStatus");
		//AddStatus("MovespeedStatus");
		//AddStatus("MovespeedStatus");
		//AddStatus("MovespeedStatus");
		//AddStatus("MovespeedStatus");

		InitializeStatsClient();
		RefreshStatusHud(To.Single(Client));
	}

	[ClientRpc]
	public void InitializeStatsClient()
	{
		Nametag?.SetVisible(true);

        if (ArrowAimer != null)
            ArrowAimer.Opacity = 1f;
    }

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		if (Game.LocalPlayer == this)
		{
			ArrowAimer = new Arrow
			{
				Parent = this,
				//LocalPosition = new Vector3(0.3f, 0f, 0f);
				Depth = 100f
			};
		}

		Nametag = Game.Hud.SpawnNametag(this);

		SpawnShadow(1.12f);

		//Game.Hud.InfoPanel.DashContainer.Refresh();
	}

	[Event.Tick.Server]
	public void ServerTick()
	{
		//Utils.DrawCircle(Position, 1.7f, 18, Time.Now, Color.Red);
		//Log.Info("HP: " + Stats[PlayerStat.MaxHp]);
		//DebugText("game over: " + Game.IsGameOver);
		//DebugText(NumDashesAvailable + " / " + NumDashes + "\n\n" + _dashTimer);
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		//Log.Info("local player: " + (Game.Client != null));
		//DebugText("\n\nclient game over: " + Game.IsGameOver);
		if (this == Sandbox.Game.LocalPawn)
		{
			Sound.Listener = new()
			{
				Position = new Vector3(Position.x, Position.y, 512f),
				//Position = new Vector3(Position.x, Position.y, 2f),
				Rotation = global::Rotation.LookAt(new Vector3(0f, 1f, 0f))
			};
		}
	}
    //Use this to store last known controller look dir.
    Vector2 LastAimDir = Vector2.Up;
    protected override void OnSimulate(IClient cl)
	{
		if (Game.IsGameOver)
			return;	

		float dt = Time.Delta;

		Vector2 inputVector = new Vector2(-Input.AnalogMove.y, Input.AnalogMove.x);

        if(Input.UsingController)
		{
            inputVector = new Vector2(Input.GetAnalog(InputAnalog.Move).x, Input.GetAnalog(InputAnalog.Move).y);
        }

		if (inputVector.LengthSquared > 0f)
			Velocity += inputVector.Normal * Stats[PlayerStat.MoveSpeed] * BASE_MOVE_SPEED * dt;

		Position += Velocity * dt;

		if (IsDashing)
			Position += DashVelocity * dt;

		Velocity = Utils.DynamicEaseTo(Velocity, Vector2.Zero, 0.2f, dt);
		TempWeight *= (1f - dt * 4.7f);

		ShadowScale = IsDashing ? Utils.MapReturn(DashProgress, 0f, 1f, 1.12f, 0.75f, EasingType.SineInOut) : 1.12f;

		HandleBounds();

		Rotation = Velocity.Length * MathF.Cos(Time.Now * MathF.PI * 7f) * 1.5f;

		Depth = -Position.y * 10f;

		if (MathF.Abs(Input.AnalogMove.y) > 0f)
			Scale = new Vector2(1f * Input.AnalogMove.y < 0f ? -1f : 1f, 1f) * 1f;

		if (Sandbox.Game.IsClient)
		{
			Camera2D.Current.TargetPosition = Position;
		}

        //Rotation = (MathF.Atan2(MouseOffset.y, MouseOffset.x) * (180f / MathF.PI)) - 90f;
        //Scale = new Vector2( MathF.Sin( Time.Now * 4f ) * 1f + 2f, MathF.Sin( Time.Now * 3f ) * 1f + 2f );

        //DebugOverlay.Text(Position.ToString(), Position);

        //DebugOverlay.Text(Position.ToString() + "\n" + Game.GetGridSquareForPos(Position).ToString(), Position + new Vector2(0.2f, 0f));
        //DebugOverlay.Line(Position, Position + new Vector2(0.01f, 0.01f), 0f, false);

        if (Input.UsingController)
		{        
            AimDir = LastAimDir;
			if (Input.GetAnalog(InputAnalog.Look).Length > 0f)
			{
				LastAimDir = new Vector2(Input.GetAnalog(InputAnalog.Look).x, Input.GetAnalog(InputAnalog.Look).y).Normal;
			}
			else
			{
                LastAimDir = AimDir;
            }
		}
		else
		{
            AimDir = MouseOffset.Normal;
        }

        if (ArrowAimer != null)
		{
			ArrowAimer.LocalRotation = (MathF.Atan2(AimDir.y, AimDir.x) * (180f / MathF.PI));
			ArrowAimer.Position = Position + new Vector2(0f, 0.4f) + AimDir * 0.65f;
		}

		if (cl.IsBot)
		{
			MouseOffset = new Vector2(Sandbox.Game.Random.Float(-10f, 10f), Sandbox.Game.Random.Float(-10f, 10f));
		}

		if (!IsDead)
		{
			HandleDashing(dt);
		}
		
		if (Sandbox.Game.IsServer)
		{
			bool hurting = TimeSinceHurt < 0.15f;
			bool attacking = !IsReloading;
			bool moving = Velocity.LengthSquared > 0.01f && inputVector.LengthSquared > 0.1f;

            string stateStr = "";
			if (hurting && attacking)
				stateStr = "hurt_attack_";
			else if(hurting)
                stateStr = "hurt_";
            else if (attacking)
                stateStr = "attack_";

            AnimationPath = IsDead ? $"textures/sprites/player_ghost_{(moving ? "walk" : "idle")}.frames" : $"textures/sprites/player_{stateStr}{(moving ? "walk" : "idle")}.frames";
            AnimationSpeed = moving ? Utils.Map(Velocity.Length, 0f, 2f, 1.5f, 2f) : 0.66f;

			TimeSinceHurt += dt;
            
			//if (Input.Pressed(InputButton.Run))
			//{
			//	//Game.Restart();
			//	AddExperience(GetExperienceReqForLevel(Level));

			//	//for(int i = 0; i < 9; i++)
			//	//            {
			//	//                AddStatus(TypeLibrary.GetDescription(typeof(FreezeShootStatus)));
			//	//}

			//	return;
			//}

			var gridPos = Game.GetGridSquareForPos(Position);
			if (gridPos != GridPos)
			{
				Game.DeregisterThingGridSquare(this, GridPos);
				Game.RegisterThingGridSquare(this, gridPos);
				GridPos = gridPos;
			}

			for (int dx = -1; dx <= 1; dx++)
			{
				for (int dy = -1; dy <= 1; dy++)
				{
					Game.HandleThingCollisionForGridSquare(this, new GridSquare(GridPos.x + dx, GridPos.y + dy), dt);
				}
			}

			if (!IsDead)
			{
				HandleStatuses(dt);
				HandleShooting(dt);
				HandleFlashing(dt);
				HandleRegen(dt);
			}
		}

		//if (Input.Down(InputButton.Reload))
		//{
		//	for (float x = Game.BOUNDS_MIN.x; x < Game.BOUNDS_MAX.x; x += Game.GRID_SIZE)
		//	{
		//		for (float y = Game.BOUNDS_MIN.y; y < Game.BOUNDS_MAX.y; y += Game.GRID_SIZE)
		//		{
		//			DebugOverlay.Box(new Vector2(x, y), new Vector2(x + Game.GRID_SIZE, y + Game.GRID_SIZE), Color.White, 0f, false);
		//			DebugOverlay.Text((new Vector2(x, y)).ToString(), new Vector2(x + 0.1f, y + 0.1f));
		//		}
		//	}
		//}
	}

	void HandleRegen(float dt)
	{
		if (Math.Abs(Stats[PlayerStat.HealthRegen]) > 0f)
			RegenHealth(Stats[PlayerStat.HealthRegen] * dt);

		if (Stats[PlayerStat.HealthRegenStill] > 0f && !IsMoving)
			RegenHealth(Stats[PlayerStat.HealthRegenStill] * dt);
	}

	public void RegenHealth(float amount)
	{
		Health += amount;
        if (Health > Stats[PlayerStat.MaxHp])
            Health = Stats[PlayerStat.MaxHp];

        if (Health <= 0f)
            Die();
    }

	void HandleDashing(float dt)
	{
		int numDashes = (int)MathF.Round(Stats[PlayerStat.NumDashes]);
		if (NumDashesAvailable < numDashes)
		{
			DashTimer -= dt;
			DashRechargeProgress = Utils.Map(DashTimer, Stats[PlayerStat.DashCooldown], 0f, 0f, 1f);
			if (DashTimer <= 0f)
			{
				DashRecharged();
			}
		}

		if (DashInvulnTimer > 0f)
		{
			DashInvulnTimer -= dt;
			DashProgress = Utils.Map(DashInvulnTimer, Stats[PlayerStat.DashInvulnTime], 0f, 0f, 1f);
			if (DashInvulnTimer <= 0f)
			{
				IsDashing = false;
				ColorTint = Color.White;
				DashFinished();
			}
			else
			{
				if(IsInvulnerable)
					ColorTint = new Color(Sandbox.Game.Random.Float(0.1f, 0.25f), Sandbox.Game.Random.Float(0.1f, 0.25f), 1f);

				if(_dashCloudTime > Sandbox.Game.Random.Float(0.1f, 0.2f))
				{
					SpawnCloudClient();
					_dashCloudTime = 0f;
				}
			}
		}

		if (Input.Pressed("Jump") || Input.Pressed("attack1"))
			Dash();
	}

	public void Dash()
	{
		if (NumDashesAvailable <= 0)
			return;

		Vector2 dashDir = Velocity.LengthSquared > 0f ? Velocity.Normal : AimDir;
		DashVelocity = dashDir * Stats[PlayerStat.DashStrength];
		TempWeight = 2f;

		if (NumDashesAvailable == (int)Stats[PlayerStat.NumDashes])
			DashTimer = Stats[PlayerStat.DashCooldown];

		NumDashesAvailable--;
		IsDashing = true;
		DashInvulnTimer = Stats[PlayerStat.DashInvulnTime];
		DashProgress = 0f;
		DashRechargeProgress = 0f;

		Game.PlaySfxNearby("player.dash", Position + dashDir * 0.5f, pitch: Utils.Map(NumDashesAvailable, 0, 5, 1f, 0.9f), volume: 1f, maxDist: 4f);
		SpawnCloudClient();
		_dashCloudTime = 0f;

		ForEachStatus(status => status.OnDashStarted());
    }

	[ClientRpc]
	public void SpawnCloudClient()
	{
		Game.SpawnCloud(Position + new Vector2(Sandbox.Game.Random.Float(1f, 1f), Sandbox.Game.Random.Float(1f, 1f)) * 0.05f);
	}

	public void DashFinished()
	{
		ForEachStatus(status => status.OnDashFinished());
	}

	public void DashRecharged()
	{
		NumDashesAvailable++;
		var numDashes = (int)MathF.Round(Stats[PlayerStat.NumDashes]);
		if (NumDashesAvailable > numDashes)
			NumDashesAvailable = numDashes;

		if(NumDashesAvailable < numDashes)
		{
			DashTimer = Stats[PlayerStat.DashCooldown];
			DashRechargeProgress = 0f;
		}
		else
		{
			DashRechargeProgress = 1f;
		}

        ForEachStatus(status => status.OnDashRecharged());

		Game.PlaySfxTarget(To.Single(Client), "player.dash.recharge", Position, pitch: Utils.Map(NumDashesAvailable, 1, numDashes, 1f, 1.2f), volume: 0.2f);
	}

	public void ForEachStatus(Action<Status> action)
	{
		if (Sandbox.Game.IsClient)
			return;

        foreach (var (_, status) in Statuses)
		{
			action(status);
		}
	}

	[ClientRpc]
	public void RefreshStatusHud()
	{
		StatusPanel.Current?.Refresh();
	}

	void HandleStatuses(float dt)
	{
		string debug = "";

		foreach (KeyValuePair<int, Status> pair in Statuses)
		{
			Status status = pair.Value;
			if (status.ShouldUpdate)
				status.Update(dt);

			debug += status.ToString() + "\n";
		}

		//DebugText(debug);
	}

	void HandleShooting(float dt)
	{
		if (IsReloading)
		{
			ReloadProgress = Utils.Map(Timer, Stats[PlayerStat.ReloadTime], 0f, 0f, 1f);
			Timer -= dt * Stats[PlayerStat.ReloadSpeed];
			if (Timer <= 0f)
			{
				Reload();
			}
		}
		else
		{
            Timer -= dt * Stats[PlayerStat.AttackSpeed] * (IsMoving ? 1f : Stats[PlayerStat.AttackSpeedStill]);
			if (Timer <= 0f)
			{
				Shoot(isLastAmmo: AmmoCount == 1);
				AmmoCount--;

				if (AmmoCount <= 0)
				{
					IsReloading = true;

					//Game.PlaySfxTarget(To.Single(Client), "reload.start", Position, pitch: 1f, volume: 0.5f);

					Timer += Stats[PlayerStat.ReloadTime];
				}
				else
				{
					Timer += Stats[PlayerStat.AttackTime];
				}
			}
		}

		//DebugText(AmmoCount.ToString() + "\nreloading: " + IsReloading + "\ntimer: " + Timer + "\nShotDelay: " + AttackTime + "\nReloadTime: " + ReloadTime + "\nAttackSpeed: " + AttackSpeed);
	}

	void HandleFlashing(float dt)
	{
		if (_isFlashing)
		{
			_flashTimer -= dt;
			if (_flashTimer < 0f)
			{
				_isFlashing = false;
				ColorTint = Color.White;
			}
		}
	}

	public void Shoot(bool isLastAmmo = false)
	{
		float start_angle = MathF.Sin(-_shotNum * 2f) * Stats[PlayerStat.BulletInaccuracy];

		int num_bullets_int = (int)Stats[PlayerStat.NumProjectiles];
		float currAngleOffset = num_bullets_int == 1 ? 0f : -Stats[PlayerStat.BulletSpread] * 0.5f;
		float increment = num_bullets_int == 1 ? 0f : Stats[PlayerStat.BulletSpread] / (float)(num_bullets_int - 1);

		var pos = Position + AimDir * 0.5f;

		for (int i = 0; i < num_bullets_int; i++)
		{
			var dir = Utils.RotateVector(AimDir, start_angle + currAngleOffset + increment * i);
			SpawnBullet(pos, dir, isLastAmmo);
		}

		Game.PlaySfxNearby("shoot", pos, pitch: Utils.Map(_shotNum, 0f, (float)Stats[PlayerStat.MaxAmmoCount], 1f, 1.25f), volume: 1f, maxDist: 4f);

		Velocity -= AimDir * Stats[PlayerStat.Recoil];

		_shotNum++;
	}

	void SpawnBullet(Vector2 pos, Vector2 dir, bool isLastAmmo = false)
	{
        var damage = (Stats[PlayerStat.BulletDamage] * Stats[PlayerStat.BulletDamageMultiplier] + Stats[PlayerStat.BulletFlatDamageAddition]) * GetDamageMultiplier();
        if (isLastAmmo)
            damage *= Stats[PlayerStat.LastAmmoDamageMultiplier];

        if (Stats[PlayerStat.DamagePerEarlierShot] > 0f)
            damage += _shotNum * Stats[PlayerStat.DamagePerEarlierShot];

        if (Stats[PlayerStat.DamageForSpeed] > 0f)
        {
            damage += Stats[PlayerStat.DamageForSpeed] * Velocity.Length;

            if (IsDashing)
                damage += Stats[PlayerStat.DamageForSpeed] * DashVelocity.Length;
        }

        var basePivotY = Utils.Map(damage, 5f, 30f, -1.2f, -0.3f);

        var bullet = new Bullet
        {
            Position = pos,
            Depth = -1f,
            Velocity = dir * Stats[PlayerStat.BulletSpeed],
            Shooter = this,
            TempWeight = 3f,
            BasePivotY = basePivotY,
        };

        bullet.Stats[BulletStat.Damage] = damage;
        bullet.Stats[BulletStat.Force] = Stats[PlayerStat.BulletForce];
        bullet.Stats[BulletStat.Lifetime] = Stats[PlayerStat.BulletLifetime];
        bullet.Stats[BulletStat.NumPiercing] = (int)MathF.Round(Stats[PlayerStat.BulletNumPiercing]);
        bullet.Stats[BulletStat.FireIgniteChance] = Stats[PlayerStat.ShootFireIgniteChance];
        bullet.Stats[BulletStat.FreezeChance] = Stats[PlayerStat.ShootFreezeChance];
        bullet.Stats[BulletStat.GrowDamageAmount] = Stats[PlayerStat.BulletDamageGrow];
        bullet.Stats[BulletStat.ShrinkDamageAmount] = Stats[PlayerStat.BulletDamageShrink];
        bullet.Stats[BulletStat.DistanceDamageAmount] = Stats[PlayerStat.BulletDistanceDamage];
        bullet.Stats[BulletStat.HealTeammateAmount] = Stats[PlayerStat.BulletHealTeammateAmount];

        if (Stats[PlayerStat.GrenadesCanCrit] <= 0f)
		{
            bullet.Stats[BulletStat.CriticalChance] = Stats[PlayerStat.CritChance];
            bullet.Stats[BulletStat.CriticalMultiplier] = Stats[PlayerStat.CritMultiplier];
        }

        bullet.Init();

        bullet.HeightZ = 0f;

        Game.AddThing(bullet);
    }

    void Reload()
	{
		AmmoCount = (int)Stats[PlayerStat.MaxAmmoCount];
		IsReloading = false;
		_shotNum = 0;
		ReloadProgress = 0f;

        ForEachStatus(status => status.OnReload());

        //Game.PlaySfxTarget(To.Single(Client), "reload.end", Position, pitch: 1f, volume: 0.5f);
    }

	void HandleBounds()
	{
		var x_min = Game.BOUNDS_MIN.x + Radius;
		var x_max = Game.BOUNDS_MAX.x - Radius;
		var y_min = Game.BOUNDS_MIN.y;
		var y_max = Game.BOUNDS_MAX.y - Radius * 9.5f;

		if (Position.x < x_min)
		{
			Position = new Vector2(x_min, Position.y);
			Velocity = new Vector2(Velocity.x * -1f, Velocity.y);
		}
		else if (Position.x > x_max)
		{
			Position = new Vector2(x_max, Position.y);
			Velocity = new Vector2(Velocity.x * -1f, Velocity.y);
		}

		if (Position.y < y_min)
		{
			Position = new Vector2(Position.x, y_min);
			Velocity = new Vector2(Velocity.x, Velocity.y * -1f);
		}
		else if (Position.y > y_max)
		{
			Position = new Vector2(Position.x, y_max);
			Velocity = new Vector2(Velocity.x, Velocity.y * -1f);
		}
	}

	public override void BuildInput()
	{
		base.BuildInput();

		// To account for bullets being raised off the ground
		var aimOffset = new Vector2( 0f, 0.4f );

		// garry: use put the mouse offset in the input
		MouseOffset = Camera2D.Current.ScreenToWorld(Mouse.Position) - Position - aimOffset;
		//inputBuilder.Cursor = new Ray(0, MouseOffset);
	}
	
	[ConCmd.Server]
	public static void SetMouseOffset(Vector2 offset)
	{
		if (ConsoleSystem.Caller.Pawn is PlayerCitizen p)
		{
			p.MouseOffset = offset;
		}
	}

	// returns actual damage amount taken
	public float Damage(float damage, DamageType damageType)
	{
		if (IsInvulnerable)
		{
			// show DODGED! floater
			return 0f;
		}

		if(HasStatus(TypeLibrary.GetType(typeof(ShieldStatus))))
		{
			var shieldStatus = GetStatus(TypeLibrary.GetType(typeof(ShieldStatus))) as ShieldStatus;
            if (shieldStatus != null && shieldStatus.IsShielded)
			{
				shieldStatus.LoseShield();
				return 0f;
			}
		}

		TimeSinceHurt = 0f;

        if (Stats[PlayerStat.DamageReductionPercent] > 0f)
			damage *= (1f - MathX.Clamp(Stats[PlayerStat.DamageReductionPercent], 0f, 1f));

        if (damageType == DamageType.Explosion && Stats[PlayerStat.ExplosionDamageReductionPercent] > 0f)
            damage *= (1f - MathX.Clamp(Stats[PlayerStat.ExplosionDamageReductionPercent], 0f, 1f));

        if (damageType != DamageType.Explosion && Stats[PlayerStat.NonExplosionDamageIncreasePercent] > 0f)
            damage *= (1f + Stats[PlayerStat.NonExplosionDamageIncreasePercent]);

        ForEachStatus(status => status.OnHurt(damage));

        Health -= damage;
		DamageNumbers.Create(Position + new Vector2(Sandbox.Game.Random.Float(2.5f, 5.5f), Sandbox.Game.Random.Float(8.5f, 10.5f)) * 0.1f, damage, DamageNumberType.Player);
		Flash(0.125f);

		SpawnBloodClient(damage);

		if (Health <= 0f)
			Die();

		return damage;
	}

	[ClientRpc]
	public void SpawnBloodClient(float damage)
	{
		var blood = Game.SpawnBloodSplatter(Position);
		blood.Scale *= Utils.Map(damage, 1f, 20f, 0.3f, 0.5f, EasingType.QuadIn) * Sandbox.Game.Random.Float(0.8f, 1.2f);
		blood.Lifetime *= 0.3f;
	}

	public void Die()
	{
		if (IsDead)
			return;

		IsDead = true;
		AnimationPath = $"textures/sprites/player_ghost_idle.frames";
        Game.PlayerDied(this);
		//EnableDrawing = false;
		ColorTint = new Color(1f, 1f, 1f, 0.05f);
		ShadowOpacity = 0.1f;
		_isFlashing = false;
		IsReloading = false;

		Game.PlaySfxNearby("die", Position, pitch: Sandbox.Game.Random.Float(1f, 1.2f), volume: 1.5f, maxDist: 12f);
		DieClient();
        DieClientSingle(To.Single(Client));
    }

	[ClientRpc]
	public void DieClient()
	{
		Nametag.SetVisible(false);

		if(ArrowAimer != null)
			ArrowAimer.Opacity = 0f;
	}

    [ClientRpc]
    public void DieClientSingle()
    {
        Game.Hud.RemoveChoicePanel();
    }

    public float GetDamageMultiplier()
	{
		float damageMultiplier = Stats[PlayerStat.OverallDamageMultiplier];

        if (Stats[PlayerStat.LowHealthDamageMultiplier] > 1f)
			damageMultiplier *= Utils.Map(Health, Stats[PlayerStat.MaxHp], 0f, 1f, Stats[PlayerStat.LowHealthDamageMultiplier]);

        if (Stats[PlayerStat.FullHealthDamageMultiplier] > 1f && !(Health < Stats[PlayerStat.MaxHp]))
            damageMultiplier *= Stats[PlayerStat.FullHealthDamageMultiplier];

        //if (damageMultiplier < -1f)
        //	damageMultiplier = -1f;

        return damageMultiplier;
	}

	public override void Colliding(Thing other, float percent, float dt)
	{
		base.Colliding(other, percent, dt);

		if (IsDead)
			return;

        ForEachStatus(status => status.Colliding(other, percent, dt));

        if (other is Enemy enemy && !enemy.IsDying)
		{
			if(!Position.Equals(other.Position))
			{
                var spawnFactor = Utils.Map(enemy.ElapsedTime, 0f, enemy.SpawnTime, 0f, 1f, EasingType.QuadIn);
                Velocity += (Position - other.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 100f) * (1f + other.TempWeight) * spawnFactor * dt;
            }
		}
		else if(other is PlayerCitizen player)
		{
			if(!player.IsDead && !Position.Equals(other.Position))
			{
				Velocity += (Position - other.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 100f) * (1f + other.TempWeight) * dt;
			}
		}
	}

	[ConCmd.Server("add_status")]
	public static void AddStatusCmd(int typeIdentity)
	{
		TypeDescription type = StatusManager.IdentityToType(typeIdentity);
		var player = ConsoleSystem.Caller.Pawn as PlayerCitizen;
		player.AddStatus(type);
	}

	public void AddStatus(TypeDescription type)
	{
		//Log.Info("AddStatus: " + type.TargetType.ToString());

		Status status = null;
		var typeIdentity = type.Identity;

		if(Statuses.ContainsKey(typeIdentity))
		{
			status = Statuses[typeIdentity];
			status.Level++;
		}
			
		if (status == null)
		{
			status = StatusManager.CreateStatus(type);
			Statuses.Add(typeIdentity, status);
			status.Init(this);
		}

        Sandbox.Services.Stats.Increment( Client, "status", 1, $"{type.Name.ToLowerInvariant()}", new { Status = type.Name.ToLowerInvariant(), Level = status.Level } );

        status.Refresh();

		//ClearStatusesClient();
		//foreach (KeyValuePair<TypeDescription, Status> pair in Statuses)
  //          AddStatusClient(StatusManager.TypeToIdentity(pair.Key), pair.Value);

		RefreshStatusHud();

		IsChoosingLevelUpReward = false;
		CheckForLevelUp();
	}

	//[ClientRpc]
	//public void ClearStatusesClient()
	//{
	//	Statuses.Clear();
	//}

	//[ClientRpc]
	//public void AddStatusClient(int typeIdentity, Status status)
	//{
	//	Statuses.Add(StatusManager.IdentityToType(typeIdentity), status);
	//}

	public bool HasStatus(TypeDescription type)
	{
		return Statuses.ContainsKey(type.Identity);
	}

	public Status GetStatus(TypeDescription type)
	{
        if (Statuses.ContainsKey(type.Identity))
            return Statuses[type.Identity];

		return null;
	}

	public int GetStatusLevel(TypeDescription type)
	{
		if(Statuses.ContainsKey(type.Identity))
			return Statuses[type.Identity].Level;

		return 0;
	}

    public void Modify(Status caller, PlayerStat statType, float value, ModifierType type, float priority = 0f, bool update = true)
    {
        if (!_modifiers_stat.ContainsKey(caller))
            _modifiers_stat.Add(caller, new Dictionary<PlayerStat, ModifierData>());

        _modifiers_stat[caller][statType] = new ModifierData(value, type, priority);

        if (update)
            UpdateProperty(statType);
    }

	public void AdjustBaseStat(PlayerStat statType, float amount, bool update = true)
	{
		if (!_original_properties_stat.ContainsKey(statType))
			_original_properties_stat.Add(statType, Stats[statType]);
		
		_original_properties_stat[statType] += amount;

        if (update)
            UpdateProperty(statType);
    }
    
    void UpdateProperty(PlayerStat statType)
    {
        if (!_original_properties_stat.ContainsKey(statType))
        {
            _original_properties_stat.Add(statType, Stats[statType]);
        }

        float curr_value = _original_properties_stat[statType];
        float curr_set = curr_value;
        bool should_set = false;
        float curr_priority = 0f;
        float total_add = 0f;
        float total_mult = 1f;

        foreach (Status caller in _modifiers_stat.Keys)
        {
            var dict = _modifiers_stat[caller];
            if (dict.ContainsKey(statType))
            {
                var mod_data = dict[statType];
                switch (mod_data.type)
                {
                    case ModifierType.Set:
                        if (mod_data.priority >= curr_priority)
                        {
                            curr_set = mod_data.value;
                            curr_priority = mod_data.priority;
                            should_set = true;
                        }
                        break;
                    case ModifierType.Add:
                        total_add += mod_data.value;
                        break;
                    case ModifierType.Mult:
                        total_mult *= mod_data.value;
                        break;
                }
            }
        }

        if (should_set)
            curr_value = curr_set;

        curr_value += total_add;
        curr_value *= total_mult;

		Stats[statType] = curr_value;
    }

	public void AddExperience(int xp)
	{
		Sandbox.Game.AssertServer();

		ExperienceTotal += xp;
		ExperienceCurrent += xp;

		ForEachStatus(status => status.OnGainExperience(xp));

		if (!IsChoosingLevelUpReward)
			CheckForLevelUp();

		Sandbox.Services.Stats.Increment(Client, "coins", 1);
		Sandbox.Services.Stats.Increment(Client, "xp", xp);
	}

	public void CheckForLevelUp()
	{
		//Log.Info("CheckForLevelUp: " + ExperienceCurrent + " / " + ExperienceRequired + " IsServer: " + Sandbox.Game.IsServer + " Level: " + Level);
		if (ExperienceCurrent >= ExperienceRequired && !Game.IsGameOver)
			LevelUp();
	}

	public void LevelUp()
	{
		Sandbox.Game.AssertServer();

		ExperienceCurrent -= ExperienceRequired;

		Level++;
		ExperienceRequired = GetExperienceReqForLevel(Level + 1);
		NumRerollAvailable += (int)Stats[PlayerStat.NumRerollsPerLevel];

        ForEachStatus(status => status.OnLevelUp());

        IsChoosingLevelUpReward = true;

		LevelUpClient(To.Single(Client));
	}

	[ClientRpc]
	public void LevelUpClient()
	{
		if(this == Sandbox.Game.LocalPawn)
		{
			Game.Hud.SpawnChoicePanel();
			Game.PlaySfxTarget(To.Single(Sandbox.Game.LocalClient), "levelup", Position, Sandbox.Game.Random.Float(0.95f, 1.05f), 0.66f);
		}
	}

	[ConCmd.Server]
	public static void UseRerollCmd()
	{
		var player = ConsoleSystem.Caller.Pawn as PlayerCitizen;
		player.NumRerollAvailable--;
		player.LevelUpClient();

        player.ForEachStatus(status => status.OnReroll());
    }

	public int GetExperienceReqForLevel(int level)
	{
		return (int)MathF.Round(Utils.Map(level, 1, 150, 3f, 340f, EasingType.SineIn));
	}

	public void Flash(float time)
	{
		if (_isFlashing)
			return;

		ColorTint = new Color(1f, 0f, 0f);
		_isFlashing = true;
		_flashTimer = time;
	}

	public void Heal(float amount, float flashTime)
	{
		ColorTint = new Color(0f, 1f, 0f);
		_isFlashing = true;
		_flashTimer = flashTime;

		Health += amount;
		if (Health > Stats[PlayerStat.MaxHp])
			Health = Stats[PlayerStat.MaxHp];
	}

	public void Revive()
	{
		if (!IsDead)
			return;

		IsChoosingLevelUpReward = false;
		IsDashing = false;
		IsReloading = false;
		ReloadProgress = 0f;
		DashProgress = 0f;
		ExperienceCurrent = 0;

		Health = Stats[PlayerStat.MaxHp] * 0.33f;
		ColorTint = Color.White;

		IsDead = false;
		ReviveClient();
	}

	[ClientRpc]
	public void ReviveClient()
	{
		Nametag.SetVisible(true);

        if (ArrowAimer != null)
            ArrowAimer.Opacity = 1f;
	}

	public void SpawnGrenade(Vector2 pos, Vector2 velocity)
	{
        var grenade = new Grenade()
        {
            Position = pos,
            ExplosionSizeMultiplier = Stats[PlayerStat.ExplosionSizeMultiplier],
            Player = this,
            StickyPercent = Stats[PlayerStat.GrenadeStickyPercent],
            FearChance = Stats[PlayerStat.GrenadeFearChance],
        };

		if(Stats[PlayerStat.GrenadesCanCrit] > 0f)
		{
			grenade.CriticalChance = Stats[PlayerStat.CritChance];
            grenade.CriticalMultiplier = Stats[PlayerStat.CritMultiplier];
        }

        grenade.Velocity = velocity;
        Game.AddThing(grenade);
    }

	public void SpawnBulletRing(Vector2 pos, int numBullets, Vector2 aimDir)
	{
        float increment = 360f / numBullets;

        for (int i = 0; i < numBullets; i++)
        {
            var dir = Utils.RotateVector(aimDir, i * increment);
            SpawnBullet(pos, dir);
        }

        Game.PlaySfxNearby("shoot", pos, pitch: 1f, volume: 1f, maxDist: 3f);
    }

    [ClientRpc]
    public void CreateShieldVfx()
    {
		RemoveShieldVfx();
        _shieldVfx = new ShieldVfx(this);
    }

    [ClientRpc]
    public void RemoveShieldVfx()
    {
        if (_shieldVfx != null)
        {
            _shieldVfx.Delete();
            _shieldVfx = null;
        }
    }

    protected override void OnDestroy()
    {
		if(Sandbox.Game.IsClient)
		{
            Nametag.Delete();
            Nametag = null;
        }

        base.OnDestroy();
    }
}
