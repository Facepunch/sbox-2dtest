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
    private const float DAMAGE_TIME = 0.5f;

    private float _shootDelayTimer;
    private const float SHOOT_DELAY_MIN = 1.25f;
    private const float SHOOT_DELAY_MAX = 3f;

    public bool IsShooting { get; private set; }
    private bool _hasShot;

    private TimeSince _prepareShootTime;

    public EnemyNametag Nametag { get; private set; }

    public override void Spawn()
    {
        base.Spawn();

        if (Host.IsServer)
        {
            SpriteTexture = SpriteTexture.Atlas("textures/sprites/boss.png", 7, 7);

            AnimSpawnPath = "textures/sprites/boss_spawn.frames";
            AnimIdlePath = "textures/sprites/zombie_walk.frames";
            AnimAttackPath = "textures/sprites/boss_attack.frames";
            AnimDiePath = "textures/sprites/boss_die.frames";

            AnimSpeed = 2f;
            BasePivotY = 0.05f;
            HeightZ = 0f;
            //Pivot = new Vector2(0.5f, 0.05f);
            PushStrength = 50f;
            DeathTime = 3f;

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
            _shootDelayTimer = Rand.Float(SHOOT_DELAY_MIN, SHOOT_DELAY_MAX);

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

        float speed = 0.9f * (IsAttacking ? 1.3f : 0.7f) + Utils.FastSin(MoveTimeOffset + Time.Now * (IsAttacking ? 15f : 7.5f)) * (IsAttacking ? 1.05f : 0.66f);
        Position += Velocity * dt * speed;

        var player_dist_sqr = (closestPlayer.Position - Position).LengthSquared;
        if (!IsShooting && !IsAttacking && player_dist_sqr < MathF.Pow(9f, 2f))
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
        AnimationPath = "textures/sprites/boss_shoot.frames";
        Game.PlaySfxNearby("spitter.prepare", Position, pitch: Rand.Float(1f, 1.1f), volume: 0.6f, maxDist: 2.75f);
        CanAttack = false;
    }

    public void Shoot()
    {
        var closestPlayer = Game.GetClosestPlayer(Position);
        if (closestPlayer == null)
            return;

        var num_bullets = MathX.FloorToInt(Utils.Map(Health, MaxHealth, 0f, 3f, 8f, EasingType.SineIn)) + Rand.Int(0, 1);
        var spread = Rand.Float(30f, 60f);

        float currAngleOffset = -spread * 0.5f;
        float increment = spread / (float)(num_bullets - 1);

        var target_pos = closestPlayer.Position + closestPlayer.Velocity * 0.5f;
        Vector2 aim_dir = Utils.RotateVector((target_pos - Position).Normal, Rand.Float(-15f, 15f));

        for (int i = 0; i < num_bullets; i++)
        {
            var dir = Utils.RotateVector(aim_dir, currAngleOffset + increment * i);

            var bullet = new EnemyBullet
            {
                Position = Position + dir * 0.05f,
                Depth = 1f,
                Direction = dir,
                Shooter = this,
            };

            if (dir.x < 0f)
                bullet.Scale = new Vector2(-bullet.Scale.x, bullet.Scale.y);

            Game.AddThing(bullet);
        }

        Velocity *= 0.25f;
        _hasShot = true;

        AnimationPath = "textures/sprites/boss_shoot_reverse.frames";
        Game.PlaySfxNearby("spitter.shoot", Position, pitch: Rand.Float(0.8f, 0.9f), volume: 0.9f, maxDist: 5f);
    }

    public void FinishShooting()
    {
        AnimationPath = AnimIdlePath;
        CanAttack = true;
        _shootDelayTimer = Rand.Float(SHOOT_DELAY_MIN, SHOOT_DELAY_MAX) * Utils.Map(Health, MaxHealth, 0f, 1f, 0.5f, EasingType.QuadIn);
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

    [ClientRpc]
    public override void StartDyingClient()
    {
        base.StartDyingClient();
        Nametag.SetVisible(false);
    }
}
