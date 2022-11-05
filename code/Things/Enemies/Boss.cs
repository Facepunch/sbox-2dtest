using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public partial class Boss : Enemy
{
    private TimeSince _damageTime;
    private const float DAMAGE_TIME = 0.75f;

    private float _shootDelayTimer;
    private const float SHOOT_DELAY_MIN = 2f;
    private const float SHOOT_DELAY_MAX = 3f;

    public bool IsShooting { get; private set; }
    private bool _hasShot;

    private TimeSince _prepareShootTime;

    public override void Spawn()
    {
        base.Spawn();

        if (Host.IsServer)
        {
            SpriteTexture = SpriteTexture.Atlas("textures/sprites/spitter.png", 7, 6);

            AnimSpawnPath = "textures/sprites/spitter_spawn.frames";
            AnimIdlePath = "textures/sprites/zombie_walk.frames";
            AnimAttackPath = "textures/sprites/spitter_attack.frames";
            AnimDiePath = "textures/sprites/spitter_die.frames";

            AnimSpeed = 2f;
            BasePivotY = 0.1f;
            HeightZ = 0f;
            //Pivot = new Vector2(0.5f, 0.05f);
            PushStrength = 50f;

            Radius = 0.42f;
            Health = 1000f;
            MaxHealth = Health;
            DamageToPlayer = 9f;

            ScaleFactor = 1.75f;
            Scale = new Vector2(1f, 1f) * ScaleFactor;

            CollideWith.Add(typeof(Enemy));
            CollideWith.Add(typeof(PlayerCitizen));

            ShadowScale = 2.05f;
            _damageTime = DAMAGE_TIME;
            _shootDelayTimer = Rand.Float(SHOOT_DELAY_MIN, SHOOT_DELAY_MAX);

            AnimationPath = AnimSpawnPath;
        }
    }

    public override void Update(float dt)
    {
        if (Game.IsGameOver)
            return;

        Utils.DrawCircle(Position, Radius, 8, Time.Now, Color.Red);

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
            if(!_hasShot && _prepareShootTime > 1.0f)
                Shoot();

            if(_prepareShootTime > 1.6f)
                FinishShooting();

            return;
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
        _prepareShootTime = 0f;
        IsShooting = true;
        _hasShot = false;
        AnimationPath = "textures/sprites/spitter_shoot.frames";
        Game.PlaySfxNearby("spitter.prepare", Position, pitch: Rand.Float(1f, 1.1f), volume: 0.6f, maxDist: 2.75f);
        CanAttack = false;
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
            Position = Position + dir * 0.05f,
            Depth = 1f,
            Direction = dir,
            Shooter = this,
        };

        if(dir.x < 0f)
            bullet.Scale = new Vector2(-bullet.Scale.x, bullet.Scale.y);

        Game.AddThing(bullet);

        Velocity *= 0.25f;
        _hasShot = true;

        Game.PlaySfxNearby("spitter.shoot", Position, pitch: Rand.Float(0.8f, 0.9f), volume: 0.9f, maxDist: 5f);
        AnimationPath = "textures/sprites/spitter_shoot_reverse.frames";
    }

    public void FinishShooting()
    {
        AnimationPath = AnimIdlePath;
        CanAttack = true;
        _shootDelayTimer = Rand.Float(SHOOT_DELAY_MIN, SHOOT_DELAY_MAX);
        IsShooting = false;
    }

    public override void Colliding(Thing other, float percent, float dt)
    {
        base.Colliding(other, percent, dt);

        if (other is Enemy enemy && !enemy.IsDying)
        {
            var spawnFactor = Utils.Map(enemy.ElapsedTime, 0f, enemy.SpawnTime, 0f, 1f, EasingType.QuadIn);
            Velocity += (Position - enemy.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * enemy.PushStrength * (1f + enemy.TempWeight) * spawnFactor * dt;
        }
        else if (other is PlayerCitizen player)
        {
            if(!player.IsDead)
            {
                Velocity += (Position - player.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * player.PushStrength * (1f + player.TempWeight) * dt;

                if (IsAttacking && _damageTime > (DAMAGE_TIME / TimeScale))
                {
                    float damageDealt = player.Damage(DamageToPlayer);

                    if (damageDealt > 0f)
                    {
                        Game.PlaySfxNearby("zombie.attack.player", Position, pitch: Utils.Map(player.Health, player.MaxHp, 0f, 0.95f, 1.15f, EasingType.QuadIn), volume: 1f, maxDist: 5.5f);
                        OnDamagePlayer(player, damageDealt);
                    }

                    _damageTime = 0f;
                }
            }
        }
    }
}
