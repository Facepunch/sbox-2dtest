using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public partial class Bullet : Sprite
	{
		public TimeSince SpawnTime { get; private set; }

		public PlayerCitizen Shooter { get; set; }

		public float Damage { get; private set; }

		public bool HasFinished { get; private set; }

		public override void Spawn()
		{
			base.Spawn();

			TexturePath = "textures/sprites/bullet.png";

			//RenderColor = Color.Random;

			Scale = new Vector2(0.2f, 0.2f);
			SpawnTime = 0f;
			Damage = 15f;
		}

		[Event.Tick.Server]
		public void ServerTick()
        {
			if (HasFinished)
				return;

			Position += (Vector2)Velocity * Time.Delta;

			if (SpawnTime > 1f)
				Delete();

			var gridSquare = Game.GetGridSquareForPos(Position);

			for (int dx = -1; dx <= 1; dx++)
			{
				for (int dy = -1; dy <= 1; dy++)
				{
					HandleCollisionForGridSquare((gridSquare.x + dx, gridSquare.y + dy));
					if (HasFinished)
						return;
				}
			}
        }

		void HandleCollisionForGridSquare((int, int) gridSquare)
		{
			//Log.Info("HandleCollisionForGridSquare - " + _enemyGridPositions[gridSquare].Count);
			var enemies = Game.GetEnemiesInGridSquare(gridSquare);
			if (enemies == null)
				return;

			for (int i = enemies.Count - 1; i >= 0; i--)
            {
				Enemy enemy = enemies[i];
				if (enemy == null) 
					continue;

				var dist_sqr = (enemy.Position - Position).LengthSquared;
				var total_radius_sqr = MathF.Pow(enemy.Radius + 0.05f, 2f);
				if (dist_sqr < total_radius_sqr)
				{
					enemy.Damage(Rand.Float(1f, 20f));
					Delete();
                    HasFinished = true;
                    return;
                }
			}
		}
	}
}
