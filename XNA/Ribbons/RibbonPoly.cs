using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ribbons
{
	public class RibbonPoly : RibbonBase
	{
		private Polygon[] polygons;

		private int nPolys;

		private float thickness = 40f;

		public RibbonPoly()
		{
			capacity = 10000;
			polygons = new Polygon[capacity];
			for (int i = 0; i < capacity; i++)
			{
				polygons[i] = new Polygon(this);
			}
			nPolys = 0;
		}

		protected override void ClearPolys()
		{
			nPolys = 0;
		}

		public List<Vector2> CreateSmoothPath(List<Vector2> oldPath, int subDivCount)
		{
			if (oldPath.Count < 4 || !Game.Instance.SplineSmooth)
			{
				return oldPath;
			}
			List<Vector2> list = new List<Vector2>();
			list.Add(oldPath[0]);
			for (int i = 1; i < oldPath.Count - 2; i++)
			{
				Vector2 value = new Vector2(oldPath[i - 1].X, oldPath[i - 1].Y);
				Vector2 vector = new Vector2(oldPath[i].X, oldPath[i].Y);
				Vector2 vector2 = new Vector2(oldPath[i + 1].X, oldPath[i + 1].Y);
				Vector2 value2 = new Vector2(oldPath[i + 2].X, oldPath[i + 2].Y);
				for (int j = 0; j < subDivCount; j++)
				{
					float amount = (float)j / (float)subDivCount;
					Vector2 item = Vector2.CatmullRom(value, vector, vector2, value2, amount);
					Vector2.Lerp(vector, vector2, amount);
					list.Add(item);
				}
			}
			list.Add(oldPath[oldPath.Count - 1]);
			return list;
		}

		public List<Vector3> CreateSmoothPath(List<Vector3> oldPath, int subDivCount)
		{
			if (oldPath.Count < 4 || !Game.Instance.SplineSmooth)
			{
				return oldPath;
			}
			List<Vector3> list = new List<Vector3>();
			list.Add(oldPath[0]);
			for (int i = 1; i < oldPath.Count - 2; i++)
			{
				Vector3 value = new Vector3(oldPath[i - 1].X, oldPath[i - 1].Y, oldPath[i - 1].Z);
				Vector3 vector = new Vector3(oldPath[i].X, oldPath[i].Y, oldPath[i].Z);
				Vector3 vector2 = new Vector3(oldPath[i + 1].X, oldPath[i + 1].Y, oldPath[i + 1].Z);
				Vector3 value2 = new Vector3(oldPath[i + 2].X, oldPath[i + 2].Y, oldPath[i + 1].Z);
				for (int j = 0; j < subDivCount; j++)
				{
					float amount = (float)j / (float)subDivCount;
					Vector3 item = Vector3.CatmullRom(value, vector, vector2, value2, amount);
					Vector3.Lerp(vector, vector2, amount);
					list.Add(item);
				}
			}
			list.Add(oldPath[oldPath.Count - 1]);
			return list;
		}

		public void BuildPolysRibbon()
		{
			ClearPolys();
			List<Vector3> list = new List<Vector3>();
			Vector3 vector = new Vector3((path[0].X < 0f) ? ((float)width - (0f - path[0].X) % (float)width) : (path[0].X % (float)width), (path[0].Y < 0f) ? ((float)height - (0f - path[0].Y) % (float)height) : (path[0].Y % (float)height), path[0].Z);
			Vector3 vector2 = vector - path[0];
			for (int i = 0; i < pathCount; i++)
			{
				list.Add(path[i] + vector2);
			}
			List<Vector3> list2 = CreateSmoothPath(list, 10);
			int num = list2.Count - 1;
			int num2 = list2.Count - 1;
			float num3 = 1f;
			float num4 = 1f / (float)Math.Max(1, num - 1);
			Vector3 vector3 = list2[0];
			Vector3 vector4 = list2[1];
			float num5 = vector3.Z * thickness;
			float num6 = vector4.X - vector3.X;
			float num7 = vector4.Y - vector3.Y;
			float num8 = (float)Math.Sqrt(num6 * num6 + num7 * num7);
			if (num8 == 0f)
			{
				float num9 = 0.0001f;
			}
			float num10 = num5 * num6 / num8;
			float num11 = num5 * num7 / num8;
			float num12 = vector3.X - num11;
			float num13 = vector3.Y + num10;
			float num14 = vector3.X + num11;
			float num15 = vector3.Y - num10;
			float val = 0.618f;
			double y = 0.40000000596046448;
			int num16 = 1;
			int num23;
			int num28;
			int num30;
			int num32;
			int num25;
			int num33;
			int num35;
			int num37;
			for (num16 = 1; num16 < num; num16++)
			{
				num3 = (float)Math.Pow((float)(num2 - num16) * num4, y);
				vector3 = list2[num16 - 1];
				vector4 = list2[num16];
				Vector3 vector5 = list2[num16 + 1];
				float x = vector4.X;
				float y2 = vector4.Y;
				float num17 = Math.Max(val, num3 * vector4.Z * thickness);
				float num18 = vector5.X - vector3.Y;
				float num19 = vector5.X - vector3.Y;
				float num9 = (float)Math.Sqrt(num18 * num18 + num19 * num19);
				if (num9 != 0f)
				{
					num9 = num17 / num9;
				}
				float num20 = num18 * num9;
				float num21 = num19 * num9;
				int num22;
				num23 = (num22 = (int)num12);
				int num24;
				num25 = (num24 = (int)num13);
				int num26 = num23 - num22;
				int num27 = num25 - num24;
				num23 = num26 + num22;
				num28 = num26 + (int)num14;
				float num29;
				num30 = num26 + (int)(num29 = x + num21);
				float num31;
				num32 = num26 + (int)(num31 = x - num21);
				num25 = num27 + num24;
				num33 = num27 + (int)num15;
				float num34;
				num35 = num27 + (int)(num34 = y2 - num20);
				float num36;
				num37 = num27 + (int)(num36 = y2 + num20);
				polygons[nPolys++].BuildPoly(new Vector2(num23, num25), new Vector2(num28, num33), new Vector2(num30, num35), new Vector2(num32, num37));
				num12 = num31;
				num13 = num36;
				num14 = num29;
				num15 = num34;
			}
			num23 = (int)num12;
			num28 = (int)num14;
			num30 = (int)list2[num].X;
			num32 = (int)list2[num].X;
			num25 = (int)num13;
			num33 = (int)num15;
			num35 = (int)list2[num].Y;
			num37 = (int)list2[num].Y;
			polygons[nPolys].BuildPoly(new Vector2(num23, num25), new Vector2(num28, num33), new Vector2(num30, num35), new Vector2(num32, num37));
		}

		public override void BuildPolys()
		{
			float val = 10f;
			ClearPolys();
			Vector3[] array = (Vector3[])path.Clone();
			Vector3 vector = new Vector3((path[0].X < 0f) ? ((float)width - (0f - path[0].X) % (float)width) : (path[0].X % (float)width), (path[0].Y < 0f) ? ((float)height - (0f - path[0].Y) % (float)height) : (path[0].Y % (float)height), path[0].Z);
			Vector3 vector2 = vector - path[0];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] += vector2;
			}
			List<Vector2> list = new List<Vector2>();
			List<Vector2> list2 = new List<Vector2>();
			if (loop)
			{
				float num = Math.Min(val, array[1].Z * thickness);
				Vector3 vector3 = array[2] - array[0];
				float num2 = (float)Math.Sqrt(vector3.X * vector3.X + vector3.Y * vector3.Y);
				if (num2 != 0f)
				{
					num2 = num / num2;
				}
				float num3 = vector3.X * num2;
				float num4 = vector3.Y * num2;
				list.Add(new Vector2(array[0].X - num4, array[0].Y + num3));
				list2.Add(new Vector2(array[0].X + num4, array[0].Y - num3));
			}
			else
			{
				list.Add(new Vector2(array[0].X, array[0].Y));
				list2.Add(new Vector2(array[0].X, array[0].Y));
			}
			for (int j = 1; j < pathCount - 1; j++)
			{
				Vector3 vector6 = array[j - 1];
				Vector3 vector4 = array[j];
				Vector3 vector7 = array[j + 1];
				float num5 = Math.Min(val, array[j].Z * thickness);
				Vector3 vector5 = array[j + 1] - array[j - 1];
				float num6 = (float)Math.Sqrt(vector5.X * vector5.X + vector5.Y * vector5.Y);
				if (num6 != 0f)
				{
					num6 = num5 / num6;
				}
				float num7 = vector5.X * num6;
				float num8 = vector5.Y * num6;
				list.Add(new Vector2(vector4.X - num8, vector4.Y + num7));
				list2.Add(new Vector2(vector4.X + num8, vector4.Y - num7));
			}
			List<Vector2> list3 = new List<Vector2>();
			List<Vector2> list4 = new List<Vector2>();
			list3 = CreateSmoothPath(list, 10);
			list4 = CreateSmoothPath(list2, 10);
			for (int k = 1; k < list3.Count; k++)
			{
				polygons[nPolys++].BuildPoly(list3[k - 1], list4[k - 1], list4[k], list3[k]);
			}
			if (loop)
			{
				polygons[nPolys++].BuildPoly(list3[list3.Count - 1], list4[list3.Count - 1], list4[0], list3[0]);
			}
		}

		public new void Draw(GraphicsDevice device, Effect effect)
		{
			if (!exists || nPolys <= 0)
			{
				return;
			}
			int num = 8 * nPolys;
			int num2 = 12 * nPolys;
			VertexPositionColoredNormal[] Vertices = new VertexPositionColoredNormal[num];
			int[] Indexes = new int[num2];
			for (int i = 0; i < nPolys; i++)
			{
				polygons[i].FillVertexBuffer(ref Vertices, ref Indexes, i);
			}
			effect.Parameters["World"].SetValue(Matrix.Identity);
			effect.CommitChanges();
			device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, num, Indexes, 0, num / 2);
			if (Game.Instance.DrawAllWorldEdges)
			{
				Matrix[] worldEdges = Game.Instance.WorldEdges;
				foreach (Matrix value in worldEdges)
				{
					effect.Parameters["World"].SetValue(value);
					effect.CommitChanges();
					device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, num, Indexes, 0, num / 2);
				}
			}
		}

		public override Game.RibbonType GetRibbonType()
		{
			return Game.RibbonType.ERibbonPoly;
		}
	}
}
