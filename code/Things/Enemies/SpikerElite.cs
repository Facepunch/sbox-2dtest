using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public partial class SpikerElite : Enemy
{
    private TimeSince _damageTime;
    private const float DAMAGE_TIME = 0.75f;

    private float _shootDelayTimer;
    private const float SHOOT_DELAY_MIN = 2f;
    private const float SHOOT_DELAY_MAX = 3f;

    public bool IsShooting { get; private set; }
    private float _shotTimer;
    private const float SHOOT_TIME = 4f;
    private bool _hasShot;
    private TimeSince _prepareStartTime;
    private bool _hasReversed;

    public override void Spawn()
    {
        base.Spawn();

        if (Sandbox.Game.IsServer)
        {
            SpriteTexture = SpriteTexture.Atlas("textures/sprites/spiker3_elite.png", 7, 7);

            AnimSpawnPath = "textures/sprites/spiker_spawn.frames";
            AnimIdlePath = "textures/sprites/zombie_walk.frames";
            AnimAttackPath = "textures/sprites/spiker_attack.frames";
            AnimDiePath = "textures/sprites/spiker_die.frames";

            AnimSpeed = 3f;
            BasePivotY = 0.05f;
            HeightZ = 0f;
            //Pivot = new Vector2(0.5f, 0.05f);
            PushStrength = 12f;
            Deceleration = 2.57f;
            DecelerationAttacking = 2.35f;
            AggroRange = 0.45f;

            Radius = 0.34f;
            Health = 220f;
            MaxHealth = Health;
            DamageToPlayer = 24f;

            ScaleFactor = 2.1f;
            Scale = new Vector2(1f, 1f) * ScaleFactor;

            CollideWith.Add(typeof(Enemy));
            CollideWith.Add(typeof(PlayerCitizen));

            ShadowScale = 1.45f;
            _damageTime = DAMAGE_TIME;
            _shootDelayTimer = Sandbox.Game.Random.Float(SHOOT_DELAY_MIN, SHOOT_DELAY_MAX);

            AnimationPath = AnimSpawnPath;

            CoinValueMin = 7;
            CoinValueMax = 15;
        }
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
            //DebugText("SHOOTING... " + _shotTimer.ToString("#.#") + " shot: " + _hasShot + " rev: " + _hasReversed);
            Velocity *= (1f - dt * (IsAttacking ? DecelerationAttacking : Deceleration));
            if(!_hasShot && _prepareStartTime > 1f)
            {
                CreateSpike();
                _hasShot = true;
            }

            if (!_hasReversed && _prepareStartTime > 3f)
            {
                _hasReversed = true;
                AnimationPath = "textures/sprites/spiker_shoot_reverse.frames";
            }

            Velocity *= (1f - dt * 4f);

            _shotTimer -= dt;
            if(_shotTimer < 0f)
            {
                FinishShooting();
                return;
            }
        } 
        else
        {
            Velocity += (closestPlayer.Position - Position).Normal * 1.0f * dt * (IsFeared ? -1f : 1f);
        }

        float speed = 0.5f * (IsAttacking ? 1.3f : 0.7f) + Utils.FastSin(MoveTimeOffset + Time.Now * (IsAttacking ? 12f : 4.5f)) * (IsAttacking ? 0.66f : 0.35f);
        Position += Velocity * dt * speed;

        var player_dist_sqr = (closestPlayer.Position - Position).LengthSquared;
        if (!IsShooting && player_dist_sqr < MathF.Pow(8f, 2f))
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
        CanTurn = false;
        _hasShot = false;
        _hasReversed = false;
        _prepareStartTime = 0f;
        Velocity *= 0.25f;
        AnimationPath = "textures/sprites/spiker_shoot.frames";
        //Game.PlaySfxNearby("spitter.prepare", Position, pitch: Sandbox.Game.Random.Float(0.9f, 1.0f), volume: 1f, maxDist: 4f);
    }

    public void CreateSpike()
    {
        var closestPlayer = Game.GetClosestPlayer(Position);
        if (closestPlayer == null)
            return;

        var target_pos = closestPlayer.Position + closestPlayer.Velocity * Sandbox.Game.Random.Float(0.1f, 4f) + new Vector2(Sandbox.Game.Random.Float(-1f, 1f), Sandbox.Game.Random.Float(-1f, 1f)) * 0.8f;
        var spike = new EnemySpikeElite
        {
            Position = target_pos,
            Shooter = this,
        };

        Game.AddThing(spike);

        Game.PlaySfxNearby("spike.prepare", target_pos, pitch: Sandbox.Game.Random.Float(0.95f, 1.05f), volume: 1.5f, maxDist: 5f);
        //DebugOverlay.Line(Position, target_pos, new Color(0f, 0f, 1f, 0.5f), 2f, false);
    }

    public void FinishShooting()
    {
        _shootDelayTimer = Sandbox.Game.Random.Float(SHOOT_DELAY_MIN, SHOOT_DELAY_MAX);
        IsShooting = false;
        CanAttack = true;
        CanTurn = true;
        AnimationPath = AnimIdlePath;
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
                    float damageDealt = player.Damage(DamageToPlayer);

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
