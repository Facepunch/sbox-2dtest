using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.UI;

namespace Sandbox
{
    public enum SpriteFilter
    {
		Default,
		Pixelated
    }

	public partial class Sprite : ModelEntity
	{
		private Material _material;
		private Texture _texture;

		private float _localRotation;

		public MyGame Game => MyGame.Current;

		[Net, Change]
		public string TexturePath { get; set; }

		[Net]
		public new Vector2 Scale { get; set; }

        [Net, Change]
		public SpriteFilter Filter { get; set; }

        [Net]
		public Color ColorFill { get; set; }

		public Vector2 Forward => Vector2.FromDegrees(Rotation + 180f);

		public new Vector2 Position
		{
			get => base.Position;
			set => base.Position = new Vector3( value.x, value.y, base.Position.z );
		}

		public new Vector2 LocalPosition
        {
			get => base.LocalPosition;
			set => base.LocalPosition = new Vector3(value.x, value.y, base.LocalPosition.z);
		}

		public float Depth
		{
			get => base.Position.z;
			set => base.Position = base.Position.WithZ( value );
		}

		public new float Rotation
		{
			get => base.Rotation.Angles().yaw + 90f;
			set
			{
				base.Rotation = global::Rotation.FromYaw( value - 90f );
			}
		}

		public new float LocalRotation
		{
			get => _localRotation;
			set
			{
				_localRotation = value;
				base.LocalRotation = global::Rotation.FromYaw(LocalRotation - 90f);
			}
		}

		public new Vector2 Velocity
		{
			get => base.Velocity;
			set => base.Velocity = new Vector3(value.x, value.y, 0f);
		}

        private static Dictionary<(string TexturePath, SpriteFilter Filter), Material> MaterialDict { get; } = new();

        private static Material GetMaterial( Texture texture, SpriteFilter filter )
        {
            if (MaterialDict.TryGetValue((texture.ResourcePath, filter), out var mat))
            {
                return mat;
            }

            var srcMat = Material.Load($"materials/sprite_{filter.ToString().ToLowerInvariant()}.vmat");
			
			mat = srcMat.CreateCopy();
            mat.OverrideTexture("g_tColor", texture);

			// TODO: this makes sprites render multiple times??
			// MaterialDict[(texture.ResourcePath, filter)] = mat;

            return mat;
        }

		private void OnTexturePathChanged()
		{
			_texture = string.IsNullOrEmpty( TexturePath )
				? Texture.White
				: Texture.Load( FileSystem.Mounted, TexturePath );

            _material = GetMaterial(_texture ?? Texture.White, Filter);

            SetMaterialOverride(_material);
		}

        private void OnFilterChanged()
		{
			_material = GetMaterial(_texture ?? Texture.White, Filter);

            SetMaterialOverride(_material);
		}
		
		public Sprite()
		{

		}

		public override void Spawn()
		{
            SetModel( "models/quad.vmdl" );

			EnableDrawing = true;

			Rotation = 0f;
			Scale = new Vector2( 1f, 1f );

            PhysicsEnabled = false;

            base.Spawn();
		}
		
		[Event.PreRender]
		private void ClientPreRender()
		{
			SceneObject.Flags.IsTranslucent = true;
			SceneObject.Attributes.Set( "SpriteScale", new Vector2(Scale.y, Scale.x) / 100f );
            SceneObject.Attributes.Set("TextureSize", _texture?.Size ?? new Vector2(1f, 1f));
			SceneObject.Attributes.Set("ColorFill", ColorFill);
		}
	}
}
