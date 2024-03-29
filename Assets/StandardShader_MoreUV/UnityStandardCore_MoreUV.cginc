// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef UNITY_STANDARD_CORE_INCLUDED
#define UNITY_STANDARD_CORE_INCLUDED

#include "UnityCG.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityStandardInput_MoreUV.cginc"
#include "UnityPBSLighting.cginc"
#include "UnityStandardUtils.cginc"
#include "UnityGBuffer.cginc"
#include "UnityStandardBRDF.cginc"

#include "AutoLight.cginc"


struct VertexOutputForwardBase_MoreUV
{
    UNITY_POSITION(pos);
    float4 tex01                          : TEXCOORD0;
	float4 tex23                          : Normal;
    float4 eyeVec                         : TEXCOORD1;    // eyeVec.xyz | fogCoord
    float4 tangentToWorldAndPackedData[3] : TEXCOORD2;    // [3x3:tangentToWorld | 1x3:viewDirForParallax or worldPos]
    half4 ambientOrLightmapUV             : TEXCOORD5;    // SH or Lightmap UV
    UNITY_LIGHTING_COORDS(6,7)

    // next ones would not fit into SM2.0 limits, but they are always for SM3.0+
#if UNITY_REQUIRE_FRAG_WORLDPOS && !UNITY_PACK_WORLDPOS_WITH_TANGENT
    float3 posWorld                     : TEXCOORD8;
#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

struct VertexOutputForwardAdd_MoreUV
{
    UNITY_POSITION(pos);
    float4 tex01                          : TEXCOORD0;
	float4 tex23                          : Normal;
    float4 eyeVec                       : TEXCOORD1;    // eyeVec.xyz | fogCoord
    float4 tangentToWorldAndLightDir[3] : TEXCOORD2;    // [3x3:tangentToWorld | 1x3:lightDir]
    float3 posWorld                     : TEXCOORD5;
    UNITY_LIGHTING_COORDS(6, 7)

    // next ones would not fit into SM2.0 limits, but they are always for SM3.0+
#if defined(_PARALLAXMAP)
    half3 viewDirForParallax            : TEXCOORD8;
#endif

