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
    public EnemyNametag Nametag { get; private set; }

    private TimeSince _damageTime;
    private const float DAMAGE_TIME = 0.5f;

    private float _shootDelayTimer;
    private const float SHOOT_DELAY_MIN = 1.25f;
    private const float SHOOT_DELAY_MAX = 3f;

    public bool IsShooting { get; private set; }
    private bool _hasShot;

    private TimeSince _prepareShootTime;

    public bool IsCharging { get; private set; }
    private float _chargeTimer;
    private const float CHARGE_TIME_MIN = 1f;
    private const float CHARGE_TIME_MAX = 2.4f;
    private float _chargeTime;
    private Vector2 _chargeDir;
    private Vector2 _chargeVel;
    private TimeSince _chargeCloudTimer;

    public bool IsPreparingToCharge { get; private set; }
    private float _prepareTimer;
    private const float PREPARE_TIME = 0.8f;
    private bool _hasLandedCharge;

    private float _chargeDelayTimer;
    private const float CHARGE_DELAY_MIN = 3f;
    private const float CHARGE_DELAY_MAX = 6f;

    public override void Spawn()
    {
        base.Spawn();

        if (Sandbox.Game.IsServer)
        {
            SpriteTexture = SpriteTexture.Atlas("textures/sprites/boss.png", 7, 7);

            AnimSpawnPath = "textures/sprites/boss_spawn.frames";
            AnimIdlePath = "textures/sprites/zombie_walk.frames";
            AnimAttackPath = "textures/sprites/boss_attack.frames";
            AnimDiePath = "textures/sprites/boss_die.frames";

            AnimSpeed = 3f;
            BasePivotY = 0.05f;
            HeightZ = 0f;
            //Pivot = new Vector2(0.5f, 0.05f);
            PushStrength = 50f;
            DeathTime = 3f;

            Deceleration = 1.1f;
            DecelerationAttacking = 1.1f;

            Radius = 0.42f;
            Health = 2000f;
            MaxHealth = Health;
            DamageToPlayer = 20f;

            ScaleFactor = 1.85f;
            Scale = new Vector2(1f, 1f) * ScaleFactor;

            CollideWith.Add(typeof(Enemy));
            CollideWith.Add(typeof(PlayerCitizen));

            ShadowScale = 2.0f;
            _damageTime = DAMAGE_TIME;
            _shootDelayTimer = Sandbox.Game.Random.Float(SHOOT_DELAY_MIN, SHOOT_DELAY_MAX);

            AnimationPath = AnimSpawnPath;
        }
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();

        Nametag = Game.Hud.SpawnEnemyNametag(this);
    }

    public override void Update(float dt)
    {
        if (Game.IsGameOver)
            return;

        //Utils.DrawCircle(Position, Radius, 8, Time.Now, Color.Red);

        base.Update(dt);
    }

    protected override void UpdatePosition(float dt)
    {
        base.UpdatePosition(dt);

        var closestPlayer = Game.GetClosestPlayer(Position);
        if (closestPlayer == null)
            return;

        if (IsPreparingToCharge)
        {
            _prepareTimer -= dt;
            if (_prepareTimer < 0f)
            {
                Charge();
                return;
            }
        }
        else if (IsCharging)
        {
            _chargeTimer -= dt;
            
            if(!_hasLandedCharge && _chargeTimer < 0.5f)
            {
                AnimationPath = "textures/sprites/boss_charge_reverse.frames";
                _hasLandedCharge = true;
            }

            if (_chargeTimer < 0f)
            {
                IsCharging = false;
                //ColorTint = Color.White;
                AnimationPath = AnimIdlePath;
                CanTurn = true;
                CanAttackAnim = true;
            }
            else
            {
                _chargeVel += _chargeDir * 4f * Utils.MapReturn(_chargeTimer, _chargeTime, 0f, 0f, 1f, EasingType.Linear) * dt;
                TempWeight += Utils.MapReturn(_chargeTimer, _chargeTime, 0f, 1f, 6f, EasingType.Linear) * dt;
            }

            if (_chargeTimer < 0.1f)
                _chargeVel *= 0f;

            Position += (_chargeVel + Velocity) * dt;

            if (_chargeCloudTimer > 0.25f)
            {
                SpawnCloudClient(Position + new Vector2(0f, 0.25f), -_chargeDir * Sandbox.Game.Random.Float(0.2f, 0.8f));
                _chargeCloudTimer = Sandbox.Game.Random.Float(0f, 0.075f);

                if(Health < MaxHealth / 2f)
                {
                    var dir = (new Vector2(Sandbox.Game.Random.Float(-1f, 1f), Sandbox.Game.Random.Float(-1f, 1f))).Normal;
                    SpawnBullet(dir);
                }
            }
        }
        else
        {
            if (IsShooting)
            {
                if (!_hasShot && _prepareShootTime > 1.0f)
                    Shoot();

                if (_prepareShootTime > 1.6f)
                    FinishShooting();

                return;
            }
            else
            {
                Velocity += (closestPlayer.Position - Position).Normal * 1.0f * dt;
            }

            float speed = 1.66f * (IsAttacking ? 1.3f : 0.7f) + Utils.FastSin(MoveTimeOffset + Time.Now * (IsAttacking ? 15f : 7.5f));
            Position += Velocity * dt * speed;

            var player_dist_sqr = (closestPlayer.Position - Position).LengthSquared;

            if (!IsPreparingToCharge && !IsCharging && !IsShooting && !IsAttacking && player_dist_sqr < MathF.Pow(9f, 2f))
            {
                _shootDelayTimer -= dt;
                if (_shootDelayTimer < 0f)
                {
                    PrepareToShoot();
                }
            }

            if (!IsPreparingToCharge && !IsCharging && !IsShooting && !IsAttacking && player_dist_sqr < 8f * 8f)
            {
                _chargeDelayTimer -= dt;
                if (_chargeDelayTimer < 0f)
                {
                    PrepareToCharge();
                }
            }
        }
    }

    public void PrepareToShoot()
    {
        _prepareShootTime = 0f;
        IsShooting = true;
        _hasShot = false;
        AnimationPath = "textures/sprites/boss_shoot.frames";
        Game.PlaySfxNearby("boss.prepare", Position, pitch: Sandbox.Game.Random.Float(0.75f, 0.85f), volume: 1.7f, maxDist: 10f);
        CanAttack = false;
    }

    public void Shoot()
    {
        var closestPlayer = Game.GetClosestPlayer(Position);
        if (closestPlayer == null)
            return;

        var num_bullets = MathX.FloorToInt(Utils.Map(Health, MaxHealth, 0f, 3f, 8f, EasingType.SineIn)) + Sandbox.Game.Random.Int(0, 1);
        var spread = Sandbox.Game.Random.Float(30f, 60f);

        float currAngleOffset = -spread * 0.5f;
        float increment = spread / (float)(num_bullets - 1);

        var target_pos = closestPlayer.Position + closestPlayer.Velocity * 0.5f;
        Vector2 aim_dir = Utils.RotateVector((target_pos - Position).Normal, Sandbox.Game.Random.Float(-15f, 15f));

        for (int i = 0; i < num_bullets; i++)
        {
            var dir = Utils.RotateVector(aim_dir, currAngleOffset + increment * i);
            SpawnBullet(dir);
        }

        Velocity *= 0.25f;
        _hasShot = true;

        AnimationPath = "textures/sprites/boss_shoot_reverse.frames";
        Game.PlaySfxNearby("boss.shoot", Position, pitch: Sandbox.Game.Random.Float(0.65f, 0.75f), volume: 1.5f, maxDist: 9f);
    }

    void SpawnBullet(Vector2 dir)
    {
        var bullet = new EnemyBullet
        {
            Position = Position + dir * 0.05f,
            Depth = 1f,
            Direction = dir,
            Shooter = this,
        };

        bullet.ColorTint = Color.Yellow;
        bullet.Speed = 1.9f;

        if (dir.x < 0f)
            bullet.Scale = new Vector2(-bullet.Scale.x, bullet.Scale.y);

        Game.AddThing(bullet);
    }

    public void FinishShooting()
    {
        AnimationPath = AnimIdlePath;
        CanAttack = true;
        _shootDelayTimer = Sandbox.Game.Random.Float(SHOOT_DELAY_MIN, SHOOT_DELAY_MAX) * Utils.Map(Health, MaxHealth, 0f, 1f, 0.5f, EasingType.QuadIn);
        IsShooting = false;
    }

    public void PrepareToCharge()
    {
        _prepareTimer = PREPARE_TIME;
        IsPreparingToCharge = true;
        Game.PlaySfxNearby("boss.prepare", Position, pitch: Sandbox.Game.Random.Float(1.05f, 1.1f), volume: 1.75f, maxDist: 10f);
        AnimationPath = "textures/sprites/boss_charge.frames";
        CanTurn = false;
        CanAttack = false;
        CanAttackAnim = false;
    }

    public void Charge()
    {
        var closestPlayer = Game.GetClosestPlayer(Position);
        if (closestPlayer == null)
            return;

        var target_pos = closestPlayer.Position + closestPlayer.Velocity * Sandbox.Game.Random.Float(0f, 1.66f);
        _chargeDir = Utils.RotateVector((target_pos - Position).Normal, Sandbox.Game.Random.Float(-10f, 10f));

        IsPreparingToCharge = false;
        IsCharging = true;
        _chargeTime = Sandbox.Game.Random.Float(CHARGE_TIME_MIN, CHARGE_TIME_MAX);
        _chargeTimer = _chargeTime;
        CanAttack = true;
        _hasLandedCharge = false;

        _chargeDelayTimer = Sandbox.Game.Random.Float(CHARGE_DELAY_MIN, CHARGE_DELAY_MAX) * Utils.Map(Health, MaxHealth, 0f, 1f, 0.5f, EasingType.SineIn);
        //ColorTint = new Color(1f, 0f, 0f);
        _chargeVel = Vector2.Zero;

        Scale = new Vector2(1f * target_pos.x < Position.x ? 1f : -1f, 1f) * ScaleFactor;

        Game.PlaySfxNearby("boss.charge", Position, pitch: Sandbox.Game.Random.Float(0.9f, 1.05f), volume: 1.6f, maxDist: 9f);
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

    public override void StartDying(PlayerCitizen player)
    {
        base.StartDying(player);

        Game.PlaySfxNearby("boss.die", Position, pitch: Sandbox.Game.Random.Float(0.75f, 0.8f), volume: 1.5f, maxDist: 15f);
        Game.Victory();
    }

    [ClientRpc]
    public override void StartDyingClient()
    {
        base.StartDyingClient();
        Nametag.SetVisible(false);
    }
}
