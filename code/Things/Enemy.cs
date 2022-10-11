using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public partial class Enemy : Thing
{
	public float MoveTimeOffset { get; set; }
	public float MoveTimeSpeed { get; set; }

	private float _flashTimer;
	private bool _isFlashing;

	public float MaxHealth { get; private set; }

	public bool IsSpawning { get; private set; }
	public float ElapsedTime { get; private set; }
	public bool IsDying { get; private set; }
	public float DeathTimeElapsed { get; private set; }
	private Vector2 _deathScale;

	public bool IsAttacking { get; private set; }
	private float _aggroTimer;
	private const float AGGRO_RANGE = 1.4f;
	private const float AGGRO_START_TIME = 0.2f;
	private const float AGGRO_LOSE_TIME = 0.4f;

	public float DamageToPlayer { get; private set; }

	public static float SCALE_FACTOR = 0.8f;

	private Shadow _shadow;

    public override void Spawn()
	{
		base.Spawn();

		//TexturePath = "textures/sprites/mummy_walk3.png";
		//TexturePath = "textures/sprites/zombie_bw.png";
		//TexturePath = "textures/sprites/mummy_walk3.png";
		//SpriteTexture = "textures/sprites/zombie.png";

		if (Host.IsServer)
		{
			SpriteTexture = SpriteTexture.Atlas("textures/sprites/zombie_spritesheet3.png", 5, 6);
			AnimationPath = "textures/sprites/zombie_spawn.frames";
			AnimationSpeed = 2f;
			Pivot = new Vector2(0.5f, 0.05f);

			Radius = 0.25f;
			Health = 30f;
			MaxHealth = Health;
			MoveTimeOffset = Rand.Float(0f, 4f);
			MoveTimeSpeed = Rand.Float(6f, 9f);
			DamageToPlayer = 10f;

			IsSpawning = true;
			ElapsedTime = 0f;

			Scale = new Vector2(1f, 1f) * SCALE_FACTOR;

			CollideWith.Add(typeof(Enemy));
			CollideWith.Add(typeof(PlayerCitizen));
		}

		//Scale = new Vector2(1f, 35f / 16f) * 0.5f;
		//RenderColor = Color.Random;
		//Rotation = Time.Now * 0.1f;

        Filter = SpriteFilter.Pixelated;
		//ColorFill = new ColorHsv(Rand.Float(0f, 360f), 0.5f, 1f, 0.125f);
		ColorFill = new ColorHsv(0f, 0f, 0f, 0f);
	}

    public override void ClientSpawn()
    {
        base.ClientSpawn();

		_shadow = new Shadow();
		_shadow.SetThing(this);
	}

    [Event.Tick.Client]
    public void ClientTick()
    {
        //DebugText(AnimationTimeElapsed.ToString());
    }

    [Event.Tick.Server]
    public void ServerTick()
    {
		//DebugText(IsAttacking.ToString());
		//DebugText(SinceSpawning.Absolute.ToString("#.##"));
	}

	public override void Update(float dt)
    {
		if (Game.IsGameOver)
			return;

		base.Update(dt);
		ElapsedTime += dt;

		if (_isFlashing)
		{
			_flashTimer -= dt;
			if (_flashTimer < 0f)
			{
				_isFlashing = false;
				ColorFill = new ColorHsv(0f, 0f, 0f, 0f);
			}
		}

		if (IsDying)
        {
            DeathTimeElapsed += dt;
			Scale = _deathScale * Utils.Map(DeathTimeElapsed, 0f, 0.3f, 1f, 1.2f);

            if (DeathTimeElapsed > 0.3f)
            {
                Remove();
            }
			return;
        }

        if (IsSpawning)
        {
			Depth = -Position.y * 10f;

			if (ElapsedTime > 1.75f)
            {
				IsSpawning = false;
				AnimationPath = "textures/sprites/zombie_walk.frames";
			} 
			else
            {
				return;
            }
		}

		var closestPlayer = Game.GetClosestPlayer(Position);
		if (closestPlayer == null)
			return;

		Velocity += (closestPlayer.Position - Position).Normal * 1.0f * dt;
		float speed = 0.7f + Utils.FastSin(MoveTimeOffset + Time.Now * MoveTimeSpeed) * 0.3f * (IsAttacking ? 1.25f : 1f);
		Position += Velocity * dt * speed;
		//Position = new Vector2(MathX.Clamp(Position.x, Game.BOUNDS_MIN.x + Radius, Game.BOUNDS_MAX.x - Radius), MathX.Clamp(Position.y, Game.BOUNDS_MIN.y + Radius, Game.BOUNDS_MAX.y - Radius));
		Position = new Vector2(MathX.Clamp(Position.x, Game.BOUNDS_MIN.x + Radius, Game.BOUNDS_MAX.x - Radius), MathX.Clamp(Position.y, Game.BOUNDS_MIN.y + Radius, Game.BOUNDS_MAX.y - Radius));
		Velocity *= (1f - dt * 1.47f);

		//if (MathF.Abs(Velocity.x) > 0.2f)
		//	Scale = new Vector2(1f * Velocity.x < 0f ? 1f : -1f, 1f) * SCALE_FACTOR;

		//enemy.Rotation = enemy.Velocity.LengthSquared * Utils.FastSin(Time.Now * 12f);
		//enemy.Rotation = enemy.Velocity.Length * Utils.FastSin(Time.Now * MathF.PI * 7f) * 4.5f;

		//DebugOverlay.Line(enemy.Position, enemy.Position + enemy.Radius, 0f, false);
		//DebugText(Health.ToString("#.") + "/" + MaxHealth.ToString("#."));

		//DebugText(MathF.Abs(Velocity.x).ToString("#.#"));

		Depth = -Position.y * 10f;

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

		TempWeight *= (1f - dt * 4.7f);

		float dist_sqr = (closestPlayer.Position - Position).LengthSquared;
		float attack_dist_sqr = MathF.Pow(AGGRO_RANGE, 2f);

		if (!IsAttacking)
		{
			if (dist_sqr < attack_dist_sqr)
			{
				_aggroTimer += dt;
				if(_aggroTimer > AGGRO_START_TIME)
                {
					IsAttacking = true;
					AnimationPath = "textures/sprites/zombie_attack.frames";
					_aggroTimer = 0f;
				}
			}
			else
			{
				//DebugText(IsAttacking.ToString() + ", " + MathF.Abs(Velocity.x).ToString("#.##"));
				AnimationSpeed = Utils.Map(speed, 0.4f, 1f, 0.75f, 3f, EasingType.ExpoIn);
				_aggroTimer = 0f;
			}

			if (MathF.Abs(Velocity.x) > 0.175f)
				Scale = new Vector2(1f * Velocity.x < 0f ? 1f : -1f, 1f) * SCALE_FACTOR;
		}
		else
		{
			if (dist_sqr > attack_dist_sqr)
			{
				//DebugText(_lastAggroTime.Relative.ToString("#.##"));
				_aggroTimer += dt;
				if (_aggroTimer > AGGRO_LOSE_TIME)
				{
					IsAttacking = false;
					AnimationPath = "textures/sprites/zombie_walk.frames";
				}

				//IsAttacking = false;
				//AnimationPath = "textures/sprites/zombie_walk.frames";
			}
			else
			{
				AnimationSpeed = Utils.Map(dist_sqr, attack_dist_sqr, 0f, 1f, 4f, EasingType.Linear);
				_aggroTimer = 0f;
			}

			Scale = new Vector2(1f * closestPlayer.Position.x < Position.x ? 1f : -1f, 1f) * SCALE_FACTOR;
		}

		//ColorFill = new ColorHsv(0f, 0f, 0f, 0f);
		//ColorFill = new ColorHsv(0.5f + Utils.FastSin(Time.Now * 4f) * 0.5f, 0.5f + Utils.FastSin(Time.Now * 3f) * 0.5f, 0.5f + Utils.FastSin(Time.Now * 2f) * 0.5f, 0.5f + Utils.FastSin(Time.Now * 1f) * 0.5f);
		//ColorFill = new ColorHsv(0.94f, 0.157f, 0.392f, 1f);
		//ColorFill = new ColorHsv(0f, 0f, 0f, 0f);

		//ColorFill = new Color(0.2f, 0.2f, 1f) * 1.5f; // frozen
		//ColorFill = new Color(1f, 1f, 0.1f) * 2.5f; // shock
		//ColorFill = new Color(0.1f, 1f, 0.1f) * 2.5f; // poison

		//ColorFill = Color.White;
		//ColorFill = new ColorHsv(1f, 0f, 0f, 0f);
	}

    public override void Colliding(Thing other, float percent, float dt)
    {
        base.Colliding(other, percent, dt);

		if (other is Enemy enemy && !enemy.IsDying)
		{
			Velocity += (Position - enemy.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * 10f * (1f + enemy.TempWeight) * dt;
		}
		else if (other is PlayerCitizen player)
		{
			Velocity += (Position - player.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * 5f * (1f + player.TempWeight) * dt;

			if(IsAttacking)
            {
				player.Damage(DamageToPlayer * dt);
            }
		}
	}

    public void Damage(float damage, PlayerCitizen shooter, bool isCrit)
    {
		if (IsDying)
			return;

		Health -= damage;
		DamageNumbers.Create(Position + new Vector2(Rand.Float(-1f, 1f), Rand.Float(-2f, 2f)) * 0.1f, damage, isCrit);
		Flash(0.12f);

		if (Health <= 0f)
        {
			StartDying(shooter);
		}
    }

	public void StartDying(PlayerCitizen shooter)
    {
		IsDying = true;
		DeathTimeElapsed = 0f;
		AnimationPath = "textures/sprites/zombie_die.frames";
		AnimationSpeed = 5.5f;

		_isFlashing = false;
		ColorFill = new ColorHsv(0f, 0f, 0f, 0f);

		_deathScale = Scale;

		var coin_chance = shooter != null ? Utils.Map(shooter.Luck, 0f, 10f, 0.5f, 1f) : 0.5f;
		if(Rand.Float(0f, 1f) < coin_chance)
			Game.SpawnCoin(Position);
	}

	public void Flash(float time)
    {
		if (_isFlashing)
			return;

		ColorFill = new ColorHsv(1f, 0f, 15f, 1f);
		_isFlashing = true;
		_flashTimer = time;
	}
}