    UNITY_VERTEX_OUTPUT_STEREO
};

//-------------------------------------------------------------------------------------
// counterpart for NormalizePerPixelNormal
// skips normalization per-vertex and expects normalization to happen per-pixel
half3 NormalizePerVertexNormal (float3 n) // takes float to avoid overflow
{
    #if (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
        return normalize(n);
    #else
        return n; // will normalize per-pixel instead
    #endif
}

float3 NormalizePerPixelNormal (float3 n)
{
    #if (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
        return n;
    #else
        return normalize((float3)n); // takes float to avoid overflow
    #endif
}

//-------------------------------------------------------------------------------------
UnityLight MainLight ()
{
    UnityLight l;

    l.color = _LightColor0.rgb;
    l.dir = _WorldSpaceLightPos0.xyz;
    return l;
}

UnityLight AdditiveLight (half3 lightDir, half atten)
{
    UnityLight l;

    l.color = _LightColor0.rgb;
    l.dir = lightDir;
    #ifndef USING_DIRECTIONAL_LIGHT
        l.dir = NormalizePerPixelNormal(l.dir);
    #endif

    // shadow the light
    l.color *= atten;
    return l;
}

UnityLight DummyLight ()
{
    UnityLight l;
    l.color = 0;
    l.dir = half3 (0,1,0);
    return l;
}

UnityIndirect ZeroIndirect ()
{
    UnityIndirect ind;
    ind.diffuse = 0;
    ind.specular = 0;
    return ind;
}

//-------------------------------------------------------------------------------------
// Common fragment setup

// deprecated
half3 WorldNormal(half4 tan2world[3])
{
    return normalize(tan2world[2].xyz);
}

// deprecated
#ifdef _TANGENT_TO_WORLD
    half3x3 ExtractTangentToWorldPerPixel(half4 tan2world[3])
    {
        half3 t = tan2world[0].xyz;
        half3 b = tan2world[1].xyz;
        half3 n = tan2world[2].xyz;

    #if UNITY_TANGENT_ORTHONORMALIZE
        n = NormalizePerPixelNormal(n);

        // ortho-normalize Tangent
        t = normalize (t - n * dot(t, n));

        // recalculate Binormal
        half3 newB = cross(n, t);
        b = newB * sign (dot (newB, b));
    #endif

        return half3x3(t, b, n);
    }
#else
    half3x3 ExtractTangentToWorldPerPixel(half4 tan2world[3])
    {
        return half3x3(0,0,0,0,0,0,0,0,0);
    }
#endif

float3 PerPixelWorldNormal(float4 i_tex01, float4 i_tex23, float4 tangentToWorld[3], half4 alphaForIntensity)
{
#ifdef _NORMALMAP

    half3 tangent = tangentToWorld[0].xyz;
    half3 binormal = tangentToWorld[1].xyz;
    half3 normal = tangentToWorld[2].xyz;

    #if UNITY_TANGENT_ORTHONORMALIZE
        normal = NormalizePerPixelNormal(normal);

        // ortho-normalize Tangent
        tangent = normalize (tangent - normal * dot(tangent, normal));

        // recalculate Binormal
        half3 newB = cross(normal, tangent);
        binormal = newB * sign (dot (newB, binormal));
    #endif

    half3 normalTangent0 = NormalInTangentSpace(i_tex01.xy, _BumpMap0);
    float3 normalWorld0 = NormalizePerPixelNormal(tangent * normalTangent0.x + binormal * normalTangent0.y + normal * normalTangent0.z);

    float3 normalWorld = normalize(normalWorld0);
	#ifndef _LAYER_4
	#ifdef _LAYER_2
    half3 normalTangent2 = NormalInTangentSpace(i_tex23.xy, _BumpMap2);
    float3 normalWorld2 = NormalizePerPixelNormal(tangent * normalTangent2.x + binormal * normalTangent2.y + normal * normalTangent2.z);
    normalWorld = normalize(lerp(normalWorld, normalWorld2, alphaForIntensity.z));
	#endif
	#ifdef _LAYER_3
    half3 normalTangent3 = NormalInTangentSpace(i_tex23.zw, _BumpMap3);
    float3 normalWorld3 = NormalizePerPixelNormal(tangent * normalTangent3.x + binormal * normalTangent3.y + normal * normalTangent3.z);
    normalWorld = normalize(lerp(normalWorld, normalWorld3, alphaForIntensity.w));
	#endif
	#else
	// UNDONE, 没有LAYER4的UV
    // half3 normalTangent4 = NormalInTangentSpace(i_tex23.xy, _BumpMap4);
    // float3 normalWorld4 = NormalizePerPixelNormal(tangent * normalTangent2.x + binormal * normalTangent2.y + normal * normalTangent2.z);
    // normalWorld = normalize(lerp(normalWorld, normalWorld2, alphaForIntensity.z));
	#endif

#else

    float3 normalWorld = normalize(tangentToWorld[2].xyz);

#endif
    return normalWorld;
}

#ifdef _PARALLAXMAP
    #define IN_VIEWDIR4PARALLAX(i) NormalizePerPixelNormal(half3(i.tangentToWorldAndPackedData[0].w,i.tangentToWorldAndPackedData[1].w,i.tangentToWorldAndPackedData[2].w))
    #define IN_VIEWDIR4PARALLAX_FWDADD(i) NormalizePerPixelNormal(i.viewDirForParallax.xyz)
#else
    #define IN_VIEWDIR4PARALLAX(i) half3(0,0,0)
    #define IN_VIEWDIR4PARALLAX_FWDADD(i) half3(0,0,0)
#endif

#if UNITY_REQUIRE_FRAG_WORLDPOS
    #if UNITY_PACK_WORLDPOS_WITH_TANGENT
        #define IN_WORLDPOS(i) half3(i.tangentToWorldAndPackedData[0].w,i.tangentToWorldAndPackedData[1].w,i.tangentToWorldAndPackedData[2].w)
    #else
        #define IN_WORLDPOS(i) i.posWorld
    #endif
    #define IN_WORLDPOS_FWDADD(i) i.posWorld
#else
    #define IN_WORLDPOS(i) half3(0,0,0)
    #define IN_WORLDPOS_FWDADD(i) half3(0,0,0)
#endif

#define IN_LIGHTDIR_FWDADD(i) half3(i.tangentToWorldAndLightDir[0].w, i.tangentToWorldAndLightDir[1].w, i.tangentToWorldAndLightDir[2].w)




struct FragmentCommonData
{
    half3 diffColor, specColor;
    // Note: smoothness & oneMinusReflectivity for optimization purposes, mostly for DX9 SM2.0 level.
    // Most of the math is being done on these (1-x) values, and that saves a few precious ALU slots.
    half oneMinusReflectivity, smoothness;
    float3 normalWorld;
    float3 eyeVec;
    half alpha;
    float3 posWorld;

#if UNITY_STANDARD_SIMPLE
    half3 reflUVW;
#endif

#if UNITY_STANDARD_SIMPLE
    half3 tangentSpaceNormal;
#endif

