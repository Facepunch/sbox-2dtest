using Sandbox;
using System;
using System.Diagnostics;
using System.Linq;
using static Sandbox.MyGame;
using System.Collections.Generic;

namespace Sandbox;

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

	public bool IsAlive { get; private set; }

	public float FeetOffset { get; private set; }

	public float Timer { get; protected set; }
	[Net] public float AttackTime { get; protected set; }
	[Net] public float AttackSpeed { get; private set; }
	public bool IsReloading { get; protected set; }
	[Net] public float ReloadTime { get; protected set; }
	[Net] public float ReloadSpeed { get; private set; }
	[Net] public int AmmoCount { get; protected set; }
	[Net] public float MaxAmmoCount { get; protected set; }
	[Net] public float Dmg { get; protected set; }
	[Net] public float MoveSpeed { get; protected set; }
	public const float BASE_MOVE_SPEED = 30f;
	[Net] public float NumBullets { get; protected set; }
	[Net] public float BulletSpread { get; protected set; }
	[Net] public float BulletSpeed { get; protected set; }

	private int _shotNum;

	[Net] public float CoinAttractRange { get; protected set; }
	[Net] public float CoinAttractStrength { get; protected set; }
	[Net] public float Luck { get; protected set; }

	[Net] public int Level { get; protected set; }
	[Net] public int ExperienceTotal { get; protected set; }
	[Net] public int ExperienceCurrent { get; protected set; }
	[Net] public int ExperienceRequired { get; protected set; }
	[Net] public float MaxHp { get; protected set; }

	[Net] public float DashCooldown { get; private set; }
	private float _dashTimer;
	public bool IsDashing => _dashTimer > 0f;
	private Vector2 _dashVelocity;

	private float _flashTimer;
	private bool _isFlashing;

	// STATUS
	[Net] public IList<Status> Statuses { get; private set; }

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
			AnimationPath = "textures/sprites/player_idle.frames";
			AnimationSpeed = 0.66f;
			Pivot = new Vector2(0.5f, 0.05f);

			AttackTime = 1f;
			Timer = AttackTime;
			AmmoCount = 5;
			MaxAmmoCount = AmmoCount;
			ReloadTime = 1.25f;
			ReloadSpeed = 1f;
			AttackSpeed = 1f;
			Dmg = 10f;
			MoveSpeed = 1f;
			NumBullets = 1f;
			BulletSpread = 35f;
			BulletSpeed = 7.5f;
			Luck = 1f;
			Level = 0;
			ExperienceRequired = GetExperienceReqForLevel(Level + 1);
			DashCooldown = 0.5f;

			Health = 100f;
			MaxHp = 100f;
			IsAlive = true;
			Radius = 0.2f;
			GridPos = Game.GetGridSquareForPos(Position);
			AimDir = Vector2.Up;

			CoinAttractRange = 1.7f;
			CoinAttractStrength = 1.5f;

			CollideWith.Add(typeof(Enemy));
			CollideWith.Add(typeof(PlayerCitizen));

			//SetProperty("AttackSpeed", 1f);
			//SetProperty("MaxAmmoCount", 3);
			//SetProperty("ReloadTime", 1f);

			//Modify("AttackSpeed", 0.5f, ModifierType.Add);
			//Modify("AttackSpeed", 2f, ModifierType.Mult);
			Statuses = new List<Status>();
			AddStatus(new ExampleStatus());
			AddStatus(new ExampleStatus2());
		}
	}

    public override void ClientSpawn()
    {
        base.ClientSpawn();
		
        ArrowAimer = new Arrow
        {
            Parent = this,
            //LocalPosition = new Vector3(0.3f, 0f, 0f);
            Depth = 100f
        };
    }

    [Event.Tick.Client]
	public void ClientTick()
	{
		//Log.Info("local player: " + (Game.Client != null));
		//DebugText(AnimationTimeElapsed.ToString());
	}

	[Event.Tick.Server]
	public void ServerTick()
	{
		//Utils.DrawCircle(HitboxPos, 1.4f, 18, Time.Now, Color.Red);
		//Log.Info("local player: " + (Game.Client != null));
		DebugText(IsDashing.ToString() + "\n" + _dashVelocity + "\ntemp weight: " + TempWeight);
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		float dt = Time.Delta;

		Vector2 inputVector = new Vector2(-Input.Left, Input.Forward);
		Velocity += inputVector * MoveSpeed * BASE_MOVE_SPEED * dt;
		Position += (Velocity + _dashVelocity) * dt;
		Velocity = Utils.DynamicEaseTo(Velocity, Vector2.Zero, 0.2f, dt);
		_dashVelocity *= (1f - dt * 3.95f);
		TempWeight *= (1f - dt * 4.7f);

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

		//Rotation = (MathF.Atan2(MouseOffset.y, MouseOffset.x) * (180f / MathF.PI)) - 90f;
		//Scale = new Vector2( MathF.Sin( Time.Now * 4f ) * 1f + 2f, MathF.Sin( Time.Now * 3f ) * 1f + 2f );

		//DebugOverlay.Text(Position.ToString(), Position);

		//DebugOverlay.Text(Position.ToString() + "\n" + Game.GetGridSquareForPos(Position).ToString(), Position + new Vector2(0.2f, 0f));
		//DebugOverlay.Line(Position, Position + new Vector2(0.01f, 0.01f), 0f, false);

        AimDir = MouseOffset.Normal;

        if (ArrowAimer != null)
        {
            ArrowAimer.LocalRotation = (MathF.Atan2(AimDir.y, AimDir.x) * (180f / MathF.PI));
            ArrowAimer.Position = Position + AimDir * 0.65f;
        }

		if (cl.IsBot)
        {
			MouseOffset = new Vector2(Rand.Float(-10f, 10f), Rand.Float(-10f, 10f));
        }

		if (Host.IsServer)
        {
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

			if (_dashTimer > 0f)
			{
				_dashTimer -= dt;
				if(_dashTimer <= 0f)
                {
					DashRecharged();
				}
			}
			if (Input.Pressed(InputButton.Jump) || Input.Pressed(InputButton.PrimaryAttack))
				Dash();

			HandleStatuses(dt);
			HandleShooting(dt);
			HandleFlashing(dt);
		}
		else // Client
        {
			Game.Hud.ToolsPanel.Refresh();
        }
	}

	public void Dash()
    {
		if (IsDashing)
			return;

		Vector2 dashDir = Velocity.LengthSquared > 0f ? Velocity.Normal : AimDir;
		_dashVelocity = dashDir * 5f;
		TempWeight = 54f;
		_dashTimer = DashCooldown;
    }

	public void DashRecharged()
    {

    }

	void HandleStatuses(float dt)
    {
		string debug = "";
		for(int i = Statuses.Count - 1; i >= 0; i--)
		{
			Status status = Statuses[i];
			if (status.ShouldUpdate)
				status.Update(dt);

			debug += status.ToString() + "\n";
			//debug += ": " + status.ElapsedTime + "\n";
		}

		//DebugText(debug);
    }

	void HandleShooting(float dt)
    {
		if (IsReloading)
		{
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
				ColorTint = new Color(1f, 1f, 1f);
			}
		}
	}

	void Shoot()
	{
		//return;

		float start_angle = MathF.Sin(-_shotNum * 2f) * 10f;

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
				Damage = Dmg,
				Force = 2.25f,
				TempWeight = 3f,
				Lifetime = 1.5f,
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
	}

	void HandleBounds()
    {
		var BUFFER_X = Radius;
		var BUFFER_Y = Radius * 1.5f;

		if (Position.x < Game.BOUNDS_MIN.x + BUFFER_X)
		{
			Position = new Vector2(Game.BOUNDS_MIN.x + BUFFER_X, Position.y);
			Velocity = new Vector2(Velocity.x * -1f, Velocity.y);
		}
		else if (Position.x > Game.BOUNDS_MAX.x - BUFFER_X)
		{
			Position = new Vector2(Game.BOUNDS_MAX.x - BUFFER_X, Position.y);
			Velocity = new Vector2(Velocity.x * -1f, Velocity.y);
		}

		if (Position.y < Game.BOUNDS_MIN.y + BUFFER_Y)
		{
			Position = new Vector2(Position.x, Game.BOUNDS_MIN.y + BUFFER_Y);
			Velocity = new Vector2(Velocity.x, Velocity.y * -1f);
		}
		else if (Position.y > Game.BOUNDS_MAX.y - BUFFER_Y)
		{
			Position = new Vector2(Position.x, Game.BOUNDS_MAX.y - BUFFER_Y);
			Velocity = new Vector2(Velocity.x, Velocity.y * -1f);
		}
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		var DIST = 7.3f;
		Game.MainCamera.Position = new Vector2(MathX.Clamp(Position.x, -DIST, DIST), MathX.Clamp(Position.y, -DIST, DIST));

		//MouseOffset = MyGame.Current.MainCamera.ScreenToWorld(MainHud.MousePos) - Position;

		MouseOffset = Game.MainCamera.ScreenToWorld(Game.Hud.MousePosition) - Position;
		//MouseOffset = Game.MainCamera.ScreenToWorld(Game.Hud.RootPanel.MousePosition) - Position;

        SetMouseOffset(MouseOffset);

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
		Health -= damage;
		Flash(0.125f);

		if(Health <= 0f)
        {
			
		}
    }

	public override void Colliding(Thing other, float percent, float dt)
	{
		base.Colliding(other, percent, dt);

		if ((other is Enemy enemy && !enemy.IsDying && (!enemy.IsSpawning || enemy.ElapsedTime < 0.5f)) || other is PlayerCitizen)
		{
			Velocity += (Position - other.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 100f) * (1f + other.TempWeight) * dt;
		}
	}

	public void AddStatus(Status status)
    {
		Statuses.Add(status);
		status.Init(this);
    }

	public void Modify(Status caller, string propertyName, float value, ModifierType type, float priority = 0f, bool update = true)
    {
		Log.Info("------------- Modify - caller: " + caller + ", " + propertyName + ", " + value + ", " + type);

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
			Log.Info ("... storing original property... - " + propertyName + ": " + ((float)property.GetValue(this)));
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

		Log.Info("UpdateProperty: " + propertyName + ": " + curr_value);
		SetProperty(propertyName, curr_value);
    }

	public void RemoveStatus(Status status)
    {
		Log.Info("RemoveStatus: " + status);
		RemoveModifiers(status);
		Statuses.Remove(status);
    }

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
		ExperienceTotal += xp;
		ExperienceCurrent += xp;

		if(ExperienceCurrent >= ExperienceRequired)
			LevelUp();
    }

	public void LevelUp()
    {
		ExperienceCurrent -= ExperienceRequired;

		Level++;
		ExperienceRequired = GetExperienceReqForLevel(Level + 1);
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
