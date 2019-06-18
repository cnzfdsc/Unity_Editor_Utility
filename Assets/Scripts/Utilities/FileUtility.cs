using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileUtility
{
    public static List<string> GetAllDirectoryNameInDirectory(string dirPath)
	{
		List<string> dirs = new List<string>(Directory.EnumerateDirectories(dirPath));

// 		foreach (string dir in dirs)
// 		{
// 			Debug.Log(dir);
// 		}

		return dirs;
	}

	public static List<string> GetAllFileNameInDirectory(string dirPath)
	{
		List<string> files = new List<string>(Directory.EnumerateFiles(dirPath));

		//foreach (string file in files)
		//{
		//	Debug.Log(file);
		//}

		return files;
	}

	public static string GetFileExtension(string filePath)
	{
		int pointIndex = filePath.LastIndexOf('.');
		return filePath.Substring(pointIndex + 1, filePath.Length - pointIndex - 1);
	}
}
