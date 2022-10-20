﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Test2D.MyGame;
using Sandbox;

namespace Test2D;
public partial class EnemyBullet : Thing
{
	public TimeSince SpawnTime { get; private set; }

	public Enemy Shooter { get; set; }

	public float Damage { get; set; }
	public float Lifetime { get; set; }
	public Vector2 Direction { get; set; }
	public float Speed { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		if(Host.IsServer)
        {
			SpriteTexture = "textures/sprites/bullet.png";

			Scale = new Vector2(0.3f, 0.3f);
			SpawnTime = 0f;
			Damage = 10f;
			Radius = 0.1f;
			Pivot = new Vector2(0.5f, -0.9f);
			ShadowOpacity = 0.8f;
			ShadowScale = 0.6f;
			ColorTint = Color.Red;
			Lifetime = 6f;
			Speed = 1.8f;

			CollideWith.Add(typeof(PlayerCitizen));
		}

		Filter = SpriteFilter.Pixelated;
	}

    public override void ClientSpawn()
    {
        base.ClientSpawn();

		SpawnShadow(Radius * 3f);
	}

	public override void Update(float dt)
	{
		if (Game.IsGameOver)
			return;

		base.Update(dt);

		var speed = Speed * Utils.Map(SpawnTime, 0f, 0.5f, 0f, 1f, EasingType.QuadInOut);

		Position += Direction * speed * dt;
		Depth = -Position.y * 10f;

		if (SpawnTime > Lifetime)
        {
			Remove();
			return;
		}

		var gridPos = Game.GetGridSquareForPos(Position);
		if (gridPos != GridPos)
		{
			Game.DeregisterThingGridSquare(this, GridPos);
			Game.RegisterThingGridSquare(this, gridPos);
			GridPos = gridPos;
		}

		for (int dx = -1; dx <= 1; dx++)
		{
			for (int dy = -1; dy <= 1; dy++)
			{
				Game.HandleThingCollisionForGridSquare(this, new GridSquare(GridPos.x + dx, GridPos.y + dy), dt);

				if (IsRemoved)
					return;
			}
		}
    }

	public override void Colliding(Thing other, float percent, float dt)
	{
		base.Colliding(other, percent, dt);

		if (typeof(PlayerCitizen).IsAssignableFrom(other.GetType()))
		{
			var player = (PlayerCitizen)other;

			if(!player.IsDead)
            {
				player.Damage(Damage);
				player.Velocity += Direction * 2f;
				Remove();
			}
		}
	}
}
