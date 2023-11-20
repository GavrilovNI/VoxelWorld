HEADER
{
	Description = "Voxel Shader";
	Version = 1;
}

FEATURES
{
	#include "common/features.hlsl"
	
	Feature( F_ALPHA_TEST, 0..1, "Translucent" );
	Feature( F_TRANSLUCENT, 0..1, "Translucent" );
	FeatureRule( Allow1( F_TRANSLUCENT, F_ALPHA_TEST ), "Translucent and Alpha Test are not compatible" );
	Feature( F_PREPASS_ALPHA_TEST, 0..1, "Translucent" );
}

MODES
{
	VrForward(); // Indicates this shader will be used for main rendering
	ToolsVis( S_MODE_TOOLS_VIS ); // Ability to see in the editor
	ToolsWireframe(S_MODE_TOOLS_WIREFRAME ); // Allows for mat_wireframe to work
	//ToolsShadingComplexity("tools_shading_complexity.shader");  // Shows how expensive drawing is in debug view
	Depth( S_MODE_DEPTH );
}

COMMON
{
	//#include "common/shared.hlsl"
	
	#define VS_INPUT_HAS_TANGENT_BASIS 1
	#define PS_INPUT_HAS_TANGENT_BASIS 1
	
	#include "system.fxc" // This should always be the first include in COMMON
	#include "sbox_shared.fxc"
	float g_flVoxelSize < Default( 32.0 ); >;

	StaticCombo( S_ALPHA_TEST, F_ALPHA_TEST, Sys( ALL ) );
	StaticCombo( S_TRANSLUCENT, F_TRANSLUCENT, Sys( ALL ) );
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		PixelInput i = ProcessVertex( v );
        return FinalizeVertex(i);
	}
}

PS
{

	#define S_NON_DIRECTIONAL_DIFFUSE_LIGHTING 1
	float g_flTextureAtlasCellSize< Default(64.0f); >;
	StaticCombo( S_MODE_DEPTH, 0..1, Sys( ALL ) );
	StaticCombo( S_MODE_TOOLS_WIREFRAME, 0..1, Sys( ALL ) );
	StaticCombo( S_DO_NOT_CAST_SHADOWS, F_DO_NOT_CAST_SHADOWS, Sys( ALL ) );

	#if ( S_MODE_TOOLS_WIREFRAME )
		RenderState( FillMode, WIREFRAME );
		RenderState( SlopeScaleDepthBias, -0.5 );
		RenderState( DepthBiasClamp, -0.0005 );
		RenderState( DepthWriteEnable, false );
		#define DEPTH_STATE_ALREADY_SET
	#endif

	#include "common/pixel.hlsl"


	float g_flVoxelOpacity< Range(0.0f, 1.0f); Default(1.0f); >;

    //StaticCombo( S_ALPHA_TEST, F_ALPHA_TEST, Sys( ALL ) );
	
	//RenderState( AlphaTestEnable, false );
	//RenderState(BlendEnable, false);
	//RenderState(SrcBlend, SRC_ALPHA);
	//RenderState(DstBlend, INV_SRC_ALPHA);
	//RenderState(BlendOpAlpha, ADD);
	//RenderState(SrcBlendAlpha, SRC_ALPHA);
	//RenderState(DstBlendAlpha, INV_SRC_ALPHA);
	//RenderState(AlphaTestFunc, LESS_EQUAL);

	//SamplerState g_sSampler0 < Filter( POINT ); AddressU( WRAP ); AddressV( WRAP ); >;
	//CreateInputTexture2D( Texture_ps_0, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	//Texture2D g_tTexture_ps_0 < Channel( RGBA, Box( Texture_ps_0 ), Srgb ); OutputFormat( RGBA8888 ); SrgbRead( True ); >;
	
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		
			Material m = Material::From( i );
			float4 color = ShadingModelStandard::Shade( i, m );
			//color.a = i.vVertexColor.a;
			return color;
	}
}
