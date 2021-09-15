#ifndef UNIVERSAL_LIT_TESSELLATION_INPUT_INCLUDED
#define UNIVERSAL_LIT_TESSELLATION_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Library/Blur.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
half4 _SpecColor;
half4 _EmissionColor;
half _Cutoff;
half _Smoothness;
half _Metallic;
half _BumpScale;
half _OcclusionStrength;
half _Surface;

half _VertexColorBlending;
float4 _TessellationMap_ST;
half _TessellationScale;
half _TessellationFactor;
half _TessellationFactorMin;
half _TessellationFactorMax;
half _TessellationDistanceMin;
half _TessellationDistanceMax;
half _TessellationEdgeLength;
half _TessellationEdgeDistanceOffset;
half _TessellationPhongShape;
half _TessellationTriangleClipBias;
half _HeightStrength;
half _HeightBase;
half _HeightBlurSize;
half _HeightBlurSamples;
half _HeightBlurGaussStandardDeviation;
half _GeometryCustomST;
half _GeometryUVChannel;
CBUFFER_END

CBUFFER_START(CustomPerMaterial)
half4 _VertexColor;
CBUFFER_END

TEXTURE2D(_OcclusionMap);       SAMPLER(sampler_OcclusionMap);
TEXTURE2D(_MetallicGlossMap);   SAMPLER(sampler_MetallicGlossMap);
TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);
TEXTURE2D(_HeightMap);			SAMPLER(sampler_HeightMap);
TEXTURE2D(_TessellationMap);	SAMPLER(sampler_TessellationMap);

#ifdef _SPECULAR_SETUP
#define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv)
#else
#define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv)
#endif

#ifdef _GEOMETRY_CUSTOM_ST
#define TESSELLATION_TRANSFORM_SOURCE _TessellationMap
#else
#define TESSELLATION_TRANSFORM_SOURCE _BaseMap
#endif

// ***** Structures field names keywords ******
// Are required to support the different fields each pass has without creating custom structs for each of them.

// Position
#if VERTEX_POSITION
#define HAS_POSITION 1
#define HAS_VERTEX_POSITION 1
#define POSITION_ID position

#elif VERTEX_POSITION_OS
#define HAS_POSITION 1
#define HAS_VERTEX_POSITION 1
#define POSITION_ID positionOS

#elif GEOMETRY_POSITION
#define HAS_POSITION 1
#define HAS_GEOMETRY_POSITION 1
#define POSITION_ID position
#endif

// Normal
#if VERTEX_NORMAL
#define HAS_NORMAL 1
#define HAS_VERTEX_NORMAL 1
#define NORMAL_ID normal

#elif VERTEX_NORMAL_OS
#define HAS_NORMAL 1
#define HAS_VERTEX_NORMAL 1
#define NORMAL_ID normalOS

#elif GEOMETRY_NORMAL
#define HAS_NORMAL 1
#define HAS_GEOMETRY_NORMAL 1
#define NORMAL_ID normal
#endif

// UV0
#if VERTEX_UV0
#define HAS_UV0 1
#define HAS_VERTEX_UV0 1
#define UV0_ID uv0

#elif VERTEX_TEXCOORD
#define HAS_UV0 1
#define HAS_VERTEX_UV0 1
#define UV0_ID texcoord

#elif GEOMETRY_UV0
#define HAS_UV0 1
#define HAS_GEOMETRY_UV0 1
#define UV0_ID uv0
#endif

// UV1
#if VERTEX_UV1
#define HAS_UV1 1
#define HAS_VERTEX_UV1 1
#define UV1_ID uv1

#elif VERTEX_LIGHTMAP_UV
#define HAS_UV1 1
#define HAS_VERTEX_UV1 1
#define UV1_ID lightmapUV

#elif GEOMETRY_UV1
#define HAS_UV1 1
#define HAS_GEOMETRY_UV1 1
#define UV1_ID uv1
#endif

// UV2
#if VERTEX_UV2
#define HAS_UV2 1
#define HAS_VERTEX_UV2 1
#define UV2_ID uv2

#elif defined(GEOMETRY_UV2) && defined(_GEOMETRY_UV_CHANNEL_2)
#define HAS_UV2 1
#define HAS_GEOMETRY_UV2 1
#define UV2_ID uv2
#endif

// UV3
#if VERTEX_UV3
#define HAS_UV3 1
#define HAS_VERTEX_UV3 1
#define UV3_ID uv3

#elif defined(GEOMETRY_UV3) && defined(_GEOMETRY_UV_CHANNEL_3)
#define HAS_UV3 1
#define HAS_GEOMETRY_UV3 1
#define UV3_ID uv3
#endif

// Tangent
#if VERTEX_TANGENT
#define HAS_TANGENT 1
#define HAS_VERTEX_TANGENT 1
#define TANGENT_ID tangent

#elif VERTEX_TANGENT_OS
#define HAS_TANGENT 1
#define HAS_VERTEX_TANGENT 1
#define TANGENT_ID tangentOS

#elif GEOMETRY_TANGENT_OS
#define HAS_TANGENT 1
#define HAS_GEOMETRY_TANGENT 1
#define TANGENT_ID tangent
#endif

// Vertex Color
#if defined(PIXEL_VERTEX_COLOR) && defined(_VERTEX_COLOR)
#define HAS_VERTEX_COLOR 1
#define VERTEX_COLOR_ID color
#endif

