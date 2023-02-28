using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public partial class Charger : Enemy
{
    private TimeSince _damageTime;
    private const float DAMAGE_TIME = 1f;

    private float _chargeDelayTimer;
    private const float CHARGE_DELAY_MIN = 2f;
    private const float CHARGE_DELAY_MAX = 3f;

    public bool IsPreparingToCharge { get; private set; }
    public bool IsCharging { get; private set; }
    private float _prepareTimer;
    private const float PREPARE_TIME = 1f;
    private float _chargeTimer;
    private const float CHARGE_TIME = 3f;

    private Vector2 _chargeDir;
    private Vector2 _chargeVel;
    private TimeSince _chargeCloudTimer;

    public override void Spawn()
    {
        base.Spawn();

        if (Sandbox.Game.IsServer)
        {
            SpriteTexture = SpriteTexture.Atlas("textures/sprites/charger.png", 6, 6);
            AnimSpeed = 2f;
            BasePivotY = 0.08f;
            HeightZ = 0f;
            //Pivot = new Vector2(0.5f, 0.05f);

            Radius = 0.275f;
            Health = 75f;
            MaxHealth = Health;
            DamageToPlayer = 20f;

            PushStrength = 25f;

            ScaleFactor = 1.25f;
            Scale = new Vector2(1f, 1f) * ScaleFactor;

            CollideWith.Add(typeof(Enemy));
            CollideWith.Add(typeof(PlayerCitizen));

            ShadowScale = 1.25f;
            _damageTime = DAMAGE_TIME;
            _chargeDelayTimer = Sandbox.Game.Random.Float(CHARGE_DELAY_MIN, CHARGE_DELAY_MAX);

            AnimationPath = AnimSpawnPath;

            CoinValueMin = 2;
            CoinValueMax = 5;
        }
    }

    [Event.Tick.Server]
    public void ServerTick()
    {
        //DebugText(TempWeight.ToString());
        //DebugText(SinceSpawning.Absolute.ToString("#.##"));
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

        if(IsPreparingToCharge)
        {
            _prepareTimer -= dt;
            if(_prepareTimer < 0f)
            {
                Charge();
                return;
            }
        } 
        else if(IsCharging)
        {
            _chargeTimer -= dt;
            if(_chargeTimer < 0f)
            {
                IsCharging = false;
                //ColorTint = Color.White;
                AnimationPath = AnimIdlePath;
                CanTurn = true;
            }
            else
            {
                _chargeVel += _chargeDir * 4f * Utils.MapReturn(_chargeTimer, CHARGE_TIME, 0f, 0f, 1f, EasingType.Linear) * dt;
                TempWeight += Utils.MapReturn(_chargeTimer, CHARGE_TIME, 0f, 1f, 6f, EasingType.Linear) * dt;
            }
            
            Position += (_chargeVel + Velocity) * dt;

            if(_chargeCloudTimer > 0.25f)
            {
                SpawnCloudClient(Position + new Vector2(0f, 0.25f), -_chargeDir * Sandbox.Game.Random.Float(0.2f, 0.8f));
                _chargeCloudTimer = Sandbox.Game.Random.Float(0f, 0.075f);
            }
        }
        else
        {
            Velocity += (closestPlayer.Position - Position).Normal * 1.0f * dt * (IsFeared ? -1f : 1f);

            float speed = 0.75f * (IsAttacking ? 1.3f : 0.7f) + Utils.FastSin(MoveTimeOffset + Time.Now * (IsAttacking ? 15f : 7.5f)) * (IsAttacking ? 0.66f : 0.35f);
            Position += Velocity * dt * speed;
        }

        var player_dist_sqr = (closestPlayer.Position - Position).LengthSquared;
        if (!IsPreparingToCharge && !IsCharging && !IsAttacking && player_dist_sqr < 8f * 8f)
        {
            _chargeDelayTimer -= dt;
            if(_chargeDelayTimer < 0f)
            {
                PrepareToCharge();
            }
        }
    }

    protected override void HandleDeceleration(float dt)
    {
        if (IsCharging)
        {
            Velocity *= (1f - dt * 1.75f);
            _chargeVel *= (1f - dt * 0.5f);
        }
        else
        {
            base.HandleDeceleration(dt);
        }
    }

    protected override void UpdateSprite(PlayerCitizen targetPlayer)
    {
        if(!IsCharging)
            base.UpdateSprite(targetPlayer);
    }

    protected override void HandleAttacking(PlayerCitizen targetPlayer, float dt)
    {
        if(!IsPreparingToCharge && !IsCharging)
            base.HandleAttacking(targetPlayer, dt);
    }

    public void PrepareToCharge()
    {
        _prepareTimer = PREPARE_TIME;
        IsPreparingToCharge = true;
        Game.PlaySfxNearby("enemy.roar.prepare", Position, pitch: Sandbox.Game.Random.Float(0.95f, 1.05f), volume: 1f, maxDist: 5f);
        AnimationPath = "textures/sprites/charger_charge_start.frames";
        CanTurn = false;
        CanAttack = false;
    }

    public void Charge()
    {
        var closestPlayer = Game.GetClosestPlayer(Position);
        if (closestPlayer == null)
            return;

        var target_pos = closestPlayer.Position + closestPlayer.Velocity * Sandbox.Game.Random.Float(0.5f, 1.75f);
        _chargeDir = Utils.RotateVector((target_pos - Position).Normal, Sandbox.Game.Random.Float(-10f, 10f));

        IsPreparingToCharge = false;
        IsCharging = true;
        _chargeTimer = CHARGE_TIME;
        CanAttack = true;

        _chargeDelayTimer = Sandbox.Game.Random.Float(CHARGE_DELAY_MIN, CHARGE_DELAY_MAX);
        //ColorTint = new Color(1f, 0f, 0f);
        _chargeVel = Vector2.Zero;
        AnimationPath = "textures/sprites/charger_charge_loop.frames";

        AnimSpeed = 3f;
        Scale = new Vector2(1f * target_pos.x < Position.x ? 1f : -1f, 1f) * ScaleFactor;

        Game.PlaySfxNearby("enemy.roar", Position, pitch: Sandbox.Game.Random.Float(0.925f, 1.075f), volume: 1f, maxDist: 8f);
    }

    public override void Colliding(Thing other, float percent, float dt)
    {
        base.Colliding(other, percent, dt);

        if (other is Enemy enemy && !enemy.IsDying)
        {
            var spawnFactor = Utils.Map(enemy.ElapsedTime, 0f, enemy.SpawnTime, 0f, 1f, EasingType.QuadIn);
            Velocity += (Position - enemy.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * enemy.PushStrength * (IsCharging ? 0.5f : 1f) * (1f + enemy.TempWeight) * spawnFactor * dt;
        }
        else if (other is PlayerCitizen player)
        {
            if(!player.IsDead)
            {
                Velocity += (Position - player.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * player.Stats[PlayerStat.PushStrength] * (1f + player.TempWeight) * dt;

                if ((IsAttacking || IsCharging) && _damageTime > (DAMAGE_TIME / TimeScale))
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
