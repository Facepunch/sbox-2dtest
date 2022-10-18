using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public partial class Zombie : Enemy
{
    private TimeSince _damageTime;
    private const float DAMAGE_TIME = 0.25f;

    public override void Spawn()
    {
        base.Spawn();

        if (Host.IsServer)
        {
            SpriteTexture = SpriteTexture.Atlas("textures/sprites/zombie_spritesheet3.png", 5, 6);
            AnimationPath = "textures/sprites/zombie_spawn.frames";
            AnimIdlePath = "textures/sprites/zombie_walk.frames";
            AnimationSpeed = 2f;
            Pivot = new Vector2(0.5f, 0.05f);

            Radius = 0.25f;
            Health = 30f;
            MaxHealth = Health;
            DamageToPlayer = 5f;

            Scale = new Vector2(1f, 1f) * SCALE_FACTOR;

            CollideWith.Add(typeof(Enemy));
            CollideWith.Add(typeof(PlayerCitizen));

            ShadowScale = 0.95f;
            _damageTime = DAMAGE_TIME;
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

        Velocity += (closestPlayer.Position - Position).Normal * 1.0f * dt;
        float speed = (IsAttacking ? 1.3f : 0.7f) + Utils.FastSin(MoveTimeOffset + Time.Now * (IsAttacking ? 15f : 7.5f)) * (IsAttacking ? 0.66f : 0.35f);
        Position += Velocity * dt * speed;
    }

    public override void Colliding(Thing other, float percent, float dt)
    {
        base.Colliding(other, percent, dt);

        if (other is Enemy enemy && !enemy.IsDying)
        {
            Velocity += (Position - enemy.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * 10f * (1f + enemy.TempWeight) * dt;
        }
        else if (other is PlayerCitizen player)
        {
            Velocity += (Position - player.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * 5f * (1f + player.TempWeight) * dt;

            if (IsAttacking && _damageTime >= DAMAGE_TIME)
            {
                player.Damage(DamageToPlayer);
                //player.Velocity *= (1f - 13.5f * dt);
                _damageTime = 0f;
            }
        }
    }
}
