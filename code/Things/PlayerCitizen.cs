using Sandbox;
using System;
using System.Diagnostics;
using System.Linq;
using static Sandbox.MyGame;
using System.Collections.Generic;

namespace Sandbox;

public partial class PlayerCitizen : Thing
{
	public Vector2 MouseOffset { get; private set; }

	[Net, Predicted] public Arrow ArrowAimer { get; private set; }
	public Vector2 AimDir { get; private set; }

	public bool IsAlive { get; private set; }

	public float FeetOffset { get; private set; }

	public float Timer { get; protected set; }
	public float ShotDelay { get; protected set; }
	public int AmmoCount { get; protected set; }
	public int MaxAmmoCount { get; protected set; }

	public bool IsReloading { get; protected set; }
	public float ReloadTime { get; protected set; }

	public float AttackSpeed { get; private set; }
	public float ReloadSpeed { get; private set; }


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

			Timer = ShotDelay = 0.125f;
			AmmoCount = MaxAmmoCount = 6;
			ReloadTime = 1.25f;
			ReloadSpeed = 1f;
			AttackSpeed = 1f;

			Health = 100f;
			IsAlive = true;
			Radius = 0.2f;
			GridPos = Game.GetGridSquareForPos(Position);
			AimDir = Vector2.Up;

			SetProperty("AttackSpeed", 10f);
			SetProperty("MaxAmmoCount", 40);
			SetProperty("ReloadTime", 12f);
		}
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		float dt = Time.Delta;
		
		Velocity += new Vector2(-Input.Left, Input.Forward) * 30f * Time.Delta;
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

			HandleShooting(dt);
		}
	}

	void HandleShooting(float dt)
    {
		if (IsReloading)
		{
			Timer -= dt * ReloadSpeed;
			if (Timer <= 0f)
			{
				IsReloading = false;
				AmmoCount = MaxAmmoCount;
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
					Timer += ShotDelay;
				}
			}
		}

		DebugText(AmmoCount.ToString());
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

		MouseOffset = MyGame.Current.MainCamera.ScreenToWorld(MainHud.MousePos) - Position;
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

	void SetProperty(string propertyName, int value)
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
