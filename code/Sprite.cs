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

    public readonly struct SpriteTexture
    {
        public static implicit operator SpriteTexture(string texturePath)
        {
            return new SpriteTexture(texturePath);
        }

        public static SpriteTexture Single(string texturePath)
        {
            return new SpriteTexture(texturePath);
        }

        public static SpriteTexture Atlas(string texturePath, int rows, int cols)
        {
            return new SpriteTexture(texturePath, rows, cols);
        }

        [ResourceType("png")]
		public string TexturePath { get; }

        public int AtlasRows { get; }
		public int AtlasColumns { get; }

        private SpriteTexture(string texturePath, int rows = 1, int cols = 1)
        {
            TexturePath = texturePath;
            AtlasRows = rows;
            AtlasColumns = cols;
        }
    }

    public partial class Sprite : ModelEntity
	{
		private Material _material;
		private Texture _texture;
		private SpriteAnimation _anim;

		private float _localRotation;

		public MyGame Game => MyGame.Current;

        public SpriteTexture SpriteTexture
        {
            get => SpriteTexture.Atlas(NetTexturePath, NetAtlasRows, NetAtlasColumns);
            set
            {
                NetTexturePath = value.TexturePath;
                NetAtlasRows = value.AtlasRows;
                NetAtlasColumns = value.AtlasColumns;

                if (this is Arrow)
                {
                    Log.Info($"hello {NetTexturePath} {value.TexturePath}");
                }

                if (IsClient)
                {
					OnNetTexturePathChanged();
                }
            }
        }

        [Net, Change, Predicted]
		private string NetTexturePath { get; set; }

        [Net, Predicted]
		private int NetAtlasRows { get; set; }

        [Net, Predicted]
        private int NetAtlasColumns { get; set; }

        [Net, Change, ResourceType("frames"), Predicted]
		private string NetAnimationPath { get; set; }

        public string AnimationPath
        {
            get => NetAnimationPath;
            set
            {
                NetAnimationPath = value;

                if (IsClient)
                {
                    OnNetAnimationPathChanged();
                }
            }
        }

        public float AnimationTimeElapsed { get; set; }

        [Net] public float AnimationSpeed { get; set; }

        [Net, Predicted] public new Vector2 Scale { get; set; } = new Vector2(1f, 1f);

		[Net, Predicted]
        public Vector2 Pivot { get; set; } = new Vector2(0.5f, 0.5f);

        [Net, Change, Predicted]
        private SpriteFilter NetFilter { get; set; }

        public SpriteFilter Filter
        {
            get => NetFilter;
            set
            {
                NetFilter = value;

                if (IsClient)
                {
                    OnNetFilterChanged();
                }
            }
        }

        [Net, Predicted]
		public Color ColorFill { get; set; }

        [Net, Predicted] public Color ColorTint { get; set; } = Color.White;

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
			set => base.Rotation = global::Rotation.FromYaw( value - 90f );
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

        private void UpdateMaterial()
		{
			_material = GetMaterial(_texture ?? Texture.White, Filter);

            SetMaterialOverride(_material);
		}

		private void OnNetTexturePathChanged()
		{
			_texture = string.IsNullOrEmpty(NetTexturePath)
				? Texture.White
				: Texture.Load( FileSystem.Mounted, NetTexturePath);

            UpdateMaterial();
        }

        private void OnNetAnimationPathChanged()
        {
            _anim = string.IsNullOrEmpty(AnimationPath)
                ? null
                : ResourceLibrary.Get<SpriteAnimation>(AnimationPath);

			AnimationTimeElapsed = 0f;
		}

        private void OnNetFilterChanged()
		{
            UpdateMaterial();
		}
		
		public Sprite()
        {
            if (IsServer || IsClientOnly)
            {
                SetModel("models/quad.vmdl");

                EnableDrawing = true;
                PhysicsEnabled = false;
            }
        }

        public override void Spawn()
        {
            base.Spawn();

            if (IsServer || IsClientOnly)
            {
                AnimationSpeed = 1f;
                Rotation = 0f;
            }
        }

        [Event.PreRender]
		private void ClientPreRender()
		{
			if (SceneObject == null)
				return;

			SceneObject.Flags.IsTranslucent = true;
			SceneObject.Attributes.Set( "SpriteScale", new Vector2(Scale.y, Scale.x) / 100f );
            SceneObject.Attributes.Set("SpritePivot", new Vector2(Pivot.y, Pivot.x));
            SceneObject.Attributes.Set("TextureSize", _texture?.Size ?? new Vector2(1f, 1f));
			SceneObject.Attributes.Set("ColorFill", ColorFill);
            SceneObject.Attributes.Set("ColorMultiply", ColorTint);

            if (_anim != null)
            {
				AnimationTimeElapsed += Time.Delta * AnimationSpeed;
				var (min, max) = _anim.GetFrameUvs(AnimationTimeElapsed, NetAtlasRows, NetAtlasColumns);

                SceneObject.Attributes.Set("UvMin", min);
                SceneObject.Attributes.Set("UvMax", max);
			}
            else
			{
				SceneObject.Attributes.Set("UvMin", Vector2.Zero);
                SceneObject.Attributes.Set("UvMax", Vector2.One);
			}
		}
	}
}
