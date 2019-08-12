// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

using System;
using UnityEngine;

namespace UnityEditor
{
    internal class StandardShaderGUI_MoreUV : ShaderGUI
    {
        private enum WorkflowMode
        {
            Specular,
            Metallic,
            Dielectric
        }

        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
        }

        public enum SmoothnessMapChannel
        {
            SpecularMetallicAlpha,
            AlbedoAlpha,
        }

        private static class Styles
        {
            public static GUIContent uvSetLabel = EditorGUIUtility.TrTextContent("UV Set");

            public static GUIContent albedoText0 = EditorGUIUtility.TrTextContent("Albedo 1", "Albedo (RGB) and Transparency (A)");
			public static GUIContent albedoText1 = EditorGUIUtility.TrTextContent("Albedo 2", "Albedo (RGB) and Transparency (A)");
			public static GUIContent albedoText2 = EditorGUIUtility.TrTextContent("Albedo 3", "Albedo (RGB) and Transparency (A)");
			public static GUIContent albedoText3 = EditorGUIUtility.TrTextContent("Albedo 4", "Albedo (RGB) and Transparency (A)");

			public static GUIContent alphaCutoffText = EditorGUIUtility.TrTextContent("Alpha Cutoff", "Threshold for alpha cutoff");
            //public static GUIContent specularMapText = EditorGUIUtility.TrTextContent("Specular", "Specular (RGB) and Smoothness (A)");

            public static GUIContent metallicMapText0 = EditorGUIUtility.TrTextContent("Metallic 1", "Metallic (R) and Smoothness (A)");
			public static GUIContent metallicMapText1 = EditorGUIUtility.TrTextContent("Metallic 2", "Metallic (R) and Smoothness (A)");
			public static GUIContent metallicMapText2 = EditorGUIUtility.TrTextContent("Metallic 3", "Metallic (R) and Smoothness (A)");
			public static GUIContent metallicMapText3 = EditorGUIUtility.TrTextContent("Metallic 4", "Metallic (R) and Smoothness (A)");

			public static GUIContent smoothnessText0 = EditorGUIUtility.TrTextContent("Smoothness 1", "Smoothness value");
			public static GUIContent smoothnessText1 = EditorGUIUtility.TrTextContent("Smoothness 2", "Smoothness value");
			public static GUIContent smoothnessText2 = EditorGUIUtility.TrTextContent("Smoothness 3", "Smoothness value");
			public static GUIContent smoothnessText3 = EditorGUIUtility.TrTextContent("Smoothness 4", "Smoothness value");

			public static GUIContent smoothnessScaleText = EditorGUIUtility.TrTextContent("Smoothness", "Smoothness scale factor");
            public static GUIContent smoothnessMapChannelText = EditorGUIUtility.TrTextContent("Source", "Smoothness texture and channel");
            public static GUIContent highlightsText = EditorGUIUtility.TrTextContent("Specular Highlights", "Specular Highlights");
            public static GUIContent reflectionsText = EditorGUIUtility.TrTextContent("Reflections", "Glossy Reflections");
            public static GUIContent normalMapText0 = EditorGUIUtility.TrTextContent("Normal Map 1", "Normal Map");
			public static GUIContent normalMapText1 = EditorGUIUtility.TrTextContent("Normal Map 2", "Normal Map");
			public static GUIContent normalMapText2 = EditorGUIUtility.TrTextContent("Normal Map 3", "Normal Map");
			public static GUIContent normalMapText3 = EditorGUIUtility.TrTextContent("Normal Map 4", "Normal Map");
			//public static GUIContent heightMapText = EditorGUIUtility.TrTextContent("Height Map", "Height Map (G)");
			//public static GUIContent occlusionText0 = EditorGUIUtility.TrTextContent("Occlusion0", "Occlusion (G)");
			//public static GUIContent occlusionText1 = EditorGUIUtility.TrTextContent("Occlusion1", "Occlusion (G)");
			//public static GUIContent occlusionText2 = EditorGUIUtility.TrTextContent("Occlusion2", "Occlusion (G)");
			//public static GUIContent occlusionText3 = EditorGUIUtility.TrTextContent("Occlusion3", "Occlusion (G)");
			public static GUIContent emissionText0 = EditorGUIUtility.TrTextContent("Color 1", "Emission (RGB)");
			public static GUIContent emissionText1 = EditorGUIUtility.TrTextContent("Color 2", "Emission (RGB)");
			public static GUIContent emissionText2 = EditorGUIUtility.TrTextContent("Color 3", "Emission (RGB)");
			public static GUIContent emissionText3 = EditorGUIUtility.TrTextContent("Color 4", "Emission (RGB)");
			//public static GUIContent detailMaskText = EditorGUIUtility.TrTextContent("Detail Mask", "Mask for Secondary Maps (A)");
			//public static GUIContent detailAlbedoText = EditorGUIUtility.TrTextContent("Detail Albedo x2", "Albedo (RGB) multiplied by 2");
			//public static GUIContent detailNormalMapText = EditorGUIUtility.TrTextContent("Normal Map", "Normal Map");

