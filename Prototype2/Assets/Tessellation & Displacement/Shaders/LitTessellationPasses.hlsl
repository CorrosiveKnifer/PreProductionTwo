#ifndef UNIVERSAL_LIT_TESSELLATION_INCLUDED
#define UNIVERSAL_LIT_TESSELLATION_INCLUDED

#include "Library/Tessellation.hlsl"

#if defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES3) || defined(SHADER_API_METAL) || defined(SHADER_API_VULKAN) || defined(SHADER_API_GLES)
#define UNITY_COMPILER_HLSL
#define UNITY_COMPILER_HLSLCC
#elif defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE)
#define UNITY_COMPILER_HLSL
#else
#define UNITY_COMPILER_CG
#endif

// Vertex shader
TessellationControlPoint TessellatedVertex(TessellationAttributes input)
{
	TessellationControlPoint output = (TessellationControlPoint)0;

	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);

#if HAS_POSITION
	output.POSITION_ID = input.POSITION_ID;
#endif
#if HAS_NORMAL
	output.NORMAL_ID = input.NORMAL_ID;
#endif
#if HAS_TANGENT
	output.TANGENT_ID = input.TANGENT_ID;
#endif
#if HAS_UV0
	output.UV0_ID = input.UV0_ID;
#endif
#if HAS_UV1
	output.UV1_ID = input.UV1_ID;
#endif
#if HAS_UV2
	output.UV2_ID = input.UV2_ID;
#endif
#if HAS_UV3
	output.UV3_ID = input.UV3_ID;
#endif
#if HAS_VERTEX_COLOR
	output.VERTEX_COLOR_ID = input.VERTEX_COLOR_ID;
#endif

	return output;
}

// Hull shader
[maxtessfactor(MAX_TESSELLATION_FACTORS)]
[domain("tri")]						// Processing triangle face
[partitioning("fractional_odd")]	// The parameter type of the subdivided factor, can be "integer" which is used to represent the integer, or can be a floating point number "fractional_odd"
[outputtopology("triangle_cw")]		// Clockwise vertex arranged as the front of the triangle
[patchconstantfunc("HullConstant")] // The function that calculates the factor of the triangle facet is not a constant. Different triangle faces can have different values. A constant can be understood as a uniform value for the three vertices inside a triangle face.
[outputcontrolpoints(3)]			// Explicitly point out that each patch handles three vertex data
TessellationControlPoint TessellatedHull(
	InputPatch<TessellationControlPoint, 3> patch,
	uint id:SV_OutputControlPointID)
{
	return patch[id];
}

