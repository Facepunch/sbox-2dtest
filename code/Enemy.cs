using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public partial class Enemy : Sprite
	{
		public float Radius { get; private set; }
		public float TempWeight { get; set; }
		public float FeetOffset { get; private set; }

		public float MoveTimeOffset { get; set; }
		public float MoveTimeSpeed { get; set; }


		public (int x, int y) GridPos { get; set; }

		private float _flashTimer;
		private bool _isFlashing;

		private Sprite _shadow;

		public override void Spawn()
		{
			base.Spawn();

			//TexturePath = "textures/sprites/mummy_walk3.png";
			//TexturePath = "textures/sprites/zombie_bw.png";
			// TexturePath = "textures/sprites/mummy_walk3.png";
			SpriteTexture = "textures/sprites/zombie.png";

			//Scale = new Vector2(1f, 35f / 16f) * 0.5f;
			//RenderColor = Color.Random;
			//Rotation = Time.Now * 0.1f;
			FeetOffset = 0.35f;
			Radius = 0.3f;
			Health = 40f;
			MoveTimeOffset = Rand.Float(0f, 4f);
			MoveTimeSpeed = Rand.Float(6f, 9f);

            Filter = SpriteFilter.Pixelated;
			//ColorFill = new ColorHsv(Rand.Float(0f, 360f), 0.5f, 1f, 0.125f);
			ColorFill = new ColorHsv(0f, 0f, 0f, 0f);

			//_shadow = new Shadow();
			//_shadow.Parent = this;
			//_shadow.LocalPosition = new Vector2(0.35f, 0f);
		}

		[Event.Tick.Server]
		public void ServerTick()
		{
			//if(TempWeight > 0.01f)
			//	DebugOverlay.Text(TempWeight.ToString(), Position);

            //DebugOverlay.Text(Depth.ToString("#.##"), Position);
            //FeetOffset = 0.35f;
            //DebugOverlay.Line(Position, Position + Velocity, 0f, false);
		}

		public void Update(float dt)
        {
			var closestPlayer = Game.GetClosestPlayer(Position);
			Velocity += (closestPlayer.Position - Position).Normal * 1.0f * dt;
			Position += Velocity * dt * (0.75f + Utils.FastSin(MoveTimeOffset + Time.Now * MoveTimeSpeed) * 0.25f);
			Position = new Vector2(MathX.Clamp(Position.x, Game.BOUNDS_MIN.x + Radius, Game.BOUNDS_MAX.x - Radius), MathX.Clamp(Position.y, Game.BOUNDS_MIN.y + Radius, Game.BOUNDS_MAX.y - Radius));
			Velocity *= 0.975f;

			//enemy.Rotation = enemy.Velocity.LengthSquared * Utils.FastSin(Time.Now * 12f);
			//enemy.Rotation = enemy.Velocity.Length * Utils.FastSin(Time.Now * MathF.PI * 7f) * 4.5f;

			//DebugOverlay.Line(enemy.Position, enemy.Position + enemy.Radius, 0f, false);

			Scale = new Vector2(1f * Velocity.x < 0f ? 1f : -1f, 1f) * 0.8f;
			Depth = -Position.y * 10f;

			var gridPos = Game.GetGridSquareForPos(Position);
			if (gridPos != GridPos)
			{
				Game.DeregisterEnemyGridSquare(this, GridPos);
				Game.RegisterEnemyGridSquare(this, gridPos);
				GridPos = gridPos;
			}

			for (int dx = -1; dx <= 1; dx++)
			{
				for (int dy = -1; dy <= 1; dy++)
				{
					Game.HandleCollisionForGridSquare(this, (GridPos.x + dx, GridPos.y + dy), dt);
				}
			}

			TempWeight *= 0.92f;

			if (_isFlashing)
            {
				_flashTimer -= dt;
				if(_flashTimer < 0f)
                {
					_isFlashing = false;
					ColorFill = new ColorHsv(0f, 0f, 0f, 0f);
				}
            }

			//ColorFill = new ColorHsv(0f, 0f, 0f, 0f);
			//ColorFill = new ColorHsv(0.5f + Utils.FastSin(Time.Now * 4f) * 0.5f, 0.5f + Utils.FastSin(Time.Now * 3f) * 0.5f, 0.5f + Utils.FastSin(Time.Now * 2f) * 0.5f, 0.5f + Utils.FastSin(Time.Now * 1f) * 0.5f);
			//ColorFill = new ColorHsv(0.94f, 0.157f, 0.392f, 1f);
			//ColorFill = new ColorHsv(0f, 0f, 0f, 0f);



			//ColorFill = new Color(0.2f, 0.2f, 1f) * 1.5f; // frozen
			//ColorFill = new Color(1f, 1f, 0.1f) * 2.5f; // shock
			//ColorFill = new Color(0.1f, 1f, 0.1f) * 2.5f; // poison

			//ColorFill = Color.White;
			//ColorFill = new ColorHsv(1f, 0f, 0f, 0f);
		}

		public void Damage(float damage)
        {
			Health -= damage;
			DamageNumbers.Create(Position + new Vector2(Rand.Float(-1f, 1f), Rand.Float(-2f, 2f)) * 0.1f, damage);
			Flash(0.12f);

			if (Health <= 0f)
            {
				Remove();
			}
        }

		public void Remove()
        {
			Game.RemoveEnemy(this);
			Delete();
		}

		public void Flash(float time)
        {
			if (_isFlashing)
				return;

			ColorFill = new ColorHsv(1f, 0f, 15f, 1f);
			_isFlashing = true;
			_flashTimer = time;
		}
	}
}