			public static string primaryMapsText = "Main Maps";
            public static string secondaryMapsText = "Secondary Maps";
			public static string thirdMapsText = "Third Maps";
			public static string fourthMapsText = "Fourth Maps";
			public static string forwardText = "Forward Rendering Options";
            public static string renderingMode = "Rendering Mode";
            public static string advancedText = "Advanced Options";
            public static readonly string[] blendNames = Enum.GetNames(typeof(BlendMode));
        }

        MaterialProperty blendMode = null;
        MaterialProperty albedoMap0 = null;
		MaterialProperty albedoMap1 = null;
		MaterialProperty albedoMap2 = null;
		MaterialProperty albedoMap3 = null;
		MaterialProperty albedoColor = null;
        MaterialProperty alphaCutoff = null;
        //MaterialProperty specularMap = null;
        //MaterialProperty specularColor = null;

        MaterialProperty metallicMap0 = null;
        MaterialProperty metallic0 = null;
        MaterialProperty smoothness0 = null;
        //MaterialProperty smoothnessScale0 = null;

		MaterialProperty metallicMap1 = null;
		MaterialProperty metallic1 = null;
		MaterialProperty smoothness1 = null;
		//MaterialProperty smoothnessScale1 = null;

		MaterialProperty metallicMap2 = null;
		MaterialProperty metallic2 = null;
		MaterialProperty smoothness2 = null;
		//MaterialProperty smoothnessScale2 = null;

		MaterialProperty metallicMap3 = null;
		MaterialProperty metallic3 = null;
		MaterialProperty smoothness3 = null;
		//MaterialProperty smoothnessScale3 = null;

		MaterialProperty smoothnessMapChannel = null;
        MaterialProperty highlights = null;
        MaterialProperty reflections = null;

        MaterialProperty bumpScale0 = null;
        MaterialProperty bumpMap0 = null;
		MaterialProperty bumpScale1 = null;
		MaterialProperty bumpMap1 = null;
		MaterialProperty bumpScale2 = null;
		MaterialProperty bumpMap2 = null;
		MaterialProperty bumpScale3 = null;
		MaterialProperty bumpMap3 = null;

		//MaterialProperty occlusionStrength0 = null;
  //      MaterialProperty occlusionMap0 = null;

		//MaterialProperty occlusionStrength1 = null;
		//MaterialProperty occlusionMap1 = null;

		//MaterialProperty occlusionStrength2 = null;
		//MaterialProperty occlusionMap2 = null;

		//MaterialProperty occlusionStrength3 = null;
		//MaterialProperty occlusionMap3 = null;

		//MaterialProperty heigtMapScale = null;
		//MaterialProperty heightMap = null;

		MaterialProperty emissionColorForRendering0 = null;
        MaterialProperty emissionMap0 = null;

		MaterialProperty emissionColorForRendering1 = null;
		MaterialProperty emissionMap1 = null;

		MaterialProperty emissionColorForRendering2 = null;
		MaterialProperty emissionMap2 = null;

		MaterialProperty emissionColorForRendering3 = null;
		MaterialProperty emissionMap3 = null;

