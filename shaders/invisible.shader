HEADER
{
	Description = "Invisible";
}

FEATURES
{
    #include "common/features.hlsl"
}

MODES
{
	VrForward();
	Depth(); 
	ToolsVis( S_MODE_TOOLS_VIS );
	ToolsWireframe( "vr_tools_wireframe.shader" );
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#define S_ALPHA_TEST 1
	#define S_TRANSLUCENT 0
	
	#include "common/shared.hlsl"
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
		return ProcessVertex( v );
	}
}

PS
{
	#include "sbox_pixel.fxc"

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		return float4(0,0,0,0);
	}
}
