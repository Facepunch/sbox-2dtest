using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public partial class Spiker : Enemy
{
    private TimeSince _damageTime;
    private const float DAMAGE_TIME = 0.75f;

    private float _shootDelayTimer;
    private const float SHOOT_DELAY_MIN = 2f;
    private const float SHOOT_DELAY_MAX = 3f;

    public bool IsShooting { get; private set; }
    private float _shotTimer;
    private const float SHOOT_TIME = 2f;
    private bool _hasShot;
    private TimeSince _prepareStartTime;

    public override void Spawn()
    {
        base.Spawn();

        if (Host.IsServer)
        {
            SpriteTexture = SpriteTexture.Atlas("textures/sprites/spiker.png", 5, 6);
            //AnimationPath = "textures/sprites/zombie_spawn.frames";
            //AnimIdlePath = "textures/sprites/zombie_walk.frames";
            AnimSpeed = 4f;
            BasePivotY = 0.05f;
            HeightZ = 0f;
            //Pivot = new Vector2(0.5f, 0.05f);
            PushStrength = 8f;
            Deceleration = 2.57f;
            DecelerationAttacking = 2.35f;
            AggroRange = 0.75f;

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
            DebugText("SHOOTING...");

            if(!_hasShot && _prepareStartTime > 0.75f)
            {
                CreateSpike();
                _hasShot = true;
            }

            _shotTimer -= dt;
            if(_shotTimer < 0f)
            {
                FinishShooting();
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
        if (!IsShooting && !IsAttacking && player_dist_sqr < MathF.Pow(6f, 2f))
        {
            _shootDelayTimer -= dt;
            if(_shootDelayTimer < 0f)
            {
                StartShooting();
            }
        }
    }

    public void StartShooting()
    {
        _shotTimer = SHOOT_TIME;
        IsShooting = true;
        CanAttack = false;
        _hasShot = false;
        _prepareStartTime = 0f;
        Velocity *= 0.25f;
        Game.PlaySfxNearby("spitter.prepare", Position, pitch: Rand.Float(0.9f, 1.0f), volume: 1f, maxDist: 4f);
    }

    public void CreateSpike()
    {
        var closestPlayer = Game.GetClosestPlayer(Position);
        if (closestPlayer == null)
            return;

        var target_pos = closestPlayer.Position + closestPlayer.Velocity * 0.1f + new Vector2(Rand.Float(-1f, 1f), Rand.Float(-1f, 1f)) * 0.8f;
        var spike = new EnemySpike
        {
            Position = target_pos,
            Shooter = this,
        };

        Game.AddThing(spike);

        //Game.PlaySfxNearby("spitter.shoot", Position, pitch: Rand.Float(0.8f, 0.9f), volume: 1f, maxDist: 5f);
    }

    public void FinishShooting()
    {
        _shootDelayTimer = Rand.Float(SHOOT_DELAY_MIN, SHOOT_DELAY_MAX);
        IsShooting = false;
        CanAttack = true;
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
