using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class WriteExcelTest : MonoBehaviour
{
	[MenuItem("MyMenu/ShowFiles")]
	public static void ShowFiles()
	{
		List<string> files = FileUtility.GetAllFileNameInDirectory(Application.dataPath + "\\Prefabs");
		foreach (string file in files)
		{
			string extension = FileUtility.GetFileExtension(file);
			if (extension != "meta")
				Debug.Log(file);
		}
	}

	private void Start()
	{
		string dataPath = Application.dataPath;
		ExcelWriter excelWriter = new ExcelWriter();
		excelWriter.OpenExcel(Application.dataPath + "ExcelTest.xlsx");
		excelWriter.OpenSheet("Launchers");
		excelWriter.SetExcelHeader(new string[] { "ShipName", "Launchers" });
		List<Vector3> vList = new List<Vector3>();
		vList.Add(Vector3.zero);
		vList.Add(Vector3.forward);
		vList.Add(Vector3.one);
		excelWriter.WriteRow(2, new string[] { "Avatar", VectorListToString(vList) });
		excelWriter.SaveAndCloseExcel();
	}

	void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
		{
		}

		if (Input.GetKeyDown(KeyCode.S))
		{

		}
	}

	private string VectorListToString(List<Vector3> vList)
	{
		StringBuilder sb = new StringBuilder();
		for (int iVector = 0; iVector < vList.Count; iVector++)
		{
			sb.Append(vList[iVector].x);
			sb.Append(",");
			sb.Append(vList[iVector].y);
			sb.Append(",");
			sb.Append(vList[iVector].z);
			if (iVector < vList.Count - 1)
				sb.Append("|");
		}

		return sb.ToString();
	}
}
