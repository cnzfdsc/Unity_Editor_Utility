
Shader "Standard_MoreUV"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}
        _SecondTex("Second Albedo", 2D) = "grey" {}
        _ThirdTex("Third Albedo", 2D) = "grey" {}
        _FourthTex("Fourth Albedo", 2D) = "grey" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Glossiness0("Smoothness", Range(0.0, 1.0)) = 0.5
        //_GlossMapScale0("Smoothness Scale", Range(0.0, 1.0)) = 1.0

        _Glossiness1("Smoothness", Range(0.0, 1.0)) = 0.5
        //_GlossMapScale1("Smoothness Scale", Range(0.0, 1.0)) = 1.0
		
        _Glossiness2("Smoothness", Range(0.0, 1.0)) = 0.5
        //_GlossMapScale2("Smoothness Scale", Range(0.0, 1.0)) = 1.0
		
        _Glossiness3("Smoothness", Range(0.0, 1.0)) = 0.5
        //_GlossMapScale3("Smoothness Scale", Range(0.0, 1.0)) = 1.0

        [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic0("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap0("Metallic", 2D) = "white" {}

        [Gamma] _Metallic1("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap1("Metallic", 2D) = "white" {}
		
        [Gamma] _Metallic2("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap2("Metallic", 2D) = "white" {}
		
        [Gamma] _Metallic3("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap3("Metallic", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BumpScale0("Scale", Float) = 1.0
        _BumpMap0("Normal Map", 2D) = "bump" {}

        _BumpScale1("Scale", Float) = 1.0
        _BumpMap1("Normal Map", 2D) = "bump" {}

        _BumpScale2("Scale", Float) = 1.0
        _BumpMap2("Normal Map", 2D) = "bump" {}

        _BumpScale3("Scale", Float) = 1.0
        _BumpMap3("Normal Map", 2D) = "bump" {}

        _OcclusionStrength0("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap0("Occlusion", 2D) = "white" {}

        _OcclusionStrength1("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap1("Occlusion", 2D) = "white" {}

        _OcclusionStrength2("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap2("Occlusion", 2D) = "white" {}

        _OcclusionStrength3("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap3("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        _EmissionColor1("Color", Color) = (0,0,0)
        _EmissionMap1("Emission", 2D) = "white" {}

        _EmissionColor2("Color", Color) = (0,0,0)
        _EmissionMap2("Emission", 2D) = "white" {}

        _EmissionColor3("Color", Color) = (0,0,0)
        _EmissionMap3("Emission", 2D) = "white" {}

        _DetailMask("Detail Mask", 2D) = "white" {}


        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 300


        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _EMISSION_0 _EMISSION_1 _EMISSION_2 _EMISSION_3
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature _PARALLAXMAP

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertBase
            #pragma fragment fragBase
            #include "UnityStandardCoreForward_MoreUV.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _PARALLAXMAP

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertAdd
            #pragma fragment fragAdd
            #include "UnityStandardCoreForward_MoreUV.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _PARALLAXMAP
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"

            ENDCG
        }
    }

    FallBack "VertexLit"
    CustomEditor "StandardShaderGUI_MoreUV"
}