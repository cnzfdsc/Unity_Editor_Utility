using UnityEngine;
using Excel;
using System.Data;
using System.IO;
using System.Collections.Generic;
using OfficeOpenXml;
using UnityEditor;


public class ExcelWriter
{
	private ExcelPackage m_ExcelPackage;
	private ExcelWorksheet m_WorkSheet;
	private int m_ColumnCount;

	/// <summary>
	/// 写入 Excel ; 需要添加 OfficeOpenXml.dll;
	/// </summary>
	public void OpenExcel(string excelPath)
	{
		//通过面板设置excel路径
		//string outputDir = EditorUtility.SaveFilePanel("Save Excel", "", "New Resource", "xlsx");
		
		//string path = Application.dataPath + "/" + excelPath;
		FileInfo newFile = new FileInfo(excelPath);

		if (!newFile.Exists)
		{
			newFile = new FileInfo(excelPath);
		}
		
		m_ExcelPackage = new ExcelPackage(newFile);
	}

	public ExcelWorksheet OpenSheet(string sheetName)
	{
		if (!ExistSheet(sheetName))
		{
			m_WorkSheet = m_ExcelPackage.Workbook.Worksheets.Add(sheetName);
		}
		else
		{
			m_WorkSheet = m_ExcelPackage.Workbook.Worksheets[sheetName];
		}

		return m_WorkSheet;
	}

	public void SetExcelHeader(string[] headColumns)
	{
		m_ColumnCount = headColumns.Length;

		for (int iCol = 0; iCol < headColumns.Length; iCol++)
		{
			m_WorkSheet.Cells[1, iCol+1].Value = headColumns[iCol];
		}
	}

	/// <summary>
	/// 强制写一个格子的数据
	/// </summary>
	/// <param name="row"></param>
	/// <param name="column"></param>
	/// <param name="value"></param>
	public void WriteExcel(int row, int column, string value)
	{
		m_WorkSheet.Cells[row, column].Value = value;

		//m_WorkSheet.Cells[1, 1].Value = "ID";
	}

	public void WriteRow(int row, string[] values)
	{
		if (m_ColumnCount == 0)
		{
			Debug.LogErrorFormat("还没设置表头");
			return;
		}

		if (values.Length != m_ColumnCount)
		{
			Debug.LogErrorFormat("列数与表头不匹配");
		}

		for (int iCol = 0; iCol < values.Length; iCol++)
		{
			m_WorkSheet.Cells[row, iCol+1].Value = values[iCol];
		}
	}

	public void SaveAndCloseExcel()
	{
		m_ExcelPackage.Save();
		m_ExcelPackage.Dispose();
	}

	private bool ExistSheet(string sheetName)
	{
		bool exist = false;
		for (int iSheet = 0; iSheet < m_ExcelPackage.Workbook.Worksheets.Count; iSheet++)
		{
			m_ExcelPackage.Workbook.Worksheets[iSheet+1].Name = sheetName;
			exist = true;
			break;
		}

		return exist;
	}
}