// Domain shader
[domain("tri")]	// Specified to handle the triangle face triangle
TessellatedVaryings TessellatedDomain(
	TessellationFactors tessFactors,
	const OutputPatch<TessellationControlPoint, 3> input,
	float3 barycentricCoordinates : SV_DomainLocation)
{
	// Program output
	TessellatedVaryings varyings = (TessellatedVaryings)0;

	// Native vertex program input
	Attributes data = (Attributes)0;

	// ID Transfer for instanced materials
	UNITY_TRANSFER_INSTANCE_ID(input[0], data);

	// This uses a macro definition to reduce the writing of duplicate code
#define MY_DOMAIN_PROGRAM_INTERPOLATE(fieldName) input[0].fieldName * barycentricCoordinates.x + \
					input[1].fieldName * barycentricCoordinates.y + \
					input[2].fieldName * barycentricCoordinates.z

	// Data interpolations
#if HAS_POSITION
	float4 POSITION_ID = MY_DOMAIN_PROGRAM_INTERPOLATE(POSITION_ID);
#if HAS_VERTEX_POSITION
	data.POSITION_ID = POSITION_ID;
#endif
#endif

#if HAS_NORMAL
	float3 NORMAL_ID = MY_DOMAIN_PROGRAM_INTERPOLATE(NORMAL_ID);
#if HAS_VERTEX_NORMAL
	data.NORMAL_ID = NORMAL_ID;
#endif
#endif

#if HAS_TANGENT
	float4 TANGENT_ID = MY_DOMAIN_PROGRAM_INTERPOLATE(TANGENT_ID);
#if HAS_VERTEX_TANGENT
	data.TANGENT_ID = TANGENT_ID;
#endif
#endif

#if HAS_UV0
	float2 UV0_ID = MY_DOMAIN_PROGRAM_INTERPOLATE(UV0_ID);
#if HAS_VERTEX_UV0
	data.UV0_ID = UV0_ID;
#endif
#endif

#if HAS_UV1
#if defined(UNITY_COMPILER_HLSLCC)	// Temporal workaround until Unity fixes its shader compiler
	float3 fixer = float3(1.0, 1.0, 1.0000001);
	float3 fixedBC = barycentricCoordinates * fixer * (1.0 / fixer);
	float2 UV1_ID = input[0].UV1_ID * fixedBC.x + input[1].UV1_ID * fixedBC.y + input[2].UV1_ID * fixedBC.z;
#else
	float2 UV1_ID = MY_DOMAIN_PROGRAM_INTERPOLATE(UV1_ID);
#endif
#if HAS_VERTEX_UV1
	data.UV1_ID = UV1_ID;
#endif
#endif

#if HAS_UV2
	float2 UV2_ID = MY_DOMAIN_PROGRAM_INTERPOLATE(UV2_ID);
#if HAS_VERTEX_UV2
	data.UV2_ID = UV2_ID;
#endif
#endif

#if HAS_UV3
	float2 UV3_ID = MY_DOMAIN_PROGRAM_INTERPOLATE(UV3_ID);
#if HAS_VERTEX_UV3
	data.UV3_ID = UV3_ID;
#endif
#endif

#if HAS_VERTEX_COLOR
	varyings.VERTEX_COLOR_ID = MY_DOMAIN_PROGRAM_INTERPOLATE(VERTEX_COLOR_ID);
#endif

	// Phong displacement processing
#ifdef _TESSELLATION_PHONG
	float3 p0 = mul(unity_ObjectToWorld, input[0].POSITION_ID).xyz;
	float3 p1 = mul(unity_ObjectToWorld, input[1].POSITION_ID).xyz;
	float3 p2 = mul(unity_ObjectToWorld, input[2].POSITION_ID).xyz;
	float3 n0 = TransformObjectToWorldNormal(input[0].NORMAL_ID);
	float3 n1 = TransformObjectToWorldNormal(input[1].NORMAL_ID);
	float3 n2 = TransformObjectToWorldNormal(input[2].NORMAL_ID);
	float3 positionWS = mul(unity_ObjectToWorld, data.POSITION_ID).xyz;
	positionWS = PhongTessellation(positionWS,
		p0, p1, p2, n0, n1, n2,
		barycentricCoordinates, 
		_TessellationPhongShape);
	data.POSITION_ID = mul(unity_WorldToObject, float4(positionWS, 1.0));
#endif

	// Apply displacement
#if _HEIGHTMAP && HAS_NORMAL
#if WORLDSPACE_HEIGHTMAP_SAMPLING
#ifndef _TESSELLATION_PHONG
	float3 positionWS = mul(unity_ObjectToWorld, data.POSITION_ID).xyz;
#endif
	float3 normalWS = mul(unity_ObjectToWorld, outputNormal).xyz;
	data.POSITION_ID.xyz += normalize(outputNormal) * SampleHeightWorldCoordinates(positionWS, normalWS);
#else
	float2 samplingUV = TRANSFORM_TEX(GEOMETRY_UV_CHANNEL, TESSELLATION_TRANSFORM_SOURCE);
	data.POSITION_ID.xyz += normalize(NORMAL_ID) * SampleHeight(samplingUV);
#endif
#endif

	// Invoke native vertex program
	varyings.nativeVaryings = PassVertexProgram(data);	// Process the interpolation results, prepare the data needed in the rasterization phase
	
	return varyings;		
}

// Fragment shader
half4 TessellatedFragment(TessellatedVaryings input) : SV_Target
{
#if HAS_VERTEX_COLOR
	_VertexColor = input.VERTEX_COLOR_ID;
#endif
	half4 color = PassFragmentProgram(input.nativeVaryings);
	return color;
}

#endif //UNIVERSAL_LIT_TESSELLATION_INCLUDED
