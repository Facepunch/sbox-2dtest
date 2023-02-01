using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public partial class Runner : Enemy
{
    private TimeSince _damageTime;
    private const float DAMAGE_TIME = 0.5f;

    public bool HasTarget { get; private set; }
    private Vector2 _wanderPos;

    public override void Spawn()
    {
        base.Spawn();

        if (Sandbox.Game.IsServer)
        {
            SpriteTexture = SpriteTexture.Atlas("textures/sprites/runner.png", 5, 6);
            AnimSpeed = 2f;
            BasePivotY = 0.07f;
            HeightZ = 0f;
            //Pivot = new Vector2(0.5f, 0.05f);
            PushStrength = 10f;
            Deceleration = 0.47f;
            DecelerationAttacking = 0.33f;
            AggroRange = 2.5f;

            Radius = 0.25f;
            Health = 70f;
            MaxHealth = Health;
            DamageToPlayer = 10f;

            ScaleFactor = 1.0f;
            Scale = new Vector2(1f, 1f) * ScaleFactor;

            CollideWith.Add(typeof(Enemy));
            CollideWith.Add(typeof(PlayerCitizen));

            ShadowScale = 1.1f;
            _damageTime = DAMAGE_TIME;

            HasTarget = false;

            _wanderPos = new Vector2(Sandbox.Game.Random.Float(Game.BOUNDS_MIN.x + 1f, Game.BOUNDS_MAX.x - 1f), Sandbox.Game.Random.Float(Game.BOUNDS_MIN.y + 1f, Game.BOUNDS_MAX.y - 1f));

            AnimationPath = AnimSpawnPath;

            CoinValueMin = 1;
            CoinValueMax = 4;
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

        if (HasTarget)
        {
            Velocity += (closestPlayer.Position - Position).Normal * dt;
        }
        else
        {
            var wander_dist_sqr = (_wanderPos - Position).LengthSquared;
            if(wander_dist_sqr < 0.25f)
            {
                _wanderPos = new Vector2(MathX.Clamp(closestPlayer.Position.x + Sandbox.Game.Random.Float(-3f, 3f), Game.BOUNDS_MIN.x + 1f, Game.BOUNDS_MAX.x - 1f), MathX.Clamp(closestPlayer.Position.y + Sandbox.Game.Random.Float(-3f, 3f), Game.BOUNDS_MIN.y + 1f, Game.BOUNDS_MAX.y - 1f));
            }

            Velocity += (_wanderPos - Position).Normal * dt;

            var player_dist_sqr = (closestPlayer.Position - Position).LengthSquared;
            if(player_dist_sqr < 3.5f * 3.5f)
            {
                HasTarget = true;
                Game.PlaySfxNearby("runner.howl", Position, pitch: Sandbox.Game.Random.Float(0.9f, 1.1f), volume: 1f, maxDist: 6f);
            }
        }

        //if(!HasTarget)
        //    DebugOverlay.Line(Position, _wanderPos, 0f, false);

        float speed = (IsAttacking ? 1.3f : 0.7f) + Utils.FastSin(MoveTimeOffset + Time.Now * (IsAttacking ? 15f : 7.5f)) * (IsAttacking ? 0.66f : 0.35f);
        Position += Velocity * dt * speed;
    }

    public override void StartAttacking()
    {
        base.StartAttacking();

        Game.PlaySfxNearby("runner.bark", Position, pitch: Sandbox.Game.Random.Float(0.9f, 1.1f), volume: 1f, maxDist: 4f);
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
                        Game.PlaySfxNearby("runner.bite", Position, pitch: Utils.Map(player.Health, player.MaxHp, 0f, 0.9f, 0.95f, EasingType.QuadIn), volume: 1f, maxDist: 5.5f);
                        OnDamagePlayer(player, damageDealt);
                        //player.Velocity += (Position - player.Position).Normal * 1.5f;
                    }

                    _damageTime = 0f;
                }
            }
        }
    }
}
