using Microsoft.Xna.Framework;

namespace Ribbons
{
	public class EdgeDetection : Metaballs2D
	{
		public int imgWidth;

		public int imgHeight;

		public byte[] pixels;

		public float m_coeff = 765f;

		public EdgeDetection(int imgWidth, int imgHeight)
		{
			this.imgWidth = imgWidth;
			this.imgHeight = imgHeight;
			Init(imgWidth, imgHeight);
		}

		public void SetThreshold(float value)
		{
			if (value < 0f)
			{
				value = 0f;
			}
			if (value > 1f)
			{
				value = 1f;
			}
			SetIsovalue(value * m_coeff);
		}

		public void SetImage(byte[] _pixels)
		{
			pixels = _pixels;
		}

		public void ComputeIsovalue()
		{
			for (int i = 0; i < imgHeight; i++)
			{
				for (int j = 0; j < imgWidth; j++)
				{
					int num = j + imgWidth * i;
					gridValue[num] = (float)(int)pixels[num] * 3f;
				}
			}
			for (int j = 0; j < resx - 1; j++)
			{
				for (int i = 0; i < resy - 1; i++)
				{
					int num = j + resx * i;
					int num2 = 0;
					int num3 = resx * i;
					int num4 = resx * (i + 1);
					if (gridValue[j + num3] < isovalue)
					{
						num2 |= 1;
					}
					if (gridValue[j + 1 + num3] < isovalue)
					{
						num2 |= 2;
					}
					if (gridValue[j + 1 + num4] < isovalue)
					{
						num2 |= 4;
					}
					if (gridValue[j + num4] < isovalue)
					{
						num2 |= 8;
					}
					squareIndices[num] = num2;
				}
			}
		}

		protected int GetSquareIndex(int x, int y)
		{
			return squareIndices[x + resx * y];
		}

		public Vector2 GetEdgeVector(int index)
		{
			return edgeVectors[index];
		}
	}
}
