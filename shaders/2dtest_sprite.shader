//=========================================================================================================================
// Optional
//=========================================================================================================================
HEADER
{
	Description = "2D sprite shader";
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
FEATURES
{
	#include "common/features.hlsl"

	Feature(F_PIXELATED, 0..1, "Sprite");
}

//=========================================================================================================================
COMMON
{
	#include "common/shared.hlsl"

	// #define S_TRANSLUCENT 1
}

//=========================================================================================================================

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

//=========================================================================================================================

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

//=========================================================================================================================

VS
{
	#include "common/vertex.hlsl"
	
	float2 g_vSpriteScale < UiType( VectorText ); Default2( 1.0, 1.0 ); UiGroup( "Transform,10/10" ); Attribute( "SpriteScale" ); >;
	float2 g_vSpritePivot < UiType( VectorText ); Default2( 0.5, 0.5 ); UiGroup( "Transform,10/20" ); Attribute( "SpritePivot" ); >;
	float2 g_vUvMin < UiType( VectorText ); Default2( 0.0, 0.0 ); UiGroup( "Transform,10/20" ); Attribute( "UvMin" ); >;
	float2 g_vUvMax < UiType( VectorText ); Default2( 1.0, 1.0 ); UiGroup( "Transform,10/30" ); Attribute( "UvMax" ); >;
	
	//
	// Main
	//
	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VertexInput i ) )
	{
		i.vPositionOs.xy += (g_vSpritePivot - float2(0.5, 0.5)) * 100.0;
		i.vPositionOs.xy *= g_vSpriteScale;

		PixelInput o = ProcessVertex( i );

		o.vTextureCoords.xy = g_vUvMin + o.vTextureCoords.xy * (g_vUvMax - g_vUvMin);
		
		return FinalizeVertex( o );
	}
}

//=========================================================================================================================

PS
{
	CreateInputTexture2D( Texture, Srgb, 8, "", "", "Color", Default3( 1.0, 1.0, 1.0 ) );
	CreateTexture2DInRegister( g_tColor, 0 ) < Channel( RGBA, None( Texture ), Srgb ); OutputFormat( DXT5 ); SrgbRead( true ); Filter( BILINEAR ); >;
	TextureAttribute( RepresentativeTexture, g_tColor );
	
	float2 g_vTextureSize < UiType( VectorText ); Default2( 1.0, 1.0 ); UiGroup( "Transform,10/40" ); Attribute( "TextureSize" ); >;
	float4 g_vColorFill < UiType( Color ); Default4( 0.0, 0.0, 0.0, 0.0 ); UiGroup( "Color,10/10" ); Attribute( "ColorFill" ); >;
	float4 g_vColorMultiply < UiType( Color ); Default4( 1.0, 1.0, 1.0, 1.0 ); UiGroup( "Color,10/20" ); Attribute( "ColorMultiply" ); >;

	StaticCombo( S_PIXELATED, F_PIXELATED, Sys( ALL ) );

	RenderState( BlendEnable, true );
	RenderState( SrcBlend, SRC_ALPHA );
	RenderState( DstBlend, INV_SRC_ALPHA );

	RenderState( DepthEnable, false );
	RenderState( DepthClipEnable, false );
	RenderState( DepthWriteEnable, false );

	// Always write rgba
	RenderState( ColorWriteEnable0, RGBA );
	RenderState( FillMode, SOLID );

	// Never cull
	RenderState( CullMode, NONE );

	struct PS_OUTPUT
	{
		float4 vColor : SV_Target0;
	};
	
	//
	// Main
	//
	PS_OUTPUT MainPs( PixelInput i )
	{
		PS_OUTPUT o;
		
		#if ( S_PIXELATED )
			float2 texSize = g_vTextureSize;

			float2 texelScale = float2(
				1.0 / length(float2(ddx(i.vTextureCoords.x), ddy(i.vTextureCoords.x))),
				1.0 / length(float2(ddx(i.vTextureCoords.y), ddy(i.vTextureCoords.y))))
				/ texSize;

			float2 texel = i.vTextureCoords.xy * texSize;
			float2 nearestEdge = floor(texel + 0.5);
			float2 dist = (texel - nearestEdge) * texelScale;

			float2 factor = clamp(dist * 2.0, -1.0, 1.0);
			float2 aaTexel = (nearestEdge + 0.5 * factor) / texSize;

			o.vColor = Tex2D( g_tColor, aaTexel ) * i.vVertexColor;
		#else
			o.vColor = Tex2D( g_tColor, i.vTextureCoords ) * i.vVertexColor;
		#endif

		o.vColor.rgb = lerp(o.vColor.rgb * g_vColorMultiply.rgb, g_vColorFill.rgb, g_vColorFill.a);
		o.vColor.a = o.vColor.a * g_vColorMultiply.a;

		return o;
	}
}