		//MaterialProperty detailMask = null;
		//MaterialProperty detailAlbedoMap = null;
		//MaterialProperty detailNormalMapScale = null;
		//MaterialProperty detailNormalMap = null;
		//MaterialProperty uvSetSecondary = null;

        MaterialEditor m_MaterialEditor;
		//WorkflowMode m_WorkflowMode = WorkflowMode.Specular;
		WorkflowMode m_WorkflowMode = WorkflowMode.Metallic;

		bool m_FirstTimeApply = true;

        public void FindProperties(MaterialProperty[] props)
        {
            blendMode = FindProperty("_Mode", props);
            albedoMap0 = FindProperty("_MainTex", props);
			albedoMap1 = FindProperty("_SecondTex", props);
			albedoMap2 = FindProperty("_ThirdTex", props);
			albedoMap3 = FindProperty("_FourthTex", props);

			albedoColor = FindProperty("_Color", props);
            alphaCutoff = FindProperty("_Cutoff", props);
            //specularMap = FindProperty("_SpecGlossMap", props, false);
            //specularColor = FindProperty("_SpecColor", props, false);

            metallicMap0 = FindProperty("_MetallicGlossMap0", props, false);
            metallic0 = FindProperty("_Metallic0", props, false);
            //if (specularMap != null && specularColor != null)
            //    m_WorkflowMode = WorkflowMode.Specular;
            //else if (metallicMap0 != null && metallic0 != null)
                m_WorkflowMode = WorkflowMode.Metallic;
            //else
            //    m_WorkflowMode = WorkflowMode.Dielectric;
            smoothness0 = FindProperty("_Glossiness0", props);
            //smoothnessScale0 = FindProperty("_GlossMapScale0", props, false);

			metallicMap1 = FindProperty("_MetallicGlossMap1", props, false);
			metallic1 = FindProperty("_Metallic1", props, false);
			smoothness1 = FindProperty("_Glossiness1", props);
			//smoothnessScale1 = FindProperty("_GlossMapScale1", props, false);

			metallicMap2 = FindProperty("_MetallicGlossMap2", props, false);
			metallic2 = FindProperty("_Metallic2", props, false);
			smoothness2 = FindProperty("_Glossiness2", props);
			//smoothnessScale2 = FindProperty("_GlossMapScale2", props, false);

			metallicMap3 = FindProperty("_MetallicGlossMap3", props, false);
			metallic3 = FindProperty("_Metallic3", props, false);
			smoothness3 = FindProperty("_Glossiness3", props);
			//smoothnessScale3 = FindProperty("_GlossMapScale3", props, false);

			smoothnessMapChannel = FindProperty("_SmoothnessTextureChannel", props, false);

            highlights = FindProperty("_SpecularHighlights", props, false);
            reflections = FindProperty("_GlossyReflections", props, false);
            bumpScale0 = FindProperty("_BumpScale0", props);
            bumpMap0 = FindProperty("_BumpMap0", props);
			bumpScale1 = FindProperty("_BumpScale1", props);
			bumpMap1 = FindProperty("_BumpMap1", props);
			bumpScale2 = FindProperty("_BumpScale2", props);
			bumpMap2 = FindProperty("_BumpMap2", props);
			bumpScale3 = FindProperty("_BumpScale3", props);
			bumpMap3 = FindProperty("_BumpMap3", props);
			//heigtMapScale = FindProperty("_Parallax", props);
			//heightMap = FindProperty("_ParallaxMap", props);

			//occlusionStrength0 = FindProperty("_OcclusionStrength0", props);
   //         occlusionMap0 = FindProperty("_OcclusionMap0", props);

			//occlusionStrength1 = FindProperty("_OcclusionStrength1", props);
			//occlusionMap1 = FindProperty("_OcclusionMap1", props);

			//occlusionStrength2 = FindProperty("_OcclusionStrength2", props);
			//occlusionMap2 = FindProperty("_OcclusionMap2", props);

			//occlusionStrength3 = FindProperty("_OcclusionStrength3", props);
			//occlusionMap3 = FindProperty("_OcclusionMap3", props);

			emissionColorForRendering0 = FindProperty("_EmissionColor", props);
            emissionMap0 = FindProperty("_EmissionMap", props);

			emissionColorForRendering1 = FindProperty("_EmissionColor1", props);
			emissionMap1 = FindProperty("_EmissionMap1", props);

			emissionColorForRendering2 = FindProperty("_EmissionColor2", props);
			emissionMap2 = FindProperty("_EmissionMap2", props);

			emissionColorForRendering3 = FindProperty("_EmissionColor3", props);
			emissionMap3 = FindProperty("_EmissionMap3", props);


			//detailMask = FindProperty("_DetailMask", props);
			//detailAlbedoMap = FindProperty("_DetailAlbedoMap", props);
			//detailNormalMapScale = FindProperty("_DetailNormalMapScale", props);
			//detailNormalMap = FindProperty("_DetailNormalMap", props);
			//uvSetSecondary = FindProperty("_UVSec", props);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
            m_MaterialEditor = materialEditor;
            Material material = materialEditor.target as Material;

            // Make sure that needed setup (ie keywords/renderqueue) are set up if we're switching some existing
            // material to a standard shader.
            // Do this before any GUI code has been issued to prevent layout issues in subsequent GUILayout statements (case 780071)
            if (m_FirstTimeApply)
            {
                MaterialChanged(material, m_WorkflowMode);
                m_FirstTimeApply = false;
            }

            ShaderPropertiesGUI(material);
        }