    // albedo贴图的alpha通道用于标记各UV通道上的贴图的强度
    half4 alphaForIntensity;
};


inline half3 DiffuseAndSpecularFromMetallic_MoreUV (half3 albedo, half metallic, out half3 specColor, out half oneMinusReflectivity)
{
    specColor = lerp (unity_ColorSpaceDielectricSpec.rgb, albedo, metallic);
    oneMinusReflectivity = OneMinusReflectivityFromMetallic(metallic);
    return albedo * oneMinusReflectivity;
}

inline FragmentCommonData MetallicSetup (float4 i_tex01, float4 i_tex23)
{
	half  alpha = 0, alpha0 = 0, alpha1 = 0, alpha2 = 0, alpha3 = 0;

    alpha0 = Alpha(i_tex01.xy, _MainTex);
	alpha += alpha0;
	
    #ifdef _LAYER_1
    alpha1 = Alpha(i_tex01.zw, _SecondTex);
	alpha += alpha1;
	#endif
	#ifdef _LAYER_2
    alpha2 = Alpha(i_tex23.xy, _ThirdTex);
	alpha += alpha2;
	#endif
	#ifdef _LAYER_3
    alpha3 = Alpha(i_tex23.zw, _FourthTex);
	alpha += alpha3;
	#endif

    #if defined(_ALPHATEST_ON)
        clip (alpha - _Cutoff);
    #endif

    // albedo 的混合
    half3 albedoColor = Albedo(i_tex01.xy, _MainTex);
    #ifdef _LAYER_1
    albedoColor = lerp(albedoColor, Albedo(i_tex01.zw, _SecondTex), alpha1);
    #endif
    #ifdef _LAYER_2
    albedoColor = lerp(albedoColor, Albedo(i_tex23.xy, _ThirdTex), alpha2);
    #endif
    #ifdef _LAYER_3
    albedoColor = lerp(albedoColor, Albedo(i_tex23.zw, _FourthTex), alpha3);
    #endif

    // metallic 和 smoothness 的混合
	half2 metallicGloss = 0;
	
    half2 metallicGloss0 = MetallicGloss(i_tex01.xy, _MetallicGlossMap0, _Metallic0, _Glossiness0, 1);
	
	metallicGloss = metallicGloss0;
	#ifdef _LAYER_2
	half2 metallicGloss2 = MetallicGloss(i_tex23.xy, _MetallicGlossMap2, _Metallic2, _Glossiness2, 1);
    metallicGloss = lerp(metallicGloss, metallicGloss2, alpha2);
    #endif
    #ifdef _LAYER_3
	half2 metallicGloss3 = MetallicGloss(i_tex23.zw, _MetallicGlossMap3, _Metallic3, _Glossiness3, 1);
    metallicGloss = lerp(metallicGloss, metallicGloss3, alpha3);
	#endif

    half metallic = metallicGloss.x;
    half smoothness = metallicGloss.y; // this is 1 minus the square root of real roughness m.

    half oneMinusReflectivity;
    half3 specColor;

    half3 diffColor = DiffuseAndSpecularFromMetallic_MoreUV (albedoColor, metallic
					, /*out*/ specColor, /*out*/ oneMinusReflectivity);

    FragmentCommonData o = (FragmentCommonData)0;
    o.diffColor = diffColor;
    o.specColor = specColor;
    o.oneMinusReflectivity = oneMinusReflectivity;
    o.smoothness = smoothness;
    o.alphaForIntensity = half4(alpha0, alpha1, alpha2, alpha3);
    return o;
}

// parallax transformed texcoord is used to sample occlusion
inline FragmentCommonData FragmentSetup (inout float4 i_tex01
									, inout float4 i_tex23
									, float3 i_eyeVec
									, half3 i_viewDirForParallax
									, float4 tangentToWorld[3]
									, float3 i_posWorld)
{
	// Seeker. 暂时屏蔽 ParallelMap
    //i_tex = Parallax(i_tex, i_viewDirForParallax);

    // Seeker. 要使用alpha通道的信息对不同层的albedo做混合, 所以把计算alpha的代码挪到MetallicSetup里了
    // half alpha = Alpha(i_tex01.xy, _MainTex)
	// 			+ Alpha(i_tex34.xy, _SecondTex)
	// 			+ Alpha(i_tex34.zw, _ThirdTex)
	// 			+ Alpha(i_tex5.xy, _FourthTex);

    // #if defined(_ALPHATEST_ON)
    //     clip (alpha - _Cutoff);
    // #endif

    FragmentCommonData o = MetallicSetup (i_tex01, i_tex23);
    o.normalWorld = PerPixelWorldNormal(i_tex01, i_tex23, tangentToWorld, o.alphaForIntensity);
    o.eyeVec = NormalizePerPixelNormal(i_eyeVec);
    o.posWorld = i_posWorld;

    // Seeker. 使用多UV
    // NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
    //o.diffColor = PreMultiplyAlpha (o.diffColor, alpha, o.oneMinusReflectivity, /*out*/ o.alpha);
    return o;
}

inline UnityGI FragmentGI (FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light, bool reflections)
{
    UnityGIInput d;
    d.light = light;
    d.worldPos = s.posWorld;
    d.worldViewDir = -s.eyeVec;
    d.atten = atten;
    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
        d.ambient = 0;
        d.lightmapUV = i_ambientOrLightmapUV;
    #else
        d.ambient = i_ambientOrLightmapUV.rgb;
        d.lightmapUV = 0;
    #endif

    d.probeHDR[0] = unity_SpecCube0_HDR;
    d.probeHDR[1] = unity_SpecCube1_HDR;
    #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
      d.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
    #endif
    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
      d.boxMax[0] = unity_SpecCube0_BoxMax;
      d.probePosition[0] = unity_SpecCube0_ProbePosition;
      d.boxMax[1] = unity_SpecCube1_BoxMax;
      d.boxMin[1] = unity_SpecCube1_BoxMin;
      d.probePosition[1] = unity_SpecCube1_ProbePosition;
    #endif

    if(reflections)
    {
        Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.smoothness, -s.eyeVec, s.normalWorld, s.specColor);
        // Replace the reflUVW if it has been compute in Vertex shader. Note: the compiler will optimize the calcul in UnityGlossyEnvironmentSetup itself
        #if UNITY_STANDARD_SIMPLE
            g.reflUVW = s.reflUVW;
        #endif

        return UnityGlobalIllumination (d, occlusion, s.normalWorld, g);
    }
    else
    {
        return UnityGlobalIllumination (d, occlusion, s.normalWorld);
    }
}

