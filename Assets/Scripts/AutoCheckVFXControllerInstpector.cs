using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AutoCheckVFXController))]
[CanEditMultipleObjects]
public class AutoCheckVFXControllerInspector : Editor
{
	SerializedProperty IsLoop_Prop;
	SerializedProperty Duration_Prop;
	SerializedProperty DestroyDelayAfterPlay_Prop;
	SerializedProperty PSList_Prop;
	AutoCheckVFXController VFXController;

	void OnEnable()
	{
		IsLoop_Prop = serializedObject.FindProperty("IsLoop");
		Duration_Prop = serializedObject.FindProperty("Duration");
		DestroyDelayAfterPlay_Prop = serializedObject.FindProperty("DestroyDelayAfterPlay");
		PSList_Prop = serializedObject.FindProperty("PSList");
		VFXController = target as AutoCheckVFXController;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (GUILayout.Button("CheckAndSetupVFXController"))
		{
			CheckAndSetupVFXController();
		}

		serializedObject.ApplyModifiedProperties();
	}

	public void CheckAndSetupVFXController()
	{
		// 获取所有粒子发射器
		// UNDONE, 分别记录需要瞬间销毁的和需要停止播放的特效
		ParticleSystem[] PSs = VFXController.GetComponentsInChildren<ParticleSystem>();
		PSList_Prop.arraySize = PSs.Length;
		for (int iPS = 0; iPS < PSs.Length; iPS++)
		{
			PSList_Prop.GetArrayElementAtIndex(iPS).objectReferenceValue = PSs[iPS];
		}

		// 获取是不是循环的
		bool isLoop = false;
		for (int iPS = 0; iPS < PSs.Length; iPS++)
		{
			if (PSs[iPS].emission.enabled && PSs[iPS].main.loop)
			{
				isLoop = true;
				break;
			}
		}

		IsLoop_Prop.boolValue = isLoop;

		// 获取自动停止时间
		// 虽然粒子发射器不需要这个参数. 但是其他类型的特效是需要的
		// 比如激光类型. 需要检查动画时间. 在动画时间结束后销毁这个特效
		// duration这个参数预计可以不暴露出来, 不过为了逻辑清晰, 还是保留这个参数
		float duration = 0;
		float destroyDelay = 0;
		for (int iPS = 0; iPS < PSs.Length; iPS++)
		{
			ParticleSystem ps = PSs[iPS];
			// 停止播放时间
			if (ps.emission.enabled && !ps.main.loop && ps.main.duration > duration)
			{
				duration = ps.main.duration;
			}

			// 自动销毁时间
			if (ps.emission.enabled && !ps.main.loop
				&& (ps.main.duration + ps.main.startLifetimeMultiplier) > destroyDelay)
			{
				destroyDelay = ps.main.duration + ps.main.startLifetimeMultiplier;
			}
		}

		Duration_Prop.floatValue = duration;
		DestroyDelayAfterPlay_Prop.floatValue = destroyDelay;
	}

	//private float GetParticleLifetime(ParticleSystem ps)
	//{
	//	float lifeTime = 0;
	//	if (ps.main.startLifetime.constant > lifeTime)
	//	{
	//		lifeTime = ps.main.startLifetime.constant;
	//	}

	//	return ps.main.startLifetimeMultiplier * lifeTime;
	//}
}