        public void ShaderPropertiesGUI(Material material)
        {
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            {
                BlendModePopup();

                // Primary properties
                //GUILayout.Label(Styles.primaryMapsText, EditorStyles.boldLabel);
				for (int i = 0; i < 4; i++)
				{
					DoLabelTitle(i);
					DoAlbedoArea(material, i);

					// Seeker. 广平不要贴图的Transform
					//EditorGUI.BeginChangeCheck();
					//m_MaterialEditor.TextureScaleOffsetProperty(albedoMap0);
					//m_MaterialEditor.TextureScaleOffsetProperty(albedoMap1);
					//m_MaterialEditor.TextureScaleOffsetProperty(albedoMap2);
					//m_MaterialEditor.TextureScaleOffsetProperty(albedoMap3);
					//if (EditorGUI.EndChangeCheck())
					//{
					//	// UNDONE
					//	emissionMap0.textureScaleAndOffset = albedoMap0.textureScaleAndOffset; // Apply the main texture scale and offset to the emission texture as well, for Enlighten's sake 
					//}

					EditorGUILayout.Space();
					DoSpecularMetallicArea(i);
					EditorGUILayout.Space();
					DoNormalArea(i);
					EditorGUILayout.Space();
				}
				//m_MaterialEditor.TexturePropertySingleLine(Styles.heightMapText, heightMap, heightMap.textureValue != null ? heigtMapScale : null);
				//m_MaterialEditor.TexturePropertySingleLine(Styles.occlusionText0, occlusionMap0, occlusionMap0.textureValue != null ? occlusionStrength0 : null);
				//m_MaterialEditor.TexturePropertySingleLine(Styles.occlusionText1, occlusionMap1, occlusionMap1.textureValue != null ? occlusionStrength1 : null);
				//m_MaterialEditor.TexturePropertySingleLine(Styles.occlusionText2, occlusionMap2, occlusionMap2.textureValue != null ? occlusionStrength2 : null);
				//m_MaterialEditor.TexturePropertySingleLine(Styles.occlusionText3, occlusionMap3, occlusionMap3.textureValue != null ? occlusionStrength3 : null);
				//m_MaterialEditor.TexturePropertySingleLine(Styles.detailMaskText, detailMask);
				DoEmissionArea(material);
				EditorGUILayout.Space();

                // Secondary properties
                //GUILayout.Label(Styles.secondaryMapsText, EditorStyles.boldLabel);
                //m_MaterialEditor.TexturePropertySingleLine(Styles.detailAlbedoText, detailAlbedoMap);
                //m_MaterialEditor.TexturePropertySingleLine(Styles.detailNormalMapText, detailNormalMap, detailNormalMapScale);
                //m_MaterialEditor.TextureScaleOffsetProperty(detailAlbedoMap);
                //m_MaterialEditor.ShaderProperty(uvSetSecondary, Styles.uvSetLabel.text);

                // Third properties
                GUILayout.Label(Styles.forwardText, EditorStyles.boldLabel);
                if (highlights != null)
                    m_MaterialEditor.ShaderProperty(highlights, Styles.highlightsText);
                if (reflections != null)
                    m_MaterialEditor.ShaderProperty(reflections, Styles.reflectionsText);
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendMode.targets)
                    MaterialChanged((Material)obj, m_WorkflowMode);
            }