// UV Channel
#ifdef _GEOMETRY_CUSTOM_UV_CHANNEL
#ifdef _GEOMETRY_UV_CHANNEL_1
#define GEOMETRY_UV_CHANNEL UV1_ID
#elif _GEOMETRY_UV_CHANNEL_2
#define GEOMETRY_UV_CHANNEL UV2_ID
#elif _GEOMETRY_UV_CHANNEL_3
#define GEOMETRY_UV_CHANNEL UV3_ID
#else
#define GEOMETRY_UV_CHANNEL UV0_ID
#endif
#else
#define GEOMETRY_UV_CHANNEL UV0_ID
#endif

half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha)
{
    half4 specGloss;

#ifdef _METALLICSPECGLOSSMAP
    specGloss = SAMPLE_METALLICSPECULAR(uv);
#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    specGloss.a = albedoAlpha * _Smoothness;
#else
    specGloss.a *= _Smoothness;
#endif
#else // _METALLICSPECGLOSSMAP
#if _SPECULAR_SETUP
    specGloss.rgb = _SpecColor.rgb;
#else
    specGloss.rgb = _Metallic.rrr;
#endif

#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    specGloss.a = albedoAlpha * _Smoothness;
#else
    specGloss.a = _Smoothness;
#endif
#endif

    return specGloss;
}

half SampleOcclusion(float2 uv)
{
#ifdef _OCCLUSIONMAP
    // TODO: Controls things like these by exposing SHADER_QUALITY levels (low, medium, high)
#if defined(SHADER_API_GLES)
    return SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
#else
    half occ = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
    return LerpWhiteTo(occ, _OcclusionStrength);
#endif
#else
    return 1.0;
#endif
}

inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);

    half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;

#if HAS_VERTEX_COLOR
#if _VERTEX_COLOR_MULTIPLY
    outSurfaceData.albedo.rgb = outSurfaceData.albedo.rgb * (1 - _VertexColorBlending) + outSurfaceData.albedo.rgb * _VertexColor.rgb * _VertexColorBlending;
#elif _VERTEX_COLOR_ADDITIVE
    outSurfaceData.albedo.rgb = saturate(outSurfaceData.albedo.rgb + _VertexColor.rgb * _VertexColorBlending);
#elif _VERTEX_COLOR_SUBTRACTIVE
    outSurfaceData.albedo.rgb = saturate(outSurfaceData.albedo.rgb - _VertexColor.rgb * _VertexColorBlending);
#elif _VERTEX_COLOR_REPLACE
    outSurfaceData.albedo.rgb = lerp(outSurfaceData.albedo.rgb, _VertexColor.rgb, _VertexColorBlending);
#endif
#endif

#if _SPECULAR_SETUP
    outSurfaceData.metallic = 1.0h;
    outSurfaceData.specular = specGloss.rgb;
#else
    outSurfaceData.metallic = specGloss.r;
    outSurfaceData.specular = half3(0.0h, 0.0h, 0.0h);
#endif

    outSurfaceData.smoothness = specGloss.a;
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    outSurfaceData.occlusion = SampleOcclusion(uv);
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
}

half SampleTessellation(float2 uv)
{
#ifdef _TESSELLATIONMAP
    half sampledTessellation = max(SAMPLE_TEXTURE2D_LOD(_TessellationMap, sampler_TessellationMap, uv, 0).g, 0.01);
	return lerp(1.0, sampledTessellation, _TessellationScale);
#else
	return max(_TessellationScale, 0.01);
#endif
}

half SampleHeight(float2 uv)
{
#ifdef _HEIGHTMAP
#if defined(SHADER_API_GLES)
	return SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, uv, 0).b;
#else
#ifdef _HEIGHTMAP_BLUR_BOX
	return (SAMPLE_TEXTURE2D_LOD_BLUR_BOX(_HeightMap, sampler_HeightMap, uv, 0, _HeightBlurSize, _HeightBlurSamples).b + _HeightBase) * _HeightStrength;
#elif _HEIGHTMAP_BLUR_GAUSS
	return (SAMPLE_TEXTURE2D_LOD_BLUR_GAUSS(_HeightMap, sampler_HeightMap, uv, 0, _HeightBlurSize, _HeightBlurSamples, _HeightBlurGaussStandardDeviation * 0.1).b + _HeightBase) * _HeightStrength;
#else
	return (SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, uv, 0).b + _HeightBase) * _HeightStrength;
#endif
#endif
#else
	return 0.0;
#endif
}

half SampleHeightWorldCoordinates(float3 positionWS, float3 normalWS)
{
#ifndef _HEIGHTMAP
	return 0.0;
#else

#ifdef _HEIGHTMAP_BLUR_BOX
#define SAMPLE(uv) (SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, uv, 0).b + _HeightBase) * _HeightStrength
#elif _HEIGHTMAP_BLUR_GAUSS
#define SAMPLE(uv) (SAMPLE_TEXTURE2D_LOD_BLUR_BOX(_HeightMap, sampler_HeightMap, uv, 0, _HeightBlurSize, _HeightBlurSamples).b + _HeightBase) * _HeightStrength
#else
#define SAMPLE(uv) (SAMPLE_TEXTURE2D_LOD_BLUR_GAUSS(_HeightMap, sampler_HeightMap, uv, 0, _HeightBlurSize, _HeightBlurSamples, _HeightBlurGaussStandardDeviation * 0.1).b + _HeightBase) * _HeightStrength
#endif

	half height0 = SAMPLE(positionWS.xy);
    half height1 = SAMPLE(positionWS.zx);
    half height2 = SAMPLE(positionWS.zy);

    return lerp(lerp(height1, height0, normalWS.z), height2, normalWS.x);

#endif
}

#endif // UNIVERSAL_LIT_TESSELLATION_INPUT_INCLUDED
