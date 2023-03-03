using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public partial class ZombieElite : Enemy
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
            SpriteTexture = SpriteTexture.Atlas("textures/sprites/zombie_elite.png", 5, 6);
            AnimSpeed = 2f;
            BasePivotY = 0.05f;
            HeightZ = 0f;
            //Pivot = new Vector2(0.5f, 0.05f);
            PushStrength = 12f;

            Radius = 0.27f;
            Health = 55f;
            MaxHealth = Health;
            DamageToPlayer = 10f;

            ScaleFactor = 0.95f;
            Scale = new Vector2(1f, 1f) * ScaleFactor;

            CollideWith.Add(typeof(Enemy));
            CollideWith.Add(typeof(PlayerCitizen));

            ShadowScale = 1f;
            _damageTime = DAMAGE_TIME;

            HasTarget = false;

            _wanderPos = new Vector2(Sandbox.Game.Random.Float(Game.BOUNDS_MIN.x + 1f, Game.BOUNDS_MAX.x - 1f), Sandbox.Game.Random.Float(Game.BOUNDS_MIN.y + 1f, Game.BOUNDS_MAX.y - 1f));

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

        if (HasTarget)
        {
            Velocity += (closestPlayer.Position - Position).Normal * dt * (IsFeared ? -1f : 1f);
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
                Game.PlaySfxNearby("zombie.spawn0", Position, pitch: Sandbox.Game.Random.Float(0.9f, 1.1f), volume: 1f, maxDist: 4f);
            }
        }

        //if(!HasTarget)
        //    DebugOverlay.Line(Position, _wanderPos, 0f, false);

        float speed = (IsAttacking ? 1.9f : 1f) + Utils.FastSin(MoveTimeOffset + Time.Now * (IsAttacking ? 16f : 9.5f)) * (IsAttacking ? 1.9f : 1f);
        Position += Velocity * dt * speed;
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
                        //player.Velocity += (Position - player.Position).Normal * 1.5f;
                    }

                    _damageTime = 0f;
                }
            }
        }
    }
}
