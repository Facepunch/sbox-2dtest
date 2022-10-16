using Sandbox;
using System;
using System.Diagnostics;
using System.Linq;
using static Test2D.MyGame;
using System.Collections.Generic;

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

public partial class PlayerCitizen : Thing
{
	public Vector2 MouseOffset { get; private set; }
	
    public Arrow ArrowAimer { get; private set; }
	public Vector2 AimDir { get; private set; }

	public bool IsDead { get; private set; }

	public float Timer { get; protected set; }
	[Net] public float AttackTime { get; protected set; }
	[Net] public float AttackSpeed { get; private set; }
	[Net] public bool IsReloading { get; protected set; }
	[Net] public float ReloadProgress { get; protected set; }
	[Net] public float ReloadTime { get; protected set; }
	[Net] public float ReloadSpeed { get; private set; }
	[Net] public int AmmoCount { get; protected set; }
	[Net] public float MaxAmmoCount { get; protected set; }
	[Net] public float BulletDamage { get; protected set; }
	[Net] public float BulletSize { get; protected set; }
	[Net] public float BulletForce { get; protected set; }
	[Net] public float MoveSpeed { get; protected set; }
	public const float BASE_MOVE_SPEED = 14f;
	[Net] public float NumBullets { get; protected set; }
	[Net] public float BulletSpread { get; protected set; }
	[Net] public float BulletInaccuracy { get; protected set; }
	[Net] public float BulletSpeed { get; protected set; }
	[Net] public float BulletLifetime { get; protected set; }
	[Net] public float BulletNumPiercing { get; protected set; }
	[Net] public float CritChance { get; set; }
	[Net] public float CritMultiplier { get; set; }
	[Net] public float NumUpgradeChoices { get; protected set; }
	[Net] public float HealthRegen { get; protected set; }

	private int _shotNum;

	[Net] public float CoinAttractRange { get; protected set; }
	[Net] public float CoinAttractStrength { get; protected set; }
	[Net] public float Luck { get; protected set; }

	[Net] public int Level { get; protected set; }
	[Net] public int ExperienceTotal { get; protected set; }
	[Net] public int ExperienceCurrent { get; protected set; }
	[Net] public int ExperienceRequired { get; protected set; }
	[Net] public bool IsChoosingLevelUpReward { get; protected set; }

	[Net] public float MaxHp { get; protected set; }

	[Net] public float NumDashes { get; private set; }
	[Net] public int NumDashesAvailable { get; private set; }
	[Net] public float DashCooldown { get; private set; }
	private float _dashTimer;
	public bool IsDashing { get; private set; }
	private Vector2 _dashVelocity;
	private float _dashInvulnTimer;
	[Net] public float DashInvulnTime { get; private set; }
	[Net] public float DashProgress { get; protected set; }
	[Net] public float DashRechargeProgress { get; protected set; }

	private float _flashTimer;
	private bool _isFlashing;

	public Nametag Nametag { get; private set; }

	// STATUS
	[Net] public IDictionary<int, Status> Statuses { get; private set; }
	//[Net] public List<Status> ClientStatuses { get; private set; }

	//private List<Status> _statusesToRemove = new List<Status>();;

	// MODIFIERS
	private Dictionary<Status, Dictionary<string, ModifierData>> _modifiers = new Dictionary<Status, Dictionary<string, ModifierData>>();
	private Dictionary<string, float> _original_properties = new Dictionary<string, float>();

	public override void Spawn()
	{
		base.Spawn();

		//TexturePath = "textures/sprites/head.png";
		//SpriteTexture = "textures/sprites/citizen.png";
		Filter = SpriteFilter.Pixelated;
		//Scale = new Vector2(1f, 142f / 153f);
		//Scale = new Vector2(1f, 35f / 16f) * 0.5f;

		if (Host.IsServer)
        {
			SpriteTexture = SpriteTexture.Atlas("textures/sprites/player_spritesheet.png", 2, 2);
			Pivot = new Vector2(0.5f, 0.05f);

			CollideWith.Add(typeof(Enemy));
			CollideWith.Add(typeof(PlayerCitizen));

			Statuses = new Dictionary<int, Status>();
			//ClientStatuses = new List<Status>();

			InitializeStats();
		}
	}

