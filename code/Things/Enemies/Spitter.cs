using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public partial class Spitter : Enemy
{
    private TimeSince _damageTime;
    private const float DAMAGE_TIME = 0.75f;

    private float _shootDelayTimer;
    private const float SHOOT_DELAY_MIN = 2f;
    private const float SHOOT_DELAY_MAX = 3f;

    public bool IsShooting { get; private set; }
    private float _shotTimer;
    private const float SHOOT_TIME = 1f;

    public override void Spawn()
    {
        base.Spawn();

        if (Host.IsServer)
        {
            SpriteTexture = SpriteTexture.Atlas("textures/sprites/spitter.png", 5, 6);
            //AnimationPath = "textures/sprites/zombie_spawn.frames";
            //AnimIdlePath = "textures/sprites/zombie_walk.frames";
            AnimationSpeed = 2f;
            Pivot = new Vector2(0.5f, 0.05f);

            Radius = 0.225f;
            Health = 26f;
            MaxHealth = Health;
            DamageToPlayer = 9f;

            ScaleFactor = 0.75f;
            Scale = new Vector2(1f, 1f) * ScaleFactor;

            CollideWith.Add(typeof(Enemy));
            CollideWith.Add(typeof(PlayerCitizen));

            ShadowScale = 0.925f;
            _damageTime = DAMAGE_TIME;
            _shootDelayTimer = Rand.Float(SHOOT_DELAY_MIN, SHOOT_DELAY_MAX);

            AnimationPath = AnimSpawnPath;
        }
    }

    public override void Update(float dt)
    {
        if (Game.IsGameOver)
            return;

        base.Update(dt);
    }

    protected override void UpdatePosition(float dt)
    {
        base.UpdatePosition(dt);

        var closestPlayer = Game.GetClosestPlayer(Position);
        if (closestPlayer == null)
            return;

        if(IsShooting)
        {
            _shotTimer -= dt;
            if(_shotTimer < 0f)
            {
                Shoot();
                return;
            }
        } 
        else
        {
            Velocity += (closestPlayer.Position - Position).Normal * 1.0f * dt;
        }

        float speed = 0.9f * (IsAttacking ? 1.3f : 0.7f) + Utils.FastSin(MoveTimeOffset + Time.Now * (IsAttacking ? 15f : 7.5f)) * (IsAttacking ? 0.66f : 0.35f);
        Position += Velocity * dt * speed;

        var player_dist_sqr = (closestPlayer.Position - Position).LengthSquared;
        if (!IsShooting && !IsAttacking && player_dist_sqr < 5f * 5f)
        {
            _shootDelayTimer -= dt;
            if(_shootDelayTimer < 0f)
            {
                PrepareToShoot();
            }
        }
    }

    public void PrepareToShoot()
    {
        _shotTimer = SHOOT_TIME;
        IsShooting = true;
    }

    public void Shoot()
    {
        var closestPlayer = Game.GetClosestPlayer(Position);
        if (closestPlayer == null)
            return;

        var target_pos = closestPlayer.Position + closestPlayer.Velocity * 1.5f;
        var dir = Utils.RotateVector((target_pos - Position).Normal, Rand.Float(-10f, 10f));
        var bullet = new EnemyBullet
        {
            Position = Position,
            Depth = -1f,
            Direction = dir,
            Shooter = this,
        };

        Game.AddThing(bullet);

        Velocity *= 0.25f;
        _shootDelayTimer = Rand.Float(SHOOT_DELAY_MIN, SHOOT_DELAY_MAX);
        IsShooting = false;
    }

    public override void Colliding(Thing other, float percent, float dt)
    {
        base.Colliding(other, percent, dt);

        if (other is Enemy enemy && !enemy.IsDying)
        {
            var spawnFactor = Utils.Map(enemy.ElapsedTime, 0f, enemy.SpawnTime, 0f, 1f, EasingType.QuadIn);
            Velocity += (Position - enemy.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * 10f * (1f + enemy.TempWeight) * spawnFactor * dt;
        }
        else if (other is PlayerCitizen player)
        {
            if(!player.IsDead)
            {
                Velocity += (Position - player.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * 5f * (1f + player.TempWeight) * dt;

                if (IsAttacking && _damageTime >= DAMAGE_TIME)
                {
                    float damageDealt = player.Damage(DamageToPlayer);

                    if (damageDealt > 0f && player.ThornsPercent > 0f)
                    {
                        Damage(damageDealt * player.ThornsPercent, player, false);
                    }

                    _damageTime = 0f;
                }
            }
        }
    }
}
