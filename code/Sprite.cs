using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.UI;
using Sandbox;

namespace Test2D;

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

public partial class Sprite : Entity
{
	private Material _material;
	private Texture _texture;
	private SpriteAnimation _anim;

	private float _localRotation;

	private string _lastAnimPath;
	private string _lastTexturePath;
	private SpriteFilter _lastFilter;

	internal SceneObject SceneObject { get; }

	public MyGame Game => MyGame.Current;

	public SpriteTexture SpriteTexture
	{
		get => IsClientOnly ? ClientSpriteTexture : SpriteTexture.Atlas(NetTexturePath, NetAtlasRows, NetAtlasColumns);
		set
		{
			var oldValue = ClientSpriteTexture;

			ClientSpriteTexture = value;

			NetTexturePath = value.TexturePath;
			NetAtlasRows = value.AtlasRows;
			NetAtlasColumns = value.AtlasColumns;
		}
	}

	[Net, Predicted]
	private string NetTexturePath { get; set; }

	[Net]
	private int NetAtlasRows { get; set; }

	[Net]
	private int NetAtlasColumns { get; set; }

	private SpriteTexture ClientSpriteTexture { get; set; }

	[Net, Predicted]
	private string NetAnimationPath { get; set; }
	private string ClientAnimationPath { get; set; }

	[ResourceType( "frames" )]
	public string AnimationPath
	{
		get => IsClientOnly ? ClientAnimationPath : NetAnimationPath;
		set
		{
			var oldValue = ClientAnimationPath;

			NetAnimationPath = ClientAnimationPath = value;
		}
	}

	public float AnimationTimeElapsed { get; set; }

	[Net, Predicted]
	private float NetAnimationSpeed { get; set; }
	private float ClientAnimationSpeed { get; set; }

	public float AnimationSpeed
	{
		get => IsClientOnly ? ClientAnimationSpeed : NetAnimationSpeed;
		set => NetAnimationSpeed = ClientAnimationSpeed = value;
	}

	[Net, Predicted]
	private Vector2 NetScale { get; set; } = new Vector2( 1f, 1f );
	private Vector2 ClientScale { get; set; } = new Vector2( 1f, 1f );

	public new Vector2 Scale
	{
		get => IsClientOnly ? ClientScale : NetScale;
		set => NetScale = ClientScale = value;
	}

	[Net, Predicted]
	private Vector2 NetPivot { get; set; } = new Vector2(0.5f, 0.5f);
	private Vector2 ClientPivot { get; set; } = new Vector2( 0.5f, 0.5f );

	public Vector2 Pivot
	{
		get => IsClientOnly ? ClientPivot : NetPivot;
		set => NetPivot = ClientPivot = value;
	}

	[Net, Predicted]
	private SpriteFilter NetFilter { get; set; }
	private SpriteFilter ClientFilter { get; set; }

	public SpriteFilter Filter
	{
		get => IsClientOnly ? ClientFilter : NetFilter;
		set
		{
			var oldValue = ClientFilter;

			NetFilter = ClientFilter = value;
		}
	}

	[Net, Predicted]
	private Color NetColorFill { get; set; }
	private Color ClientColorFill { get; set; }

	public Color ColorFill
	{
		get => IsClientOnly ? ClientColorFill : NetColorFill;
		set => NetColorFill = ClientColorFill = value;
	}

	[Net, Predicted]
	private Color NetColorTint { get; set; } = Color.White;
	private Color ClientColorTint { get; set; } = Color.White;

	public Color ColorTint
	{
		get => IsClientOnly ? ClientColorTint : NetColorTint;
		set => NetColorTint = ClientColorTint = value;
	}

	[Net, Predicted]
	private float NetOpacity { get; set; } = 1f;
	private float ClientOpacity { get; set; } = 1f;

	public float Opacity
	{
		get => IsClientOnly ? ClientOpacity : NetOpacity;
		set => NetOpacity = ClientOpacity = value;
	}

	[Net, Predicted]
	private Rect NetUvRect { get; set; } = new Rect( 0f, 0f, 1f, 1f );
	private Rect ClientUvRect { get; set; } = new Rect( 0f, 0f, 1f, 1f );

	/// <summary>
	/// Useful for offsetting or tiling this sprite's texture.
	/// Only supported for non-animated sprites for now.
	/// </summary>
	public Rect UvRect
	{
		get => IsClientOnly ? ClientUvRect : NetUvRect;
		set => NetUvRect = ClientUvRect = value;
	}

	public Vector2 Forward => Vector2.FromDegrees(Rotation + 180f);

	public new Vector2 Position
	{
		get => base.Position;
		set
		{
			if ( float.IsNaN( value.x ) || float.IsNaN( value.y ) )
			{
				Log.Error( "Tried to set a NaN position!" );
				return;
			}

			base.Position = new Vector3( value.x, value.y, base.Position.z );
		}
	}

