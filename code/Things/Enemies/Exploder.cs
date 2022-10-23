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

    public override void Spawn()
    {
        base.Spawn();

        if (Host.IsServer)
        {
            SpriteTexture = SpriteTexture.Atlas("textures/sprites/exploder.png", 5, 6);
            //AnimationPath = "textures/sprites/zombie_spawn.frames";
            //AnimIdlePath = "textures/sprites/zombie_walk.frames";
            AnimationSpeed = 2f;
            BasePivotY = 0.05f;
            HeightZ = 0f;
            //Pivot = new Vector2(0.5f, 0.05f);

            Radius = 0.215f;
            Health = 20f;
            MaxHealth = Health;
            DamageToPlayer = 12f;
            DeathTime = 0.2f;

            ScaleFactor = 0.85f;
            Scale = new Vector2(1f, 1f) * ScaleFactor;

            CollideWith.Add(typeof(Enemy));
            CollideWith.Add(typeof(PlayerCitizen));

            ShadowScale = 0.975f;
            _damageTime = DAMAGE_TIME;

            AnimationPath = AnimSpawnPath;
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

        base.Update(dt);
    }

    protected override void UpdatePosition(float dt)
    {
        base.UpdatePosition(dt);

        var closestPlayer = Game.GetClosestPlayer(Position);
        if (closestPlayer == null)
            return;

        Velocity += (closestPlayer.Position - Position).Normal * 1.0f * dt;
        float speed = (IsAttacking ? 1.3f : 0.7f) + Utils.FastSin(MoveTimeOffset + Time.Now * (IsAttacking ? 15f : 7.5f)) * (IsAttacking ? 0.66f : 0.35f);
        Position += Velocity * dt * speed;
    }

    public override void Colliding(Thing other, float percent, float dt)
    {
        base.Colliding(other, percent, dt);

        if (other is Enemy enemy && !enemy.IsDying)
        {
            var spawnFactor = Utils.Map(enemy.ElapsedTime, 0f, enemy.SpawnTime, 0f, 1f, EasingType.QuadIn);
            Velocity += (Position - enemy.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * 10f * (1f + enemy.TempWeight) * spawnFactor * dt;
        }
        else if (other is PlayerCitizen player)
        {
            if(!player.IsDead)
            {
                Velocity += (Position - player.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * 5f * (1f + player.TempWeight) * dt;

                if (IsAttacking && _damageTime >= DAMAGE_TIME)
                {
                    float damageDealt = player.Damage(DamageToPlayer);

                    if (damageDealt > 0f && player.ThornsPercent > 0f)
                    {
                        Damage(damageDealt * player.ThornsPercent * player.GetDamageMultiplier(), player, false);
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

        Game.SpawnExplosionEffect(Position);
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
                    enemy.Damage(30f, null, false);
            }
            else if(thing is PlayerCitizen player)
            {
                var dist_sqr = (thing.Position - Position).LengthSquared;
                if (dist_sqr < MathF.Pow(EXPLOSION_RADIUS, 2f) * 0.95f)
                    player.Damage(30f);
            }
                
        }

        base.FinishDying();
    }
}
