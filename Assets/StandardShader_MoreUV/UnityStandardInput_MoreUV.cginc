// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef UNITY_STANDARD_INPUT_INCLUDED
#define UNITY_STANDARD_INPUT_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityPBSLighting.cginc" // TBD: remove
#include "UnityStandardUtils.cginc"

//---------------------------------------
// Directional lightmaps & Parallax require tangent space too
#if (_NORMALMAP || DIRLIGHTMAP_COMBINED || _PARALLAXMAP)
    #define _TANGENT_TO_WORLD 1
#endif

#if (_DETAIL_MULX2 || _DETAIL_MUL || _DETAIL_ADD || _DETAIL_LERP)
    #define _DETAIL 1
#endif

//---------------------------------------
half4       _Color;
half        _Cutoff;

sampler2D   _MainTex;

sampler2D   _SecondTex;

sampler2D   _ThirdTex;

sampler2D   _FourthTex;

sampler2D   _BumpMap0;

sampler2D   _BumpMap1;

sampler2D   _BumpMap2;

sampler2D   _BumpMap3;

sampler2D   _BumpMap4;

sampler2D   _DetailMask;

sampler2D   _MetallicGlossMap0;
half        _Metallic0;
float       _Glossiness0;
//float       _GlossMapScale0;

sampler2D   _MetallicGlossMap1;
half        _Metallic1;
float       _Glossiness1;
//float       _GlossMapScale1;

sampler2D   _MetallicGlossMap2;
half        _Metallic2;
float       _Glossiness2;
//float       _GlossMapScale2;

sampler2D   _MetallicGlossMap3;
half        _Metallic3;
float       _Glossiness3;
//float       _GlossMapScale3;

// Seeker. 貌似Unity内部绑定了 "_EmissionColor" 这个属性名. 不让改, 改了就报错
half4       _EmissionColor;
sampler2D   _EmissionMap;

half4       _EmissionColor1;
sampler2D   _EmissionMap1;

half4       _EmissionColor2;
sampler2D   _EmissionMap2;

half4       _EmissionColor3;
sampler2D   _EmissionMap3;

struct VertexInput_MoreUV
{
    float4 vertex   : POSITION;
    half3 normal    : NORMAL;
    float2 uv0      : TEXCOORD0;
    float2 uv1      : TEXCOORD1;
    float2 uv2      : TEXCOORD2;
	float2 uv3      : TEXCOORD3;
    half4 tangent   : TANGENT;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

//-------------------------------------------------------------------------------------
// Input functions

float4 TexCoords_01(VertexInput_MoreUV v)
{
    float4 texcoord;
    texcoord.xy = v.uv0;
    texcoord.zw = v.uv1;
    return texcoord;
}

float4 TexCoords_23(VertexInput_MoreUV v)
{
    float4 texcoord;
    texcoord.xy = v.uv2;
    texcoord.zw = v.uv3;
    return texcoord;
}

half DetailMask(float2 uv)
{
    return tex2D (_DetailMask, uv).a;
}

half3 Albedo(float2 uv, sampler2D tex)
{
    half3 albedo = _Color.rgb * tex2D (tex, uv).rgb;
    return albedo;
}

half Alpha(float2 uv, sampler2D tex)
{
    return tex2D(tex, uv).a * _Color.a;
}

half Occlusion(float2 uv, sampler2D occlusionMap, float occlusionStrength)
{
#if (SHADER_TARGET < 30)
    // SM20: instruction count limitation
    // SM20: simpler occlusion
    return tex2D(occlusionMap, uv).g;
#else
    half occ = tex2D(occlusionMap, uv).g;
    return LerpOneTo (occ, occlusionStrength);
#endif
}

half2 MetallicGloss(float2 uv, sampler2D metallicGlossMap)
{
    half2 mg = tex2D(metallicGlossMap, uv).ra;
    return mg;
}

half2 MetallicGloss(float2 uv, sampler2D metallicGlossMap, 
					float metallic, float glossiness, float glossMapScale)
{
    half2 mg;

#ifdef _METALLICGLOSSMAP
    mg = tex2D(metallicGlossMap, uv).ra;
    mg.g *= glossMapScale;
#else
    mg.r = metallic;
    mg.g = glossiness;
#endif
    return mg;
}

half3 Emission(float2 uv, sampler2D emissonMap, half4 emissionColor)
{
#ifndef _EMISSION
    return 0;
#else
    return tex2D(emissonMap, uv).rgb * emissionColor.rgb;
#endif
}

#ifdef _NORMALMAP
half3 NormalInTangentSpace(float2 uv, sampler2D bumpMap, float bumpScale)
{
    half3 normalTangent = UnpackScaleNormal(tex2D (bumpMap, uv), bumpScale);

    return normalTangent;
}
half3 NormalInTangentSpace(float2 uv, sampler2D bumpMap)
{
    half3 normalTangent = UnpackScaleNormal(tex2D (bumpMap, uv), 1);

    return normalTangent;
}
#endif

#endif // UNITY_STANDARD_INPUT_INCLUDED