            EditorGUILayout.Space();

            // NB renderqueue editor is not shown on purpose: we want to override it based on blend mode
            GUILayout.Label(Styles.advancedText, EditorStyles.boldLabel);
            m_MaterialEditor.EnableInstancingField();
            m_MaterialEditor.DoubleSidedGIField();
        }

        internal void DetermineWorkflow(MaterialProperty[] props)
        {
            //if (FindProperty("_SpecGlossMap", props, false) != null && FindProperty("_SpecColor", props, false) != null)
            //    m_WorkflowMode = WorkflowMode.Specular;
            //else if (FindProperty("_MetallicGlossMap", props, false) != null && FindProperty("_Metallic", props, false) != null)
                m_WorkflowMode = WorkflowMode.Metallic;
            //else
            //    m_WorkflowMode = WorkflowMode.Dielectric;
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor0", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));
                return;
            }

            BlendMode blendMode = BlendMode.Opaque;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                blendMode = BlendMode.Cutout;
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                blendMode = BlendMode.Fade;
            }
            material.SetFloat("_Mode", (float)blendMode);

            DetermineWorkflow(MaterialEditor.GetMaterialProperties(new Material[] { material }));
            MaterialChanged(material, m_WorkflowMode);
        }

        void BlendModePopup()
        {
            EditorGUI.showMixedValue = blendMode.hasMixedValue;
            var mode = (BlendMode)blendMode.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (BlendMode)EditorGUILayout.Popup(Styles.renderingMode, (int)mode, Styles.blendNames);
            if (EditorGUI.EndChangeCheck())
            {
                m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
                blendMode.floatValue = (float)mode;
            }

            EditorGUI.showMixedValue = false;
        }

        void DoNormalArea(int index)
        {
			if (index == 0)
			{
				m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText0, bumpMap0, bumpMap0.textureValue != null ? bumpScale0 : null);
				if (bumpScale0.floatValue != 1 && UnityEditorInternal.InternalEditorUtility.IsMobilePlatform(EditorUserBuildSettings.activeBuildTarget))
					if (m_MaterialEditor.HelpBoxWithButton(
							EditorGUIUtility.TrTextContent("Bump scale is not supported on mobile platforms"),
							EditorGUIUtility.TrTextContent("Fix Now")))
					{
						bumpScale0.floatValue = 1;
					}
			}
			if (index == 1)
			{
				m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText1, bumpMap1, bumpMap1.textureValue != null ? bumpScale1 : null);
				if (bumpScale1.floatValue != 1 && UnityEditorInternal.InternalEditorUtility.IsMobilePlatform(EditorUserBuildSettings.activeBuildTarget))
					if (m_MaterialEditor.HelpBoxWithButton(
							EditorGUIUtility.TrTextContent("Bump scale is not supported on mobile platforms"),
							EditorGUIUtility.TrTextContent("Fix Now")))
					{
						bumpScale1.floatValue = 1;
					}
			}
			if (index == 2)
			{
				m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText2, bumpMap2, bumpMap2.textureValue != null ? bumpScale2 : null);
				if (bumpScale2.floatValue != 1 && UnityEditorInternal.InternalEditorUtility.IsMobilePlatform(EditorUserBuildSettings.activeBuildTarget))
					if (m_MaterialEditor.HelpBoxWithButton(
							EditorGUIUtility.TrTextContent("Bump scale is not supported on mobile platforms"),
							EditorGUIUtility.TrTextContent("Fix Now")))
					{
						bumpScale2.floatValue = 1;
					}
			}
			if (index == 3)
			{
				m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText3, bumpMap3, bumpMap3.textureValue != null ? bumpScale3 : null);
				if (bumpScale3.floatValue != 1 && UnityEditorInternal.InternalEditorUtility.IsMobilePlatform(EditorUserBuildSettings.activeBuildTarget))
					if (m_MaterialEditor.HelpBoxWithButton(
							EditorGUIUtility.TrTextContent("Bump scale is not supported on mobile platforms"),
							EditorGUIUtility.TrTextContent("Fix Now")))
					{
						bumpScale3.floatValue = 1;
					}
			}
		}

		void DoLabelTitle(int index)
		{
			if (index == 0)
				GUILayout.Label(Styles.primaryMapsText, EditorStyles.boldLabel);
			if (index == 1)
				GUILayout.Label(Styles.secondaryMapsText, EditorStyles.boldLabel);
			if (index == 2)
				GUILayout.Label(Styles.thirdMapsText, EditorStyles.boldLabel);
			if (index == 3)
				GUILayout.Label(Styles.fourthMapsText, EditorStyles.boldLabel);
		}

        void DoAlbedoArea(Material material, int index)
        {
			if (index == 0)
			{
				m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText0, albedoMap0, albedoColor);
				if (((BlendMode)material.GetFloat("_Mode") == BlendMode.Cutout))
				{
					m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1);
				}
			}
			if (index == 1)
			{
				m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText1, albedoMap1, albedoColor);
				if (((BlendMode)material.GetFloat("_Mode") == BlendMode.Cutout))
				{
					m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1);
				}
			}
			if (index == 2)
			{
				m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText2, albedoMap2, albedoColor);
				if (((BlendMode)material.GetFloat("_Mode") == BlendMode.Cutout))
				{
					m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1);
				}
			}
			if (index == 3)
			{
				m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText3, albedoMap3, albedoColor);
				if (((BlendMode)material.GetFloat("_Mode") == BlendMode.Cutout))
				{
					m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1);
				}
			}
		}

        void DoEmissionArea(Material material)
        {
			bool emissionEnabled = false;

            // Emission for GI?
            if (m_MaterialEditor.EmissionEnabledProperty())
            {
				emissionEnabled = true;

				bool hadEmissionTexture = emissionMap0.textureValue != null;

                // Texture and HDR color controls
                m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText0, emissionMap0, emissionColorForRendering0, false);

                // If texture was assigned and color was black set color to white
                float brightness = emissionColorForRendering0.colorValue.maxColorComponent;
                if (emissionMap0.textureValue != null && !hadEmissionTexture && brightness <= 0f)
                    emissionColorForRendering0.colorValue = Color.white;

				emissionEnabled = true;

				hadEmissionTexture = emissionMap1.textureValue != null;

				// Texture and HDR color controls
				m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText1, emissionMap1, emissionColorForRendering1, false);

				// If texture was assigned and color was black set color to white
				brightness = emissionColorForRendering1.colorValue.maxColorComponent;
				if (emissionMap1.textureValue != null && !hadEmissionTexture && brightness <= 0f)
					emissionColorForRendering1.colorValue = Color.white;

				emissionEnabled = true;

				hadEmissionTexture = emissionMap2.textureValue != null;

				// Texture and HDR color controls
				m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText2, emissionMap2, emissionColorForRendering2, false);

				// If texture was assigned and color was black set color to white
				brightness = emissionColorForRendering2.colorValue.maxColorComponent;
				if (emissionMap2.textureValue != null && !hadEmissionTexture && brightness <= 0f)
					emissionColorForRendering2.colorValue = Color.white;

				emissionEnabled = true;

				hadEmissionTexture = emissionMap3.textureValue != null;

				// Texture and HDR color controls
				m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText3, emissionMap3, emissionColorForRendering3, false);

				// If texture was assigned and color was black set color to white
				brightness = emissionColorForRendering3.colorValue.maxColorComponent;
				if (emissionMap3.textureValue != null && !hadEmissionTexture && brightness <= 0f)
					emissionColorForRendering3.colorValue = Color.white;
			}

			if (emissionEnabled)
			{
				// change the GI flag and fix it up with emissive as black if necessary
				m_MaterialEditor.LightmapEmissionFlagsProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel, true);
			}
		}

        void DoSpecularMetallicArea(int index)
        {
            bool hasGlossMap = false;
			int indentation = 2;

			if (index == 0)
			{
				hasGlossMap = metallicMap0.textureValue != null;
				m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText0, metallicMap0, hasGlossMap ? null : metallic0);

				indentation = 2; // align with labels of texture properties
				m_MaterialEditor.ShaderProperty(smoothness0, Styles.smoothnessText0, indentation);
			}
			else if (index == 1)
			{
				hasGlossMap = false;
				hasGlossMap = metallicMap1.textureValue != null;
				m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText1, metallicMap1, hasGlossMap ? null : metallic1);

				indentation = 2; // align with labels of texture properties
				m_MaterialEditor.ShaderProperty(smoothness1, Styles.smoothnessText1, indentation);
			}
			else if (index == 2)
			{
				hasGlossMap = false;
				hasGlossMap = metallicMap2.textureValue != null;
				m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText2, metallicMap2, hasGlossMap ? null : metallic2);

				indentation = 2; // align with labels of texture properties
				m_MaterialEditor.ShaderProperty(smoothness2, Styles.smoothnessText2, indentation);
			}
			else if (index == 3)
			{
				hasGlossMap = false;
				hasGlossMap = metallicMap3.textureValue != null;
				m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText3, metallicMap3, hasGlossMap ? null : metallic3);

				indentation = 2; // align with labels of texture properties
				m_MaterialEditor.ShaderProperty(smoothness3, Styles.smoothnessText3, indentation);
			}
			////////////////////////////////////////////////////////////////

			++indentation;
            if (smoothnessMapChannel != null)
                m_MaterialEditor.ShaderProperty(smoothnessMapChannel, Styles.smoothnessMapChannelText, indentation);
        }

        public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetOverrideTag("RenderType", "");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case BlendMode.Fade:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
            }
        }

        static SmoothnessMapChannel GetSmoothnessMapChannel(Material material)
        {
            int ch = (int)material.GetFloat("_SmoothnessTextureChannel");
            if (ch == (int)SmoothnessMapChannel.AlbedoAlpha)
                return SmoothnessMapChannel.AlbedoAlpha;
            else
                return SmoothnessMapChannel.SpecularMetallicAlpha;
        }

        static void SetMaterialKeywords(Material material, WorkflowMode workflowMode)
        {
			// Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
			// (MaterialProperty value might come from renderer material property block)

			//SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap") || material.GetTexture("_DetailNormalMap"));
			SetKeyword(material, "_NORMALMAP"
				, material.GetTexture("_BumpMap0") 
					|| material.GetTexture("_BumpMap1")
					|| material.GetTexture("_BumpMap2")
					|| material.GetTexture("_BumpMap3"));
			//if (workflowMode == WorkflowMode.Specular)
   //             SetKeyword(material, "_SPECGLOSSMAP", material.GetTexture("_SpecGlossMap"));
   //         else if (workflowMode == WorkflowMode.Metallic)
                SetKeyword(material, "_METALLICGLOSSMAP", material.GetTexture("_MetallicGlossMap"));

			//SetKeyword(material, "_PARALLAXMAP", material.GetTexture("_ParallaxMap"));
			SetKeyword(material, "_PARALLAXMAP", false);
			//SetKeyword(material, "_DETAIL_MULX2", material.GetTexture("_DetailAlbedoMap") || material.GetTexture("_DetailNormalMap"));

			// A material's GI flag internally keeps track of whether emission is enabled at all, it's enabled but has no effect
			// or is enabled and may be modified at runtime. This state depends on the values of the current flag and emissive color.
			// The fixup routine makes sure that the material is in the correct state if/when changes are made to the mode or color.
			MaterialEditor.FixupEmissiveFlag(material);
            bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);

            if (material.HasProperty("_SmoothnessTextureChannel"))
            {
                SetKeyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", GetSmoothnessMapChannel(material) == SmoothnessMapChannel.AlbedoAlpha);
            }
        }

        static void MaterialChanged(Material material, WorkflowMode workflowMode)
        {
            SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));

            SetMaterialKeywords(material, workflowMode);
        }

        static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }
    }
} // namespace UnityEditor
