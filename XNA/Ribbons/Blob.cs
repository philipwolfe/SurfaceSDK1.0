using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ribbons
{
	public class Blob
	{
		public BlobDetectMgr parent;

		public int BlobRemoveCount;

		public double BlobRemoveTimer;

		public bool BlobIntersected;

		public bool BlobMarkForRemove;

		public bool BlobRemoved;

		public Vector2 center = default(Vector2);

		public float width;

		public float height;

		public float xMin;

		public float xMax;

		public float yMin;

		public float yMax;

		public int[] line;

		public int lineCount;

		public static int MAX_NBLINE = 4000;

		private List<Vector2> pointList = new List<Vector2>();

		public float scale = 1f;

		public List<Vector2> PointList
		{
			get
			{
				return pointList;
			}
		}

		public Blob(BlobDetectMgr _parent)
		{
			parent = _parent;
			line = new int[MAX_NBLINE];
			lineCount = 0;
		}

		public Blob(PointCollection pointCollection)
		{
			parent = null;
			Vector2 value = new Vector2((float)pointCollection[0].X, (float)pointCollection[0].Y);
			for (int i = 1; i < pointCollection.Count; i++)
			{
				Vector2 vector = new Vector2((float)pointCollection[i].X, (float)pointCollection[i].Y);
				int num = (int)Math.Ceiling(Vector2.Distance(value, vector) / 5f);
				for (int j = 0; j < num; j++)
				{
					Vector2 vector2 = Vector2.Lerp(value, vector, (float)j / (float)num);
					pointList.Add(new Vector2(vector2.X / 1024f, vector2.Y / 764f));
				}
				value = vector;
			}
			lineCount = pointList.Count - 1;
		}

		public Vector2 GetEdgeVectorA(int iEdge)
		{
			if (parent == null)
			{
				return pointList[iEdge];
			}
			if (iEdge * 2 < parent.linesToDrawCount * 2)
			{
				return parent.GetEdgeVector(line[iEdge * 2]);
			}
			return default(Vector2);
		}

		public Vector2 GetEdgeVectorB(int iEdge)
		{
			if (parent == null)
			{
				return pointList[iEdge + 1];
			}
			if (iEdge * 2 + 1 < parent.linesToDrawCount * 2)
			{
				return parent.GetEdgeVector(line[iEdge * 2 + 1]);
			}
			return default(Vector2);
		}

		public int GetEdgeNb()
		{
			return lineCount;
		}

		public void Update()
		{
			width = xMax - xMin;
			height = yMax - yMin;
			Vector2 vector = new Vector2(1024f, 768f);
			center.X = 0.5f * (xMax + xMin);
			center.Y = 0.5f * (yMax + yMin);
			center *= vector;
			lineCount /= 2;
		}

		public void Update(Blob newBlob)
		{
			center = newBlob.center;
			width = newBlob.width;
			height = newBlob.height;
			xMin = newBlob.xMin;
			xMax = newBlob.xMax;
			yMin = newBlob.yMin;
			yMax = newBlob.yMax;
			line = newBlob.line;
			lineCount = newBlob.lineCount;
		}

		public bool IsPointInside(Vector2 point)
		{
			bool flag = false;
			float x = point.X;
			float y = point.Y;
			Vector2 vector = new Vector2(1024f, 768f);
			int iEdge = lineCount - 1;
			Microsoft.Xna.Framework.Matrix matrix = Microsoft.Xna.Framework.Matrix.CreateTranslation(new Vector3(center * -1f, 0f));
			matrix *= Microsoft.Xna.Framework.Matrix.CreateScale(scale);
			matrix *= Microsoft.Xna.Framework.Matrix.CreateTranslation(new Vector3(center, 0f));
			for (int i = 0; i < lineCount; i++)
			{
				Vector2 vector2 = Vector2.Transform(GetEdgeVectorA(i) * vector, matrix);
				Vector2 vector3 = Vector2.Transform(GetEdgeVectorA(iEdge) * vector, matrix);
				if (((vector2.Y < y && vector3.Y >= y) || (vector3.Y < y && vector2.Y >= y)) && vector2.X + (y - vector2.Y) / (vector3.Y - vector2.Y) * (vector3.X - vector2.X) < x)
				{
					flag = !flag;
				}
				iEdge = i;
			}
			return flag;
		}

		public ArrayList GetPointList(Vector2 testPt, float averageDistBetweenPoints)
		{
			ArrayList arrayList = new ArrayList();
			Vector2 vector = new Vector2(1024f, 768f);
			Microsoft.Xna.Framework.Matrix matrix = Microsoft.Xna.Framework.Matrix.CreateTranslation(new Vector3(center * -1f, 0f));
			matrix *= Microsoft.Xna.Framework.Matrix.CreateScale(scale);
			matrix *= Microsoft.Xna.Framework.Matrix.CreateTranslation(new Vector3(center, 0f));
			Vector2[] array = new Vector2[lineCount];
			int num = -1;
			for (int i = 0; i < lineCount; i++)
			{
				Vector2 value = Vector2.Transform(GetEdgeVectorA(i) * vector, matrix);
				if (num < 0 && i > 5 && Vector2.Distance(value, array[0]) < 5f)
				{
					num = i;
				}
				int num2 = ((num > 0) ? (lineCount - 1 - (i - num)) : i);
				value = Vector2.Clamp(value, new Vector2(10f, 10f), new Vector2(1014f, 758f));
				array[num2] = value;
			}
			float num3 = 1E+11f;
			int num4 = -1;
			for (int j = 0; j < lineCount; j++)
			{
				Vector2 value2 = array[j];
				float num5 = Vector2.DistanceSquared(value2, testPt);
				if (num5 < num3)
				{
					num3 = num5;
					num4 = j;
				}
			}
			float num6 = 0f;
			Vector2 vector2 = array[num4];
			arrayList.Add(vector2);
			for (int k = num4 + 1; k < lineCount; k++)
			{
				Vector2 vector3 = array[k];
				num6 += Vector2.Distance(vector3, vector2);
				if (num6 >= averageDistBetweenPoints)
				{
					arrayList.Add(vector3);
					num6 -= averageDistBetweenPoints;
				}
				vector2 = vector3;
			}
			for (int l = 0; l < num4; l++)
			{
				Vector2 vector4 = array[l];
				num6 += Vector2.Distance(vector4, vector2);
				if (l == 0 || num6 >= averageDistBetweenPoints)
				{
					arrayList.Add(vector4);
					num6 -= averageDistBetweenPoints;
				}
				vector2 = vector4;
			}
			return arrayList;
		}

		public void Render(GraphicsDevice device)
		{
			Vector2 vector = new Vector2(1024f, 768f);
			Microsoft.Xna.Framework.Matrix matrix = Microsoft.Xna.Framework.Matrix.CreateTranslation(new Vector3(center * -1f, 0f));
			matrix *= Microsoft.Xna.Framework.Matrix.CreateScale(scale);
			matrix *= Microsoft.Xna.Framework.Matrix.CreateTranslation(new Vector3(center, 0f));
			for (int i = 0; i < lineCount; i++)
			{
				Vector2 position = GetEdgeVectorA(i) * vector;
				Vector2 position2 = GetEdgeVectorB(i) * vector;
				RenderUtil.DrawLine(device, Vector2.Transform(position, matrix), Vector2.Transform(position2, matrix), 4f, Microsoft.Xna.Framework.Graphics.Color.White);
			}
		}

		public void SetScale(float _scale)
		{
			scale = _scale;
		}
	}
}
