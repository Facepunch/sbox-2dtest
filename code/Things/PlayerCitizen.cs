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

	[Net, Predicted] public Arrow ArrowAimer { get; private set; }
	public Vector2 AimDir { get; private set; }

	public bool IsAlive { get; private set; }

	public float FeetOffset { get; private set; }

	public float Timer { get; protected set; }
	public float AttackTime { get; protected set; }
	public float AttackSpeed { get; private set; }
	public bool IsReloading { get; protected set; }
	public float ReloadTime { get; protected set; }
	public float ReloadSpeed { get; private set; }
	public int AmmoCount { get; protected set; }
	public float MaxAmmoCount { get; protected set; }
	public float NumBullets { get; protected set; }
	public float MoveSpeed { get; protected set; }
	public const float BASE_MOVE_SPEED = 30f;

	// STATUS
	private List<Status> _statuses = new List<Status>();

	// MODIFIERS
	private Dictionary<Status, Dictionary<string, ModifierData>> _modifiers = new Dictionary<Status, Dictionary<string, ModifierData>>();
	private Dictionary<string, float> _original_properties = new Dictionary<string, float>();

	public override void Spawn()
	{
		base.Spawn();

		//TexturePath = "textures/sprites/head.png";
		SpriteTexture = "textures/sprites/citizen.png";
		Filter = SpriteFilter.Pixelated;
		//Scale = new Vector2(1f, 142f / 153f);
		//Scale = new Vector2(1f, 35f / 16f) * 0.5f;

		if (Host.IsServer)
        {
            ArrowAimer = new Arrow();
            ArrowAimer.Parent = this;
			//_arrow.LocalPosition = new Vector3(0.3f, 0f, 0f);
			ArrowAimer.Depth = 100f;
			ArrowAimer.Owner = this;

			Timer = AttackTime = 1f;
			AmmoCount = 5;
			MaxAmmoCount = AmmoCount;
			ReloadTime = 1.25f;
			ReloadSpeed = 1f;
			AttackSpeed = 0.99f;
			MoveSpeed = 1f;

			Health = 100f;
			IsAlive = true;
			Radius = 0.2f;
			GridPos = Game.GetGridSquareForPos(Position);
			AimDir = Vector2.Up;

			//SetProperty("AttackSpeed", 1f);
			//SetProperty("MaxAmmoCount", 3);
			//SetProperty("ReloadTime", 1f);

			//Modify("AttackSpeed", 0.5f, ModifierType.Add);
			//Modify("AttackSpeed", 2f, ModifierType.Mult);

			AddStatus(new ExampleStatus());
			AddStatus(new ExampleStatus2());
		}
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		float dt = Time.Delta;
		
		Velocity += new Vector2(-Input.Left, Input.Forward) * MoveSpeed * BASE_MOVE_SPEED * Time.Delta;
		Position += Velocity * Time.Delta;
		Velocity = Utils.DynamicEaseTo(Velocity, Vector2.Zero, 0.2f, Time.Delta);

		HandleBounds();

		Rotation = Velocity.Length * MathF.Cos(Time.Now * MathF.PI * 7f) * 2f;

		Depth = -Position.y * 10f;

		if (MathF.Abs(Input.Left) > 0f)
			Scale = new Vector2(1f * Input.Left < 0f ? -1f : 1f, 1f) * 1f;

		//Rotation = (MathF.Atan2(MouseOffset.y, MouseOffset.x) * (180f / MathF.PI)) - 90f;
		//Scale = new Vector2( MathF.Sin( Time.Now * 4f ) * 1f + 2f, MathF.Sin( Time.Now * 3f ) * 1f + 2f );

		//DebugOverlay.Text(Position.ToString(), Position);

		//DebugOverlay.Text(Position.ToString() + "\n" + Game.GetGridSquareForPos(Position).ToString(), Position + new Vector2(0.2f, 0f));
		//DebugOverlay.Line(Position, Position + new Vector2(0.01f, 0.01f), 0f, false);

		ArrowAimer.LocalRotation = (MathF.Atan2(MouseOffset.y, MouseOffset.x) * (180f / MathF.PI));
		ArrowAimer.Position = Position + MouseOffset.Normal * 0.65f;

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

			AimDir = (ArrowAimer.Position - Position).Normal;
			//_arrow.LocalPosition = MouseOffset.Normal * 0.65f;

			//DebugOverlay.Text(MouseOffset.ToString(), Position + new Vector2(0.2f, 0f));

			if (Input.Pressed(InputButton.Jump) || Input.Pressed(InputButton.PrimaryAttack))
			{
				var dir = (ArrowAimer.Position - Position).Normal;

				for (int i = -5; i <= 5; i++)
				//for (int i = 0; i <= 0; i++)
				{
					var currDir = Utils.RotateVector(dir, i * 15f);
					var bullet = new Bullet
					{
						Position = Position,
						Depth = -1f,
						Velocity = currDir * 10f,
						Shooter = this
					};

					Game.AddThing(bullet);
				}
			}

			HandleStatuses(dt);
			HandleShooting(dt);
		}
	}

	void HandleStatuses(float dt)
    {
		string debug = "";
		for(int i = _statuses.Count - 1; i >= 0; i--)
		{
			Status status = _statuses[i];
			if (status.ShouldUpdate)
				status.Update(dt);

			debug += status.ToString() + ": " + status.ElapsedTime + "\n";
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
				IsReloading = false;
				AmmoCount = (int)MaxAmmoCount;
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

	void Shoot()
	{
		var bullet = new Bullet
		{
			Position = Position,
			Depth = -1f,
			Velocity = AimDir * 7.5f,
			Shooter = this,
			Damage = 10f,
			Force = 2.25f,
			TempWeight = 3f,
			Lifetime = 1.5f,
		};

		Game.AddThing(bullet);
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

		if(Health <= 0f)
        {
			Scale = new Vector2(2f, 1f);
		}
    }

	public override void Collide(Thing other, float percent, float dt)
	{
		base.Collide(other, percent, dt);

		if (other is Enemy || other is PlayerCitizen)
		{
			Velocity += (Position - other.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 100f) * dt;
		}
	}

	public void AddStatus(Status status)
    {
		_statuses.Add(status);
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
		_statuses.Remove(status);
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
}
