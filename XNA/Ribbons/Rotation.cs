using System;
using Microsoft.Xna.Framework;

namespace Ribbons
{
	public class Rotation
	{
		private float angle;

		public Rotation()
		{
		}

		public Rotation(Vector2 fromVec)
		{
			angle = (float)Math.Atan(fromVec.Y / fromVec.X);
			angle += ((fromVec.X > 0f) ? ((float)Math.PI * 2f) : ((float)Math.PI));
		}

		public Rotation(Vector2 vec1, Vector2 vec2)
		{
			angle = (float)Math.Acos(Vector2.Dot(vec1, vec2) / (vec1.Length() * vec2.Length()));
		}

		public Rotation(float fromAngle)
		{
			angle = fromAngle;
		}

		public Vector2 ToVector()
		{
			return ToVector(1f);
		}

		public Vector2 ToVector(float length)
		{
			Vector2 vector = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
			vector.Normalize();
			return vector * length;
		}

		public static Rotation operator +(Rotation rot1, Rotation rot2)
		{
			float fromAngle = rot1.GetAngle() + rot2.GetAngle();
			return new Rotation(fromAngle);
		}

		public static Rotation operator -(Rotation rot1, Rotation rot2)
		{
			float fromAngle = rot1.GetAngle() - rot2.GetAngle();
			return new Rotation(fromAngle);
		}

		public static Rotation Lerp(Rotation rot1, Rotation rot2, float alpha)
		{
			return new Rotation(rot1.GetAngle() + DifferenceMod(rot1.GetAngle(), rot2.GetAngle(), (float)Math.PI * 2f) * alpha);
		}

		private static float Mod(float a, float b)
		{
			float num = a % b;
			if (num < 0f)
			{
				return b + num;
			}
			return num;
		}

		public static float DifferenceMod(float a, float b, float modulo)
		{
			if (Mod(b - a, modulo) > Mod(a - b, modulo))
			{
				return 0f - Mod(a - b, modulo);
			}
			return Mod(b - a, modulo);
		}

		public float GetAngle()
		{
			return angle;
		}

		public override string ToString()
		{
			return angle.ToString();
		}

		public static Rotation DiffBetweenAngles(Rotation rot1, Rotation rot2)
		{
			return new Rotation(DifferenceMod(rot1.GetAngle(), rot2.GetAngle(), (float)Math.PI * 2f));
		}

		public static Rotation ClampRotation(Rotation startRot, Rotation endRot, Rotation clampRot, bool clampClockwise)
		{
			float num = DifferenceMod(startRot.GetAngle(), endRot.GetAngle(), (float)Math.PI * 2f);
			float num2 = DifferenceMod(clampRot.GetAngle(), startRot.GetAngle(), (float)Math.PI * 2f);
			float num3 = DifferenceMod(clampRot.GetAngle(), endRot.GetAngle(), (float)Math.PI * 2f);
			if ((double)Math.Abs(num2 - num3) > Math.PI / 2.0)
			{
				return endRot;
			}
			if (Math.Sign(num2) != Math.Sign(num3))
			{
				if (clampClockwise && num > 0f)
				{
					return clampRot;
				}
				if (!clampClockwise && num < 0f)
				{
					return clampRot;
				}
			}
			return endRot;
		}
	}
}