inline UnityGI FragmentGI (FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light)
{
    return FragmentGI(s, occlusion, i_ambientOrLightmapUV, atten, light, true);
}


//-------------------------------------------------------------------------------------
half4 OutputForward (half4 output, half alphaFromSurface)
{
    #if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
        output.a = alphaFromSurface;
    #else
        UNITY_OPAQUE_ALPHA(output.a);
    #endif
    return output;
}

inline half4 VertexGIForward(VertexInput_MoreUV v, float3 posWorld, half3 normalWorld)
{
    half4 ambientOrLightmapUV = 0;
    // Static lightmaps
    #ifdef LIGHTMAP_ON
        ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
        ambientOrLightmapUV.zw = 0;
    // Sample light probe for Dynamic objects only (no static or dynamic lightmaps)
    #elif UNITY_SHOULD_SAMPLE_SH
        #ifdef VERTEXLIGHT_ON
            // Approximated illumination from non-important point lights
            ambientOrLightmapUV.rgb = Shade4PointLights (
                unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                unity_4LightAtten0, posWorld, normalWorld);
        #endif

        ambientOrLightmapUV.rgb = ShadeSHPerVertex (normalWorld, ambientOrLightmapUV.rgb);
    #endif

    #ifdef DYNAMICLIGHTMAP_ON
        ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    #endif

    return ambientOrLightmapUV;
}


