using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NewBehaviourScript : MonoBehaviour
{
	[MenuItem("Examples/Create Prefab")]
	static void CreatePrefab()
	{
		// Keep track of the currently selected GameObject(s)
		GameObject[] objectArray = Selection.gameObjects;

		foreach (GameObject go in objectArray)
		{
			string assetPath = string.Format("Assets/{0}.prefab", go.name);

			// Check if the prefab and/or name already exist at the path
			if (AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)))
			{
				if (EditorUtility.DisplayDialog("Are you sure?",
					"The prefab already exists. Do you want to overwrite it ?",
					"Yes", "No"))
				{
					CreateNew(go, assetPath);
				}
			}
			else
			{
				CreateNew(go, assetPath);
			}
		}
	}

	[MenuItem("Examples/Create Prefab", true)]
	static bool ValidataCreatePrefab()
	{
		return Selection.activeGameObject != null;
	}

	static void CreateNew(GameObject go, string assetPath)
	{
		//Object prefab = PrefabUtility.SaveAsPrefabAsset(go, assetPath);
		PrefabUtility.SaveAsPrefabAssetAndConnect(go, assetPath, InteractionMode.AutomatedAction);
	}

}
