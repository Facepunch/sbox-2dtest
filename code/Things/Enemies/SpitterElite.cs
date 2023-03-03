using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public partial class SpitterElite : Enemy
{
    private TimeSince _damageTime;
    private const float DAMAGE_TIME = 0.75f;

    private float _shootDelayTimer;
    private const float SHOOT_DELAY_MIN = 2f;
    private const float SHOOT_DELAY_MAX = 3f;

    public bool IsShooting { get; private set; }

    private TimeSince _prepareShootTime;
    private int _numVolleysShot;
    private float _currShootDelay;

    public override void Spawn()
    {
        base.Spawn();

        if (Sandbox.Game.IsServer)
        {
            SpriteTexture = SpriteTexture.Atlas("textures/sprites/spitter_elite.png", 7, 6);

            AnimSpawnPath = "textures/sprites/spitter_spawn.frames";
            AnimIdlePath = "textures/sprites/zombie_walk.frames";
            AnimAttackPath = "textures/sprites/spitter_attack.frames";
            AnimDiePath = "textures/sprites/spitter_die.frames";

            AnimSpeed = 2f;
            BasePivotY = 0.1f;
            HeightZ = 0f;
            //Pivot = new Vector2(0.5f, 0.05f);
            PushStrength = 8f;

            Radius = 0.26f;
            Health = 100f;
            MaxHealth = Health;
            DamageToPlayer = 10f;

            ScaleFactor = 1.05f;
            Scale = new Vector2(1f, 1f) * ScaleFactor;

            CollideWith.Add(typeof(Enemy));
            CollideWith.Add(typeof(PlayerCitizen));

            ShadowScale = 1.125f;
            _damageTime = DAMAGE_TIME;
            _shootDelayTimer = Sandbox.Game.Random.Float(SHOOT_DELAY_MIN, SHOOT_DELAY_MAX);

            AnimationPath = AnimSpawnPath;

            CoinValueMin = 1;
            CoinValueMax = 2;
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
            if(_numVolleysShot < 3 && _prepareShootTime > (_numVolleysShot == 0 ? 1.0f : _currShootDelay))
                Shoot();
            else if(_numVolleysShot >= 3 && _prepareShootTime > 0.6f)
                FinishShooting();

            return;
        } 
        else
        {
            Velocity += (closestPlayer.Position - Position).Normal * 1.0f * dt * (IsFeared ? -1f : 1f);
        }

        float speed = 0.6f * (IsAttacking ? 1.3f : 0.7f) + Utils.FastSin(MoveTimeOffset + Time.Now * (IsAttacking ? 15f : 7.5f)) * (IsAttacking ? 0.5f : 0.2f);
        Position += Velocity * dt * speed;

        var player_dist_sqr = (closestPlayer.Position - Position).LengthSquared;
        if (!IsShooting && player_dist_sqr < 10f * 10f)
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
        AnimationPath = "textures/sprites/spitter_shoot.frames";
        Game.PlaySfxNearby("spitter.prepare", Position, pitch: Sandbox.Game.Random.Float(1f, 1.1f), volume: 0.6f, maxDist: 2.75f);
        CanAttack = false;
        _numVolleysShot = 0;
        _currShootDelay = Sandbox.Game.Random.Float(0.1f, 0.5f);
    }

    public void Shoot()
    {
        var closestPlayer = Game.GetClosestPlayer(Position);
        if (closestPlayer == null)
            return;

        var target_pos = closestPlayer.Position + closestPlayer.Velocity * Sandbox.Game.Random.Float(0.5f, 1.5f);
        var dir = Utils.RotateVector((target_pos - Position).Normal, Sandbox.Game.Random.Float(-10f, 10f));
        var bullet = new EnemyBullet
        {
            Position = Position + dir * 0.05f,
            Depth = 1f,
            Direction = dir,
            Shooter = this,
        };

        bullet.ColorTint = new Color(1f, 0.2f, 0f);
        bullet.Speed = 2.1f;

        if (dir.x < 0f)
            bullet.Scale = new Vector2(-bullet.Scale.x, bullet.Scale.y);

        Game.AddThing(bullet);

        Velocity *= 0.25f;
        _numVolleysShot++;
        _prepareShootTime = 0f;
        _currShootDelay = Sandbox.Game.Random.Float(0.1f, 0.5f);

        Game.PlaySfxNearby("spitter.shoot", Position, pitch: Sandbox.Game.Random.Float(1.0f, 1.1f), volume: 0.9f, maxDist: 5f);

        if(_numVolleysShot >= 3)
            AnimationPath = "textures/sprites/spitter_shoot_reverse.frames";
    }

    public void FinishShooting()
    {
        AnimationPath = AnimIdlePath;
        CanAttack = true;
        _shootDelayTimer = Sandbox.Game.Random.Float(SHOOT_DELAY_MIN, SHOOT_DELAY_MAX);
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
                Velocity += (Position - player.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * player.Stats[PlayerStat.PushStrength] * (1f + player.TempWeight) * dt;

                if (IsAttacking && _damageTime > (DAMAGE_TIME / TimeScale))
                {
                    float damageDealt = player.Damage(DamageToPlayer, DamageType.Melee);

                    if (damageDealt > 0f)
                    {
                        Game.PlaySfxNearby("zombie.attack.player", Position, pitch: Utils.Map(player.Health, player.Stats[PlayerStat.MaxHp], 0f, 0.95f, 1.15f, EasingType.QuadIn), volume: 1f, maxDist: 5.5f);
                        OnDamagePlayer(player, damageDealt);
                    }

                    _damageTime = 0f;
                }
            }
        }
    }
}
