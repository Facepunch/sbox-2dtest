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

        if (Sandbox.Game.IsServer)
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
            Health = 45f;
            MaxHealth = Health;

            ScaleFactor = 0.95f;
            Scale = new Vector2(Sandbox.Game.Random.Float(0f, 1f) < 0.5f ? -1f : 1f, 1f) * ScaleFactor;

            CollideWith.Add(typeof(Enemy));
            CollideWith.Add(typeof(PlayerCitizen));

            ShadowScale = 1.3f;

            AnimationPath = AnimSpawnPath;

            CanAttack = false;

            CoinValueMin = 1;
            CoinValueMax = 2;
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
                Velocity += (Position - player.Position).Normal * Utils.Map(percent, 0f, 1f, 0f, 1f) * player.Stats[PlayerStat.PushStrength] * (1f + player.TempWeight) * dt;
            }
        }
    }

    public override void DropLoot(PlayerCitizen player)
    {
        float RAND_POS = 0.2f;

        int num_coins = Sandbox.Game.Random.Int(2, 3);
        for(int i = 0; i < num_coins; i++)
        {
            var coin = Game.SpawnCoin(Position + new Vector2(Sandbox.Game.Random.Float(-RAND_POS, RAND_POS), Sandbox.Game.Random.Float(-RAND_POS, RAND_POS)), Sandbox.Game.Random.Int(CoinValueMin, CoinValueMax));

            if (coin != null)
                coin.Velocity = (coin.Position - Position) * Sandbox.Game.Random.Float(2f, 6f);
        }

        var health_pack_chance = player != null ? Utils.Map(player.Health, player.Stats[PlayerStat.MaxHp], 0f, 0.2f, 0.75f) : 0.1f;
        if (Sandbox.Game.Random.Float(0f, 1f) < health_pack_chance)
        {
            var healthPack = new HealthPack() { Position = Position + new Vector2(Sandbox.Game.Random.Float(-RAND_POS, RAND_POS), Sandbox.Game.Random.Float(-RAND_POS, RAND_POS)) };
            healthPack.Velocity = (healthPack.Position - Position) * Sandbox.Game.Random.Float(2f, 6f);
            Game.AddThing(healthPack);
        }

        if(Game.TimeSinceMagnet > 50f)
        {
            var magnet_chance = 0.09f * Utils.Map(Game.TimeSinceMagnet, 50f, 480f, 1f, 5.5f, EasingType.Linear);
            if (Sandbox.Game.Random.Float(0f, 1f) < magnet_chance)
            {
                var magnet = new Magnet() { Position = Position + new Vector2(Sandbox.Game.Random.Float(-RAND_POS, RAND_POS), Sandbox.Game.Random.Float(-RAND_POS, RAND_POS)) };
                magnet.Velocity = (magnet.Position - Position) * Sandbox.Game.Random.Float(2f, 6f);
                Game.AddThing(magnet);
                Game.TimeSinceMagnet = 0f;
            }
        }

        var revive_chance = Game.DeadPlayers.ToList().Count * 0.33f;
        if (Sandbox.Game.Random.Float(0f, 1f) < revive_chance)
        {
            var reviveSoul = new ReviveSoul() { Position = Position + new Vector2(Sandbox.Game.Random.Float(-RAND_POS, RAND_POS), Sandbox.Game.Random.Float(-RAND_POS, RAND_POS)) };
            reviveSoul.Velocity = (reviveSoul.Position - Position) * Sandbox.Game.Random.Float(2f, 6f);
            Game.AddThing(reviveSoul);
        }
    }
}
