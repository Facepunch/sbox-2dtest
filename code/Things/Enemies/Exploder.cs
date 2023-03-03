using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public partial class Exploder : Enemy
{
    private TimeSince _damageTime;
    private const float DAMAGE_TIME = 0.75f;

    private const float EXPLOSION_RADIUS = 1.45f;
    private const float EXPLOSION_DAMAGE = 40f;

    public bool IsExploding { get; set; }
    private TimeSince _explodeStartTime;
    private bool _hasExploded;
    private bool _hasStartedLooping;

    private PlayerCitizen _playerWhoKilledUs;

    public override void Spawn()
    {
        base.Spawn();

        if (Sandbox.Game.IsServer)
        {
            SpriteTexture = SpriteTexture.Atlas("textures/sprites/exploder.png", 6, 6);
            //AnimationPath = "textures/sprites/zombie_spawn.frames";
            //AnimIdlePath = "textures/sprites/zombie_walk.frames";
            AnimSpeed = 2f;
            BasePivotY = 0.09f;
            HeightZ = 0f;
            //Pivot = new Vector2(0.5f, 0.05f);
            PushStrength = 12f;
            Deceleration = 1.87f;
            DecelerationAttacking = 1.53f;

            Radius = 0.24f;
            Health = 40f;
            MaxHealth = Health;
            DamageToPlayer = 12f;
            DeathTime = 0.2f;

            ScaleFactor = 1.1f;
            Scale = new Vector2(1f, 1f) * ScaleFactor;

            CollideWith.Add(typeof(Enemy));
            CollideWith.Add(typeof(PlayerCitizen));

            ShadowScale = 1.05f;
            _damageTime = DAMAGE_TIME;

            AnimationPath = AnimSpawnPath;

            CoinValueMin = 1;
            CoinValueMax = 2;
        }
    }

    [Event.Tick.Server]
    public void ServerTick()
    {
        //Utils.DrawCircle(Position, EXPLOSION_RADIUS, 16, Time.Now, Color.Red);
    }

    public override void Update(float dt)
    {
        if (Game.IsGameOver)
            return;

        if(IsExploding)
        {
            if(!_hasStartedLooping && _explodeStartTime > 0.5f)
            {
                AnimationPath = "textures/sprites/exploder_explode_loop.frames";
                _hasStartedLooping = true;
            }

            if (!_hasExploded && _explodeStartTime > 1.5f)
                Explode();
        }

        base.Update(dt);
    }

    protected override void UpdatePosition(float dt)
    {
        base.UpdatePosition(dt);

        var closestPlayer = Game.GetClosestPlayer(Position);
        if (closestPlayer == null)
            return;

        Velocity += (closestPlayer.Position - Position).Normal * 1.0f * dt * (IsFeared ? -1f : 1f);

        if(!IsExploding)
        {
            float speed = (IsAttacking ? 1.3f : 0.7f) + Utils.FastSin(MoveTimeOffset + Time.Now * (IsAttacking ? 15f : 7.5f)) * (IsAttacking ? 0.3f : 0.2f);
            Position += Velocity * dt * speed;
        }
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

    public override void StartDying(PlayerCitizen player)
    {
        if (!IsExploding)
        {
            StartExploding();
            _playerWhoKilledUs = player;
        }
    }

    public void StartExploding()
    {
        IsExploding = true;
        _explodeStartTime = 0f;
        AnimationPath = "textures/sprites/exploder_explode_start.frames";
        CanAttack = false;
        CanTurn = false;
    }

    public void Explode()
    {
        base.StartDying(_playerWhoKilledUs);
        _hasExploded = true;
        IsExploding = false;
    }

    [ClientRpc]
    public override void StartDyingClient()
    {
        base.StartDyingClient();

        Game.SpawnExplosionEffect(Position);
        Game.PlaySfxNearby("explode", Position, pitch: Sandbox.Game.Random.Float(0.9f, 1.1f), volume: 1f, maxDist: 6f);
    }

    public override void FinishDying()
    {
        List<Thing> nearbyThings = new List<Thing>();

        for (int dx = -2; dx <= 2; dx++)
            for (int dy = -2; dy <= 2; dy++)
                Game.AddThingsInGridSquare(new GridSquare(GridPos.x + dx, GridPos.y + dy), nearbyThings);

        foreach (Thing thing in nearbyThings)
        {
            if (thing == this)
                continue;

            if (thing is Enemy enemy && !enemy.IsDying && (!enemy.IsSpawning || enemy.ElapsedTime > 0.75f))
            {
                var dist_sqr = (thing.Position - Position).LengthSquared;
                if (dist_sqr < MathF.Pow(EXPLOSION_RADIUS, 2f))
                    enemy.Damage(EXPLOSION_DAMAGE, null, false);
            }
            else if(thing is PlayerCitizen player && !player.IsDead)
            {
                var dist_sqr = (thing.Position - Position).LengthSquared;
                if (dist_sqr < MathF.Pow(EXPLOSION_RADIUS, 2f) * 0.95f)
                    player.Damage(EXPLOSION_DAMAGE, DamageType.Explosion);
            }
                
        }

        base.FinishDying();
    }
}