	public void InitializeStats()
    {
		AnimationPath = "textures/sprites/player_idle.frames";
		AnimationSpeed = 0.66f;
		
		Level = 0;
		ExperienceRequired = GetExperienceReqForLevel(Level + 1);
		ExperienceTotal = 0;
		ExperienceCurrent = 0;
		AttackTime = 0.15f;
		Timer = AttackTime;
		AmmoCount = 5;
		MaxAmmoCount = AmmoCount;
		ReloadTime = 1.5f;
		ReloadSpeed = 1f;
		AttackSpeed = 1f;
		BulletDamage = 5f;
		BulletSize = 0.175f;
		BulletForce = 0.55f;
		MoveSpeed = 1f;
		NumBullets = 1f;
		BulletSpread = 35f;
		BulletInaccuracy = 5f;
		BulletSpeed = 4.5f;
		BulletLifetime = 0.8f;
		Luck = 1f;
		CritChance = 0.05f;
		CritMultiplier = 1.5f;

		NumDashes = 1f;
		NumDashesAvailable = (int)NumDashes;
		DashCooldown = 3f;
		DashInvulnTime = 0.25f;
		BulletNumPiercing = 0f;

		Health = 100f;
		MaxHp = 100f;
		IsDead = false;
		Radius = 0.175f;
		GridPos = Game.GetGridSquareForPos(Position);
		AimDir = Vector2.Up;

		CoinAttractRange = 1.7f;
		CoinAttractStrength = 3.1f;

		NumUpgradeChoices = 3f;
		HealthRegen = 50f;

		Statuses.Clear();
		//_statusesToRemove.Clear();
		_modifiers.Clear();

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

		ShadowOpacity = 0.8f;
		ShadowScale = 1.12f;

		//AddStatus("MovespeedStatus");
		//AddStatus("MovespeedStatus");
		//AddStatus("MovespeedStatus");
		//AddStatus("MovespeedStatus");
		//AddStatus("MovespeedStatus");

		InitializeStatsClient();
		RefreshStatusHud();
	}

	[ClientRpc]
	public void InitializeStatsClient()
    {
		Nametag?.SetVisible(true);
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
		//Utils.DrawCircle(HitboxPos, 1.4f, 18, Time.Now, Color.Red);
		//Log.Info("local player: " + (Game.Client != null));
		//DebugText("IsChoosingLevelUpReward: " + IsChoosingLevelUpReward);
		//DebugText(NumDashesAvailable + " / " + NumDashes + "\n\n" + _dashTimer);
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		//Log.Info("local player: " + (Game.Client != null));
		//DebugText("\n\nClient - Statuses: " + Statuses);
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if (Game.IsGameOver)
			return;

		float dt = Time.Delta;

		// garry: pass the mouse offset/aim in the Input command instead of by console command
		MouseOffset = Input.Cursor.Direction;

        Vector2 inputVector = new Vector2(-Input.Left, Input.Forward);

		if(inputVector.LengthSquared > 0f)
            Velocity += inputVector.Normal * MoveSpeed * BASE_MOVE_SPEED * dt;

		Position += (Velocity + _dashVelocity) * dt;
		Velocity = Utils.DynamicEaseTo(Velocity, Vector2.Zero, 0.2f, dt);
		_dashVelocity *= (1f - dt * 7.95f);
		TempWeight *= (1f - dt * 4.7f);

		ShadowScale = IsDashing ? Utils.MapReturn(DashProgress, 0f, 1f, 1.12f, 0.75f, EasingType.SineInOut) : 1.12f;

		if (Velocity.LengthSquared > 0.01f && inputVector.LengthSquared > 0.1f)
        {
			AnimationPath = "textures/sprites/player_walk.frames";
			AnimationSpeed = Utils.Map(Velocity.Length, 0f, 2f, 1.5f, 2f);
		} 
		else
        {
			AnimationPath = "textures/sprites/player_idle.frames";
			AnimationSpeed = 0.66f;
		}

		HandleBounds();

		Rotation = Velocity.Length * MathF.Cos(Time.Now * MathF.PI * 7f) * 1.5f;

		Depth = -Position.y * 10f;

		if (MathF.Abs(Input.Left) > 0f)
			Scale = new Vector2(1f * Input.Left < 0f ? -1f : 1f, 1f) * 1f;

		if(HealthRegen > 0f)
        {
			Health += HealthRegen * dt;
			if(Health > MaxHp)
				Health = MaxHp;
        }

		//Rotation = (MathF.Atan2(MouseOffset.y, MouseOffset.x) * (180f / MathF.PI)) - 90f;
		//Scale = new Vector2( MathF.Sin( Time.Now * 4f ) * 1f + 2f, MathF.Sin( Time.Now * 3f ) * 1f + 2f );

		//DebugOverlay.Text(Position.ToString(), Position);

		//DebugOverlay.Text(Position.ToString() + "\n" + Game.GetGridSquareForPos(Position).ToString(), Position + new Vector2(0.2f, 0f));
		//DebugOverlay.Line(Position, Position + new Vector2(0.01f, 0.01f), 0f, false);

		AimDir = MouseOffset.Normal;

        if (ArrowAimer != null)
        {
            ArrowAimer.LocalRotation = (MathF.Atan2(AimDir.y, AimDir.x) * (180f / MathF.PI));
            ArrowAimer.Position = Position + new Vector2(0f, 0.4f) + AimDir * 0.65f;
        }

		if (cl.IsBot)
        {
			MouseOffset = new Vector2(Rand.Float(-10f, 10f), Rand.Float(-10f, 10f));
        }

		if (Host.IsServer)
        {
			if (Input.Pressed(InputButton.Run))
			{
				//Game.Restart();
                AddExperience(GetExperienceReqForLevel(Level));
                return;
			}

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

			for (int dx = -1; dx <= 1; dx++)
			{
				for (int dy = -1; dy <= 1; dy++)
				{
					Game.HandleThingCollisionForGridSquare(this, new GridSquare(GridPos.x + dx, GridPos.y + dy), dt);
				}
			}

			//ArrowAimer.Depth = 100f;

			//DebugOverlay.Text(MouseOffset.ToString(), Position + new Vector2(0.2f, 0f));

			HandleDashing(dt);
			HandleStatuses(dt);
			HandleShooting(dt);
			HandleFlashing(dt);
		}
	}