VertexOutputForwardBase_MoreUV vertForwardBase_MoreUV (VertexInput_MoreUV v)
{
    UNITY_SETUP_INSTANCE_ID(v);
    VertexOutputForwardBase_MoreUV o;
    UNITY_INITIALIZE_OUTPUT(VertexOutputForwardBase_MoreUV, o);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
    #if UNITY_REQUIRE_FRAG_WORLDPOS
        #if UNITY_PACK_WORLDPOS_WITH_TANGENT
            o.tangentToWorldAndPackedData[0].w = posWorld.x;
            o.tangentToWorldAndPackedData[1].w = posWorld.y;
            o.tangentToWorldAndPackedData[2].w = posWorld.z;
        #else
            o.posWorld = posWorld.xyz;
        #endif
    #endif
    o.pos = UnityObjectToClipPos(v.vertex);

    o.tex01 = TexCoords_01(v);
	o.tex23 = TexCoords_23(v);
	
    o.eyeVec.xyz = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
    float3 normalWorld = UnityObjectToWorldNormal(v.normal);
    #ifdef _TANGENT_TO_WORLD
        float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

        float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
        o.tangentToWorldAndPackedData[0].xyz = tangentToWorld[0];
        o.tangentToWorldAndPackedData[1].xyz = tangentToWorld[1];
        o.tangentToWorldAndPackedData[2].xyz = tangentToWorld[2];
    #else
        o.tangentToWorldAndPackedData[0].xyz = 0;
        o.tangentToWorldAndPackedData[1].xyz = 0;
        o.tangentToWorldAndPackedData[2].xyz = normalWorld;
    #endif

    //We need this for shadow receving
    UNITY_TRANSFER_LIGHTING(o, v.uv1);

    o.ambientOrLightmapUV = VertexGIForward(v, posWorld, normalWorld);

    #ifdef _PARALLAXMAP
        TANGENT_SPACE_ROTATION;
        half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
        o.tangentToWorldAndPackedData[0].w = viewDirForParallax.x;
        o.tangentToWorldAndPackedData[1].w = viewDirForParallax.y;
        o.tangentToWorldAndPackedData[2].w = viewDirForParallax.z;
    #endif

    UNITY_TRANSFER_FOG_COMBINED_WITH_EYE_VEC(o,o.pos);
    return o;
}

half3 AllEmission(float2 uv0, float2 uv1, float2 uv2, float2 uv3, half4 alpha)
{
    half3 emissionRGB = float3(0, 0, 0);
    #ifdef _LAYER_1
    emissionRGB += lerp(emissionRGB, Emission(uv1, _EmissionMap1, _EmissionColor1), alpha.y);
    #endif
    return emissionRGB;
}

half4 fragForwardBaseInternal_MoreUV (VertexOutputForwardBase_MoreUV i)
{
    UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

    //FRAGMENT_SETUP(s)
	FragmentCommonData s = FragmentSetup(i.tex01
	, i.tex23
	, i.eyeVec.xyz
	, IN_VIEWDIR4PARALLAX(i)
	, i.tangentToWorldAndPackedData
	, IN_WORLDPOS(i));

    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    UnityLight mainLight = MainLight ();
    UNITY_LIGHT_ATTENUATION(atten, i, s.posWorld);

	// Seeker. 战舰不要AO. By 陆广平
    // half occlusion = Occlusion(i.tex01.xy, _OcclusionMap0, _OcclusionStrength0)
	// 				+ Occlusion(i.tex34.xy, _OcclusionMap1, _OcclusionStrength1)
	// 				+ Occlusion(i.tex34.zw, _OcclusionMap2, _OcclusionStrength2)
	// 				+ Occlusion(i.tex5.xy, _OcclusionMap3, _OcclusionStrength3);

    half occlusion = 1;

    UnityGI gi = FragmentGI (s, occlusion, i.ambientOrLightmapUV, atten, mainLight);

    half4 c = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect);
    c.rgb += AllEmission(i.tex01.xy, i.tex01.zw, i.tex23.xy, i.tex23.zw, s.alphaForIntensity);

    UNITY_EXTRACT_FOG_FROM_EYE_VEC(i);
    UNITY_APPLY_FOG(_unity_fogCoord, c.rgb);
    return OutputForward (c, s.alpha);
}

half4 fragForwardBase (VertexOutputForwardBase_MoreUV i) : SV_Target   // backward compatibility (this used to be the fragment entry function)
{
    return fragForwardBaseInternal_MoreUV(i);
}

// ------------------------------------------------------------------
//  Additive forward pass (one light per pass)


