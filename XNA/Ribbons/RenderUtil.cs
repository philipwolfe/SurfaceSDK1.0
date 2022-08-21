using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ribbons
{
	internal class RenderUtil
	{
		public static void DrawTri(GraphicsDevice device, Vector2 pt1, Vector2 pt2, Vector2 pt3, Color color)
		{
			VertexPositionColor[] array = new VertexPositionColor[3];
			float z = 0f;
			array[0].Position = new Vector3(pt3, z);
			array[0].Color = color;
			array[1].Position = new Vector3(pt2, z);
			array[1].Color = color;
			array[2].Position = new Vector3(pt1, z);
			array[2].Color = color;
			device.DrawUserPrimitives(PrimitiveType.TriangleList, array, 0, 1);
		}

		public static void DrawQuad(GraphicsDevice device, Vector2 ll, Vector2 ul, Vector2 lr, Vector2 ur, Color color)
		{
			VertexPositionColor[] array = new VertexPositionColor[4];
			int[] array2 = new int[6];
			float z = 0f;
			array[0].Position = new Vector3(ll, z);
			array[0].Color = color;
			array[1].Position = new Vector3(ul, z);
			array[1].Color = color;
			array[2].Position = new Vector3(lr, z);
			array[2].Color = color;
			array[3].Position = new Vector3(ur, z);
			array[3].Color = color;
			array2[0] = 0;
			array2[1] = 1;
			array2[2] = 2;
			array2[3] = 2;
			array2[4] = 1;
			array2[5] = 3;
			device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, array, 0, 4, array2, 0, 2);
		}

		public static void DrawLine(GraphicsDevice device, Vector2 start, Vector2 end, float thickness, Color color)
		{
			Vector3 vector = new Vector3(start - end, 0f);
			Vector3 vector2 = Vector3.Cross(vector, Vector3.UnitZ);
			vector2.Normalize();
			Vector2 vector3 = new Vector2(vector2.X, vector2.Y);
			Vector2 ll = start + vector3 * thickness;
			Vector2 ul = start - vector3 * thickness;
			Vector2 lr = end + vector3 * thickness;
			Vector2 ur = end - vector3 * thickness;
			DrawQuad(device, ll, ul, lr, ur, color);
		}

		public static void DrawCircle(GraphicsDevice device, Vector2 center, float radius, Color color)
		{
			int num = 32;
			VertexPositionColor[] array = new VertexPositionColor[num * 3];
			float num2 = (float)Math.PI * 2f / (float)num;
			int num3 = 0;
			for (int i = 0; i < num; i++)
			{
				float z = 0f;
				array[num3].Position = new Vector3(center, z);
				array[num3].Color = color;
				Vector2 vector = new Vector2((float)Math.Sin((float)i * num2) * radius, (float)Math.Cos((float)i * num2) * radius);
				array[num3].Position = new Vector3(center + vector, z);
				array[num3].Color = color;
				Vector2 vector2 = new Vector2((float)Math.Sin((float)(i + 1) * num2) * radius, (float)Math.Cos((float)(i + 1) * num2) * radius);
				array[num3].Position = new Vector3(center + vector2, z);
				array[num3].Color = color;
			}
			device.DrawUserPrimitives(PrimitiveType.TriangleList, array, 0, num);
		}

		public static void DrawRing(GraphicsDevice device, Vector2 center, float insideRadius, float outsideRadius, Color insideColor, Color outsideColor)
		{
			int num = 64;
			VertexPositionColoredNormal[] array = new VertexPositionColoredNormal[num * 3 * 2];
			float num2 = (float)Math.PI * 2f / (float)num;
			int num3 = 0;
			for (int i = 0; i < num; i++)
			{
				float z = 100f;
				Vector2 vector = new Vector2((float)Math.Sin((float)i * num2), (float)Math.Cos((float)i * num2));
				Vector2 vector2 = new Vector2((float)Math.Sin((float)(i + 1) * num2), (float)Math.Cos((float)(i + 1) * num2));
				Vector2 value = center + vector * insideRadius;
				Vector2 value2 = center + vector2 * insideRadius;
				Vector2 value3 = center + vector * outsideRadius;
				Vector2 value4 = center + vector2 * outsideRadius;
				array[num3].Position = new Vector3(value, z);
				array[num3].Color = insideColor;
				array[num3++].Normal = Vector3.UnitZ;
				array[num3].Position = new Vector3(value3, z);
				array[num3].Color = outsideColor;
				array[num3++].Normal = Vector3.UnitZ;
				array[num3].Position = new Vector3(value4, z);
				array[num3].Color = outsideColor;
				array[num3++].Normal = Vector3.UnitZ;
				array[num3].Position = new Vector3(value4, z);
				array[num3].Color = outsideColor;
				array[num3++].Normal = Vector3.UnitZ;
				array[num3].Position = new Vector3(value2, z);
				array[num3].Color = insideColor;
				array[num3++].Normal = Vector3.UnitZ;
				array[num3].Position = new Vector3(value, z);
				array[num3].Color = insideColor;
				array[num3++].Normal = Vector3.UnitZ;
			}
			device.DrawUserPrimitives(PrimitiveType.TriangleList, array, 0, num * 2);
		}

		public static int FillVertsForQuad(ref VertexPositionColor[] Vertices, int vertexIdx, Color color, Vector3 ul, Vector3 ur, Vector3 ll, Vector3 lr, Vector3 norm)
		{
			Vertices[vertexIdx].Position = ul;
			Vertices[vertexIdx].Color = color;
			Vertices[vertexIdx].Position = ur;
			Vertices[vertexIdx].Color = color;
			Vertices[vertexIdx].Position = lr;
			Vertices[vertexIdx].Color = color;
			Vertices[vertexIdx].Position = ll;
			Vertices[vertexIdx].Color = color;
			Vertices[vertexIdx].Position = lr;
			Vertices[vertexIdx].Color = color;
			Vertices[vertexIdx].Position = ul;
			Vertices[vertexIdx].Color = color;
			return vertexIdx;
		}

		public static void DrawBeveledQuad(GraphicsDevice device, Vector2 ul, Vector2 ur, Vector2 ll, Vector2 lr, Color color, float topBevelWidth, float bottomBevelWidth, float bevelHeight, bool pressed)
		{
			VertexPositionColor[] Vertices = new VertexPositionColor[30];
			int vertexIdx = 0;
			Vector3 ul2 = new Vector3(ul, pressed ? bevelHeight : 0f);
			Vector3 ur2 = new Vector3(ur, pressed ? bevelHeight : 0f);
			Vector3 ll2 = new Vector3(ll, pressed ? bevelHeight : 0f);
			Vector3 lr2 = new Vector3(lr, pressed ? bevelHeight : 0f);
			Vector3 ll3 = new Vector3(ul.X + topBevelWidth, ul.Y + (ll.Y - ul.Y) / 2f, pressed ? 0f : bevelHeight);
			Vector3 vector = new Vector3(ur.X - topBevelWidth, ur.Y + (ll.Y - ul.Y) / 2f, pressed ? 0f : bevelHeight);
			Vector3 vector2 = new Vector3(ll.X + bottomBevelWidth, ll.Y - (ll.Y - ul.Y) / 2f, pressed ? 0f : bevelHeight);
			Vector3 vector3 = new Vector3(lr.X - bottomBevelWidth, lr.Y - (ll.Y - ul.Y) / 2f, pressed ? 0f : bevelHeight);
			Vector3 vector4 = (pressed ? (Vector3.UnitY * -1f) : Vector3.UnitY);
			vertexIdx = FillVertsForQuad(ref Vertices, vertexIdx, color, ul2, ur2, ll3, vector, vector4);
			vertexIdx = FillVertsForQuad(ref Vertices, vertexIdx, color, vector2, vector3, ll2, lr2, vector4 * -1f);
			Vector3 vector5 = (pressed ? (Vector3.UnitX * -1f) : Vector3.UnitX);
			vertexIdx = FillVertsForQuad(ref Vertices, vertexIdx, color, ul2, vector, ll2, vector2, vector5 * -1f);
			vertexIdx = FillVertsForQuad(ref Vertices, vertexIdx, color, vector, ur2, vector3, lr2, vector5);
			device.DrawUserPrimitives(PrimitiveType.TriangleList, Vertices, 0, vertexIdx / 3);
		}
	}
}