	void HandleDashing(float dt)
    {
		int numDashes = (int)MathF.Round(NumDashes);
		if (NumDashesAvailable < numDashes)
		{
			_dashTimer -= dt;
			DashRechargeProgress = Utils.Map(_dashTimer, DashCooldown, 0f, 0f, 1f);
			if (_dashTimer <= 0f)
			{
				DashRecharged();
			}
		}

		if (_dashInvulnTimer > 0f)
        {
			_dashInvulnTimer -= dt;
			DashProgress = Utils.Map(_dashInvulnTimer, DashInvulnTime, 0f, 0f, 1f);
			if (_dashInvulnTimer <= 0f)
            {
				IsDashing = false;
				ColorTint = Color.White;
				DashFinished();
			} else
            {
				var rand = Rand.Float(0.5f, 1f);
                ColorTint = new Color(rand, rand, rand / 2f);
			}
		}

		if (Input.Pressed(InputButton.Jump) || Input.Pressed(InputButton.PrimaryAttack))
			Dash();
	}

	public void Dash()
    {
		if (NumDashesAvailable <= 0)
			return;

		Vector2 dashDir = Velocity.LengthSquared > 0f ? Velocity.Normal : AimDir;
		_dashVelocity = dashDir * 7f;
		TempWeight = 15f;

		if (NumDashesAvailable == (int)NumDashes)
			_dashTimer = DashCooldown;

		NumDashesAvailable--;
		IsDashing = true;
		_dashInvulnTimer = DashInvulnTime;
		DashProgress = 0f;
		DashRechargeProgress = 0f;

		ForEachStatus(status => status.OnDashStarted());
	}

	public void DashFinished()
    {
		ForEachStatus(status => status.OnDashFinished());
	}

	public void DashRecharged()
    {
		NumDashesAvailable++;
		var numDashes = (int)MathF.Round(NumDashes);
		if (NumDashesAvailable > numDashes)
			NumDashesAvailable = numDashes;

		if(NumDashesAvailable < numDashes)
        {
			_dashTimer = DashCooldown;
			DashRechargeProgress = 0f;
		}
		else
        {
			DashRechargeProgress = 1f;
		}
			
		ForEachStatus(status => status.OnDashRecharged());
	}

	public void ForEachStatus(Action<Status> action)
	{
		foreach (var (_, status) in Statuses)
		{
			action(status);
		}
	}

