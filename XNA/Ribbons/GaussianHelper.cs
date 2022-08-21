using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ribbons
{
	internal class GaussianHelper
	{
		public static void SetBlurEffectParameters(ref Effect effect, float dx, float dy)
		{
			EffectParameter effectParameter = effect.Parameters["SampleWeights"];
			EffectParameter effectParameter2 = effect.Parameters["SampleOffsets"];
			int count = effectParameter.Elements.Count;
			float[] array = new float[count];
			Vector2[] array2 = new Vector2[count];
			array[0] = ComputeGaussian(0f);
			array2[0] = new Vector2(0f);
			float num = array[0];
			for (int i = 0; i < count / 2; i++)
			{
				num += (array[i * 2 + 2] = (array[i * 2 + 1] = ComputeGaussian(i + 1))) * 2f;
				float num2 = (float)(i * 2) + 1.5f;
				Vector2 vector = new Vector2(dx, dy) * num2;
				array2[i * 2 + 1] = vector;
				array2[i * 2 + 2] = -vector;
			}
			for (int j = 0; j < array.Length; j++)
			{
				array[j] /= num;
			}
			effectParameter.SetValue(array);
			effectParameter2.SetValue(array2);
		}

		private static float ComputeGaussian(float n)
		{
			float num = 2f;
			float num2 = num;
			return (float)(1.0 / Math.Sqrt(Math.PI * 2.0 * (double)num2) * Math.Exp((0f - n * n) / (2f * num2 * num2)));
		}
	}
}
