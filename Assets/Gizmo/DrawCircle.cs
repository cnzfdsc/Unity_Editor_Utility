using UnityEngine;
using System.Collections;
using System;

public class DrawCircle : MonoBehaviour
{
	public Transform m_Transform;
	public Color m_Color = Color.green; // 线框颜色

	void Start()
	{
		if (m_Transform == null)
		{
			throw new Exception("Transform is NULL.");
		}
	}

	public float X = 300;
	public float Y = 300;
	public float SideLength = 100;
	private void OnGUI()
	{
		DrawBox(new Vector3(X, Y, 0), SideLength);
	}

	public Texture DebugRectTexture;
	private void DrawBox(Vector3 center, float sideLength)
	{
		GUI.Box(new Rect(center.x - sideLength / 2, center.y - sideLength / 2, sideLength, sideLength), DebugRectTexture);
		//GUI.Box(new Rect(center.x, center.y, sideLength, sideLength), DebugRectTexture);
	}

	void DrawCircleGizmo(Vector3 center, float radius)
	{
		float ThetaSetting = 0.1f; // 值越低圆环越平滑
		if (ThetaSetting < 0.0001f)
			ThetaSetting = 0.0001f;

		// 设置矩阵
		Matrix4x4 defaultMatrix = Gizmos.matrix;
		Matrix4x4 myTrans = Matrix4x4.Rotate(Camera.main.transform.rotation);
		myTrans = myTrans * Matrix4x4.Translate(center);

		Gizmos.matrix = myTrans;

		// 设置颜色
		Color defaultColor = Gizmos.color;
		Gizmos.color = m_Color;

		// 绘制圆环
		Vector3 beginPoint = Vector3.zero;
		Vector3 firstPoint = Vector3.zero;
		for (float theta = 0; theta < 2 * Mathf.PI; theta += ThetaSetting)
		{
			float x = radius * Mathf.Cos(theta);
			float y = radius * Mathf.Sin(theta);
			Vector3 endPoint = new Vector3(x, y, 0);
			if (theta == 0)
			{
				firstPoint = endPoint;
			}
			else
			{
				Gizmos.DrawLine(beginPoint, endPoint);
			}
			beginPoint = endPoint;
		}

		// 绘制最后一条线段
		Gizmos.DrawLine(firstPoint, beginPoint);

		// 恢复默认颜色
		Gizmos.color = defaultColor;

		// 恢复默认矩阵
		Gizmos.matrix = defaultMatrix;
	}

	void OnDrawGizmos()
	{
		DrawCircleGizmo(m_Transform.position, 1);
	}
}