using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrefabTest : MonoBehaviour
{
	public GameObject PrefabHere;
	GameObject testGO;

	void Start()
    {
		testGO = PrefabUtility.InstantiatePrefab(PrefabHere) as GameObject;
		GameObject prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource<GameObject>(testGO);
		if (prefab != null)
		{
			Debug.Log(prefab.name);
		}
		else
		{
			Debug.Log("找不到Prefab");
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			if (testGO != null)
			{
				PrefabUtility.ApplyPrefabInstance(testGO, InteractionMode.AutomatedAction);
			}
		}
	}
}