	[ClientRpc]
	public void RefreshStatusHud()
	{
		Game.Hud.StatusPanel.Refresh();
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
			ReloadProgress = Utils.Map(Timer, ReloadTime, 0f, 0f, 1f);
			Timer -= dt * ReloadSpeed;
			if (Timer <= 0f)
			{
				Reload();
			}
		}
		else
        {
			Timer -= dt * AttackSpeed;
			if (Timer <= 0f)
            {
				Shoot();
				AmmoCount--;

				if (AmmoCount <= 0)
				{
					IsReloading = true;
					Timer += ReloadTime;
				}
				else
				{
					Timer += AttackTime;
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

	public void Shoot()
	{
		//return;

		float start_angle = MathF.Sin(-_shotNum * 2f) * BulletInaccuracy;

		int num_bullets_int = (int)NumBullets;
		float currAngleOffset = num_bullets_int == 1 ? 0f : -BulletSpread * 0.5f;
		float increment = num_bullets_int == 1 ? 0f : BulletSpread / (float)(num_bullets_int - 1);

		for (int i = 0; i < num_bullets_int; i++)
		{
			var dir = Utils.RotateVector(AimDir, start_angle + currAngleOffset + increment * i);
			var bullet = new Bullet
			{
				Position = Position,
				Depth = -1f,
				Velocity = dir * BulletSpeed,
				Shooter = this,
				Damage = BulletDamage,
				Force = BulletForce,
				TempWeight = 3f,
				Lifetime = BulletLifetime,
				NumPiercing = (int)MathF.Round(BulletNumPiercing),
				CriticalChance = CritChance,
				CriticalMultiplier = CritMultiplier,
				Scale = new Vector2(BulletSize, BulletSize),
				Radius = BulletSize * 0.6f,
			};

			Game.AddThing(bullet);
		}

		_shotNum++;
	}

	void Reload()
    {
		AmmoCount = (int)MaxAmmoCount;
		IsReloading = false;
		_shotNum = 0;
		ReloadProgress = 0f;
	}

	void HandleBounds()
    {
		var x_min = Game.BOUNDS_MIN.x + Radius;
		var x_max = Game.BOUNDS_MAX.x - Radius;
		var y_min = Game.BOUNDS_MIN.y;
		var y_max = Game.BOUNDS_MAX.y - Radius * 5.2f;

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

	public override void BuildInput(InputBuilder inputBuilder)
	{
		base.BuildInput(inputBuilder);

		// To account for bullets being raised off the ground
		var aimOffset = new Vector2( 0f, 0.4f );

		// garry: use put the mouse offset in the input
        MouseOffset = Game.MainCamera.ScreenToWorld(Game.Hud.MousePosition) - Position - aimOffset;
		inputBuilder.Cursor = new Ray(0, MouseOffset);
    }

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		var DIST = 7.3f;
		Game.MainCamera.Position = new Vector2(MathX.Clamp(Position.x, -DIST, DIST), MathX.Clamp(Position.y, -DIST, DIST));

		//MouseOffset = MyGame.Current.MainCamera.ScreenToWorld(MainHud.MousePos) - Position;

		
		//MouseOffset = Game.MainCamera.ScreenToWorld(Game.Hud.RootPanel.MousePosition) - Position;

		//SetMouseOffset(MouseOffset);

		//PostProcess.Clear();
	}

	[ConCmd.Server]
	public static void SetMouseOffset(Vector2 offset)
    {
		if (ConsoleSystem.Caller.Pawn is PlayerCitizen p)
        {
			p.MouseOffset = offset;
        }
    }

	public void Damage(float damage)
    {
		if (IsDashing)
			// show DODGED! floater
			return;

		Health -= damage;
		DamageNumbers.Create(Position + new Vector2(Rand.Float(0.5f, 4f), Rand.Float(8.5f, 10.5f)) * 0.1f, MathX.CeilToInt(damage), DamageType.Player);
		Flash(0.125f);

		if(Health <= 0f)
        {
			Die();
		}
    }

	public void Die()
    {
		if (IsDead)
			return;

		IsDead = true;
		Game.PlayerDied(this);
		//EnableDrawing = false;
		//ColorTint = new Color(0f, 0f, 0f, 0f);
		DieClient();
	}

	[ClientRpc]
	public void DieClient()
    {
		Nametag.SetVisible(false);
	}

	public override void Colliding(Thing other, float percent, float dt)
	{
		base.Colliding(other, percent, dt);

		//if (IsDashing)
		//	return;

		if ((other is Enemy enemy && !enemy.IsDying && (!enemy.IsSpawning || enemy.ElapsedTime < 0.5f)) || other is PlayerCitizen)
		{
			Velocity += (Position - other.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 100f) * (1f + other.TempWeight) * dt;
		}
	}

	[ConCmd.Server("add_status")]
	public static void AddStatusCmd(int typeIdentity)
    {
        Log.Info("AddStatusCmd: " + typeIdentity);
		TypeDescription type = StatusManager.IdentityToType(typeIdentity);
		Log.Info("type: " + type);

		var player = ConsoleSystem.Caller.Pawn as PlayerCitizen;
		player.AddStatus(type);
	}

	public void AddStatus(TypeDescription type)
    {
		Log.Info("AddStatus: " + type.TargetType.ToString());

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

	public int GetStatusLevel(TypeDescription type)
    {
		if(Statuses.ContainsKey(type.Identity))
			return Statuses[type.Identity].Level;

		return 0;
	}

	public void Modify(Status caller, string propertyName, float value, ModifierType type, float priority = 0f, bool update = true)
    {
		//Log.Info("------------- Modify - caller: " + caller + ", " + propertyName + ", " + value + ", " + type);

		if (!_modifiers.ContainsKey(caller))
			_modifiers.Add(caller, new Dictionary<string, ModifierData>());

		_modifiers[caller][propertyName] = new ModifierData(value, type, priority);

		if (update)
			UpdateProperty(propertyName);
    }

	void UpdateProperty(string propertyName)
    {
		if (!_original_properties.ContainsKey(propertyName))
		{
			var property = TypeLibrary.GetDescription<PlayerCitizen>().GetProperty(propertyName);
			_original_properties.Add(propertyName, (float)property.GetValue(this));
			//Log.Info ("... storing original property... - " + propertyName + ": " + ((float)property.GetValue(this)));
		}

		float curr_value = _original_properties[propertyName];
		float curr_set = curr_value;
		bool should_set = false;
		float curr_priority = 0f;
		float total_add = 0f;
		float total_mult = 1f;

		foreach (Status caller in _modifiers.Keys)
        {
			var dict = _modifiers[caller];
			if (dict.ContainsKey(propertyName))
			{
				var mod_data = dict[propertyName];
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

		SetProperty(propertyName, curr_value);
    }

	//public void RemoveStatus(Status status)
 //   {
	//	Log.Info("RemoveStatus: " + status);
	//	RemoveModifiers(status);
	//	_statusesToRemove.Add(status);
 //   }

	public void RemoveModifiers(Status status)
    {
		if (!_modifiers.ContainsKey(status))
			return;

		var dict = _modifiers[status];
		foreach(string propertyName in dict.Keys)
        {
			dict.Remove(propertyName);
			UpdateProperty(propertyName);
        }

        _modifiers.Remove(status);
    }

	void SetProperty(string propertyName, float value)
    {
		var property = TypeLibrary.GetDescription<PlayerCitizen>().GetProperty(propertyName);
		if (property == null)
        {
			Log.Error("property " + propertyName + " doesn't exist!");
			return;
        }

		TypeLibrary.SetProperty(this, propertyName, value);
	}

	public void AddExperience(int xp)
    {
		Host.AssertServer();

		ExperienceTotal += xp;
		ExperienceCurrent += xp;

		if (!IsChoosingLevelUpReward)
			CheckForLevelUp();
    }

	public void CheckForLevelUp()
    {
		//Log.Info("CheckForLevelUp: " + ExperienceCurrent + " / " + ExperienceRequired + " IsServer: " + Host.IsServer + " Level: " + Level);
		if (ExperienceCurrent >= ExperienceRequired)
			LevelUp();
	}

	public void LevelUp()
    {
		Host.AssertServer();

		ExperienceCurrent -= ExperienceRequired;

		Level++;
		ExperienceRequired = GetExperienceReqForLevel(Level + 1);

		//Log.Info("Level Up - now level: " + Level + " IsServer: " + Host.IsServer);

		IsChoosingLevelUpReward = true;

		LevelUpClient();
    }

	[ClientRpc]
	public void LevelUpClient()
    {
		Game.Hud.SpawnChoicePanel();
	}

	public int GetExperienceReqForLevel(int level)
    {
		//return 3 + level + (int)MathF.Round(Utils.Map(level, 1, 100, 0f, 1000f, EasingType.SineIn));
		return (int)MathF.Round(Utils.Map(level, 1, 100, 3f, 1500f, EasingType.SineIn));
	}

	public void Flash(float time)
	{
		if (_isFlashing)
			return;

		ColorTint = new Color(1f, 0f, 0f);
		_isFlashing = true;
		_flashTimer = time;
	}
}