VertexOutputForwardAdd_MoreUV vertForwardAdd_MoreUV (VertexInput_MoreUV v)
{
    UNITY_SETUP_INSTANCE_ID(v);
    VertexOutputForwardAdd_MoreUV o;
    UNITY_INITIALIZE_OUTPUT(VertexOutputForwardAdd_MoreUV, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
    o.pos = UnityObjectToClipPos(v.vertex);

    o.tex01 = TexCoords_01(v);
	o.tex23 = TexCoords_23(v);
	
    o.eyeVec.xyz = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
    o.posWorld = posWorld.xyz;
    float3 normalWorld = UnityObjectToWorldNormal(v.normal);
    #ifdef _TANGENT_TO_WORLD
        float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

        float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
        o.tangentToWorldAndLightDir[0].xyz = tangentToWorld[0];
        o.tangentToWorldAndLightDir[1].xyz = tangentToWorld[1];
        o.tangentToWorldAndLightDir[2].xyz = tangentToWorld[2];
    #else
        o.tangentToWorldAndLightDir[0].xyz = 0;
        o.tangentToWorldAndLightDir[1].xyz = 0;
        o.tangentToWorldAndLightDir[2].xyz = normalWorld;
    #endif
    //We need this for shadow receiving and lighting
    UNITY_TRANSFER_LIGHTING(o, v.uv1);

    float3 lightDir = _WorldSpaceLightPos0.xyz - posWorld.xyz * _WorldSpaceLightPos0.w;
    #ifndef USING_DIRECTIONAL_LIGHT
        lightDir = NormalizePerVertexNormal(lightDir);
    #endif
    o.tangentToWorldAndLightDir[0].w = lightDir.x;
    o.tangentToWorldAndLightDir[1].w = lightDir.y;
    o.tangentToWorldAndLightDir[2].w = lightDir.z;

    #ifdef _PARALLAXMAP
        TANGENT_SPACE_ROTATION;
        o.viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
    #endif

    UNITY_TRANSFER_FOG_COMBINED_WITH_EYE_VEC(o, o.pos);
    return o;
}

half4 fragForwardAddInternal_MoreUV (VertexOutputForwardAdd_MoreUV i)
{
    UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    //FRAGMENT_SETUP_FWDADD(s)
	FragmentCommonData s = 
    FragmentSetup(i.tex01
	, i.tex23
	, i.eyeVec.xyz
	, IN_VIEWDIR4PARALLAX_FWDADD(i)
	, i.tangentToWorldAndLightDir
	, IN_WORLDPOS_FWDADD(i));

    UNITY_LIGHT_ATTENUATION(atten, i, s.posWorld)
    UnityLight light = AdditiveLight (IN_LIGHTDIR_FWDADD(i), atten);
    UnityIndirect noIndirect = ZeroIndirect ();

    half4 c = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, light, noIndirect);

    UNITY_EXTRACT_FOG_FROM_EYE_VEC(i);
    UNITY_APPLY_FOG_COLOR(_unity_fogCoord, c.rgb, half4(0,0,0,0)); // fog towards black in additive pass
    return OutputForward (c, s.alpha);
}

half4 fragForwardAdd_MoreUV (VertexOutputForwardAdd_MoreUV i) : SV_Target     // backward compatibility (this used to be the fragment entry function)
{
    return fragForwardAddInternal_MoreUV(i);
}


//
// Old FragmentGI signature. Kept only for backward compatibility and will be removed soon
//

inline UnityGI FragmentGI(
    float3 posWorld,
    half occlusion, half4 i_ambientOrLightmapUV, half atten, half smoothness, half3 normalWorld, half3 eyeVec,
    UnityLight light,
    bool reflections)
{
    // we init only fields actually used
    FragmentCommonData s = (FragmentCommonData)0;
    s.smoothness = smoothness;
    s.normalWorld = normalWorld;
    s.eyeVec = eyeVec;
    s.posWorld = posWorld;
    return FragmentGI(s, occlusion, i_ambientOrLightmapUV, atten, light, reflections);
}
inline UnityGI FragmentGI (
    float3 posWorld,
    half occlusion, half4 i_ambientOrLightmapUV, half atten, half smoothness, half3 normalWorld, half3 eyeVec,
    UnityLight light)
{
    return FragmentGI (posWorld, occlusion, i_ambientOrLightmapUV, atten, smoothness, normalWorld, eyeVec, light, true);
}

#endif // UNITY_STANDARD_CORE_INCLUDED