	public new Vector2 LocalPosition
	{
		get => base.LocalPosition;
		set
		{
			if ( float.IsNaN( value.x ) || float.IsNaN( value.y ) )
			{
				Log.Error( "Tried to set a NaN local position!" );
				return;
			}

			base.LocalPosition = new Vector3( value.x, value.y, base.LocalPosition.z );
		}
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

    public new Sprite Parent
    {
        get => base.Parent as Sprite;
        set
        {
            if ( base.Parent == value ) return;

			(base.Parent as Sprite)?.SceneObject?.RemoveChild( SceneObject );

            base.Parent = value;

            value?.SceneObject?.AddChild( Name, SceneObject );
        }
    }

    private static Dictionary<SpriteFilter, Material> BaseMaterialDict { get; } = new();

	private static Material GetMaterial( Texture texture, SpriteFilter filter )
	{

        if ( !BaseMaterialDict.TryGetValue( filter, out var srcMat ) )
        {
            srcMat = Material.Load( $"materials/sprite_{filter.ToString().ToLowerInvariant()}.vmat" );
            BaseMaterialDict[filter] = srcMat;
        }

		var mat = srcMat.CreateCopy();
		mat.Set( "g_tColor", texture );

		return mat;
	}

    public Sprite()
    {
        if ( Sandbox.Game.IsClient )
        {
            SceneObject = new SceneObject( Sandbox.Game.SceneWorld, "models/quad.vmdl", Transform ) { Flags = { IsTranslucent = true } };
        }
    }

	private void UpdateTexture()
    {
        if ( !SceneObject.IsValid() ) return;
        if ( _lastTexturePath == SpriteTexture.TexturePath ) return;

		try
		{
			_texture = string.IsNullOrEmpty( SpriteTexture.TexturePath )
				? Texture.White
				: Texture.Load( FileSystem.Mounted, SpriteTexture.TexturePath );

			UpdateMaterial();
		}
		finally
		{
			_lastTexturePath = SpriteTexture.TexturePath;
		}
	}
	
	private void UpdateMaterial()
    {
        if ( !SceneObject.IsValid() ) return;
		if ( _lastTexturePath == SpriteTexture.TexturePath && _lastFilter == Filter ) return;

		try
		{
			_material = GetMaterial( _texture ?? Texture.White, Filter );
			SceneObject.SetMaterialOverride( _material );
		}
		finally
		{
			_lastFilter = Filter;
		}
	}

	private void UpdateAnim()
    {
        if ( !SceneObject.IsValid() ) return;
        if ( _lastAnimPath == AnimationPath ) return;

		try
		{
			_anim = string.IsNullOrEmpty( AnimationPath )
				? null
				: ResourceLibrary.Get<SpriteAnimation>( AnimationPath );

			AnimationTimeElapsed = 0f;
		}
		finally
		{
			_lastAnimPath = AnimationPath;
		}
	}

	private void UpdateSceneObject()
	{
		if ( Parent is { } parent && parent.SceneObject.IsValid() )
		{
			SceneObject.Transform = Transform.Concat( parent.SceneObject.Transform, new Transform( base.LocalPosition, base.LocalRotation, base.LocalScale ) );
		}
		else
		{
			SceneObject.Transform = Transform;
		}

		// TODO
		SceneObject.Bounds = new BBox( float.NegativeInfinity, float.PositiveInfinity );

		SceneObject.Attributes.Set( "SpriteScale", new Vector2( Scale.y, Scale.x ) / 100f );
		SceneObject.Attributes.Set( "SpritePivot", new Vector2( Pivot.y, Pivot.x ) );
		SceneObject.Attributes.Set( "TextureSize", _texture?.Size ?? new Vector2( 1f, 1f ) );
		SceneObject.Attributes.Set( "ColorFill", ColorFill );
		SceneObject.Attributes.Set( "ColorMultiply", ColorTint.WithAlphaMultiplied( Opacity ) );

		if ( _anim != null )
		{
			AnimationTimeElapsed += Time.Delta * AnimationSpeed;
			var (min, max) = _anim.GetFrameUvs( AnimationTimeElapsed, SpriteTexture.AtlasRows, SpriteTexture.AtlasColumns );

			SceneObject.Attributes.Set( "UvMin", min );
			SceneObject.Attributes.Set( "UvMax", max );
		}
		else
		{
			SceneObject.Attributes.Set( "UvMin", UvRect.TopLeft );
			SceneObject.Attributes.Set( "UvMax", UvRect.BottomRight );
		}
	}
	
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		if ( IsClientOnly || Sandbox.Game.IsServer )
		{
			EnableDrawing = true;

			AnimationSpeed = 1f;
			Rotation = 0f;
		}
	}

	public sealed override void Simulate( IClient cl )
	{
		OnSimulate( cl );

		if ( Sandbox.Game.IsClient )
		{
			UpdateTexture();
			UpdateMaterial();
			UpdateAnim();
			UpdateSceneObject();
		}
	}

	protected virtual void OnSimulate( IClient cl )
	{

	}

	[Event.PreRender]
	private void ClientPreRender()
	{
        SceneObject.RenderingEnabled = EnableDrawing && Opacity > 0f;

		if ( !SceneObject.RenderingEnabled ) return;
		
		if ( !IsLocalPawn )
		{
			UpdateTexture();
			UpdateMaterial();
			UpdateAnim();
			UpdateSceneObject();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

        SceneObject?.Delete();
	}
}
