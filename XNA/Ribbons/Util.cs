using System;

namespace Ribbons
{
	internal class Util
	{
		private static float Persistence = 0.2f;

		private static int NumberOfOctaves = 4;

		private static Random rand;

		public static void Init(int _seed)
		{
			rand = new Random(_seed);
		}

		public static float Rand()
		{
			return (float)rand.NextDouble();
		}

		public static int Rand(int min, int max)
		{
			return rand.Next(min, max);
		}

		public static float Normalize(float value, float minimum, float maximum)
		{
			return (value - minimum) / (maximum - minimum);
		}

		public static float Interpolate(float normValue, float minimum, float maximum)
		{
			return minimum + (maximum - minimum) * normValue;
		}

		public static float Map(float value, float min1, float max1, float min2, float max2)
		{
			return Interpolate(Normalize(value, min1, max1), min2, max2);
		}

		public static float Constrain(float value, float min, float max)
		{
			return Math.Min(Math.Max(value, min), max);
		}

		public static float Radians(float degrees)
		{
			return degrees / 57.29578f;
		}

		private static float Noise(float x, float y)
		{
			int num = (int)Math.Round(x + y * 57f);
			num = (num << 13) ^ num;
			return 1f - (float)((num * (num * num * 15731 + 789221) + 1376312589) & 0x7FFFFFFF) / 1.07374182E+09f;
		}

		private static float SmoothNoise(float x, float y)
		{
			float num = (Noise(x - 1f, y - 1f) + Noise(x + 1f, y - 1f) + Noise(x - 1f, y + 1f) + Noise(x + 1f, y + 1f)) / 16f;
			float num2 = (Noise(x - 1f, y) + Noise(x + 1f, y) + Noise(x, y - 1f) + Noise(x, y + 1f)) / 8f;
			float num3 = Noise(x, y) / 4f;
			return num + num2 + num3;
		}

		private static float InterpolatedNoise(float x, float y)
		{
			int num = (int)x;
			float maximum = x - (float)num;
			int num2 = (int)y;
			float maximum2 = y - (float)num2;
			float normValue = SmoothNoise(num, num2);
			float minimum = SmoothNoise(num + 1, num2);
			float normValue2 = SmoothNoise(num, num2 + 1);
			float minimum2 = SmoothNoise(num + 1, num2 + 1);
			float normValue3 = Interpolate(normValue, minimum, maximum);
			float minimum3 = Interpolate(normValue2, minimum2, maximum);
			return Interpolate(normValue3, minimum3, maximum2);
		}

		public static float PerlinNoise2D(float x, float y)
		{
			float num = 0f;
			float persistence = Persistence;
			float num2 = NumberOfOctaves - 1;
			for (int i = 0; (float)i < num2; i++)
			{
				float num3 = 2 * i;
				num += InterpolatedNoise(x * num3, y * num3);
			}
			return num;
		}
	}
}
