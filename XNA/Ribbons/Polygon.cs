using Microsoft.Xna.Framework;

namespace Ribbons
{
	public class Polygon
	{
		private RibbonPoly ribbon;

		private Vector2 vecA;

		private Vector2 vecB;

		private Vector2 vecC;

		private Vector2 vecD;

		public Polygon(RibbonPoly _ribbon)
		{
			ribbon = _ribbon;
		}

		public void BuildPoly(Vector2 _vecA, Vector2 _vecB, Vector2 _vecC, Vector2 _vecD)
		{
			vecA = _vecA;
			vecB = _vecB;
			vecC = _vecC;
			vecD = _vecD;
		}

		public void FillVertexBuffer(ref VertexPositionColoredNormal[] Vertices, ref int[] Indexes, int polyIdx)
		{
			Vector2 value = vecA - vecD;
			Vector3 vector = new Vector3(value, 0f);
			vector.Normalize();
			Vector3 vector2 = Vector3.Cross(new Vector3(0f, 0f, 1f), vector);
			vector2.Normalize();
			float[] array = new float[2] { 0f, 0.5f };
			float[] array2 = new float[2] { 0.5f, 1f };
			Vector3[] array3 = new Vector3[2]
			{
				vector2,
				new Vector3(0f, 0f, -1f)
			};
			Vector3[] array4 = new Vector3[2]
			{
				new Vector3(0f, 0f, -1f),
				vector2 * -1f
			};
			int num = 8 * polyIdx;
			int num2 = num;
			int num3 = 12 * polyIdx;
			for (int i = 0; i < 2; i++)
			{
				float z = 0f;
				Vector3 position = Vector3.Lerp(new Vector3(vecA, z), new Vector3(vecB, z), array[i]);
				Vector3 position2 = Vector3.Lerp(new Vector3(vecA, z), new Vector3(vecB, z), array2[i]);
				Vector3 position3 = Vector3.Lerp(new Vector3(vecD, z), new Vector3(vecC, z), array[i]);
				Vector3 position4 = Vector3.Lerp(new Vector3(vecD, z), new Vector3(vecC, z), array2[i]);
				Vertices[num].Position = position;
				Vertices[num].Normal = array3[i];
				Vertices[num++].Color = ribbon.Color;
				Vertices[num].Position = position2;
				Vertices[num].Color = ribbon.Color;
				Vertices[num++].Normal = array4[i];
				Vertices[num].Position = position3;
				Vertices[num].Color = ribbon.Color;
				Vertices[num++].Normal = array3[i];
				Vertices[num].Position = position4;
				Vertices[num].Color = ribbon.Color;
				Vertices[num++].Normal = array4[i];
				Indexes[num3++] = num2 + i * 4;
				Indexes[num3++] = num2 + i * 4 + 1;
				Indexes[num3++] = num2 + i * 4 + 2;
				Indexes[num3++] = num2 + i * 4 + 2;
				Indexes[num3++] = num2 + i * 4 + 1;
				Indexes[num3++] = num2 + i * 4 + 3;
			}
		}
	}
}
