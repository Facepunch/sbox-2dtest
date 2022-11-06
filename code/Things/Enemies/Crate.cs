using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;

public partial class Crate : Enemy
{
    public override void Spawn()
    {
        base.Spawn();

        if (Host.IsServer)
        {
            SpriteTexture = SpriteTexture.Atlas("textures/sprites/crate.png", 4, 5);

            AnimSpawnPath = "textures/sprites/crate_spawn.frames";
            AnimIdlePath = "textures/sprites/crate_walk.frames";
            AnimDiePath = "textures/sprites/crate_die.frames";

            BasePivotY = 0.05f;
            HeightZ = 0f;
            PushStrength = 5f;

            Deceleration = 10f;

            SpawnTime = 2f;

            Radius = 0.25f;
            Health = 50f;
            MaxHealth = Health;

            ScaleFactor = 0.95f;
            Scale = new Vector2(Rand.Float(0f, 1f) < 0.5f ? -1f : 1f, 1f) * ScaleFactor;

            CollideWith.Add(typeof(Enemy));
            CollideWith.Add(typeof(PlayerCitizen));

            ShadowScale = 1.3f;

            AnimationPath = AnimSpawnPath;

            CanAttack = false;

            CoinValueMin = 1;
            CoinValueMax = 4;
        }
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();

        CanBleedClient = false;
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

        Position += Velocity * dt;
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
            }
        }
    }
}
