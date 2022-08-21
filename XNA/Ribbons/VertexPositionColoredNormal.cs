using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ribbons
{
	public struct VertexPositionColoredNormal
	{
		private Vector3 vertexPosition;

		private Color vertexColor;

		private Vector3 vertexNormal;

		public static readonly VertexElement[] VertexElements = new VertexElement[3]
		{
			new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
			new VertexElement(0, 12, VertexElementFormat.Color, VertexElementMethod.Default, VertexElementUsage.Color, 0),
			new VertexElement(0, 16, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0)
		};

		public static int SizeInBytes
		{
			get
			{
				return 24;
			}
		}

		public Vector3 Position
		{
			get
			{
				return vertexPosition;
			}
			set
			{
				vertexPosition = value;
			}
		}

		public Color Color
		{
			get
			{
				return vertexColor;
			}
			set
			{
				vertexColor = value;
			}
		}

		public Vector3 Normal
		{
			get
			{
				return vertexNormal;
			}
			set
			{
				vertexNormal = value;
			}
		}

		public VertexPositionColoredNormal(Vector3 pos, Color color, Vector3 normal)
		{
			vertexPosition = pos;
			vertexColor = color;
			vertexNormal = normal;
		}
	}
}
