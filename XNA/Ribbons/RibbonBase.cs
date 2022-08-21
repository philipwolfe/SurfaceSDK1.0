using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ribbons
{
	public class RibbonBase
	{
		private static double twoPi = Math.PI * 2.0;

		private static float damp = 5f;

		private static float dampInv = 1f / damp;

		private static float damp1 = damp - 1f;

		private Vector2 jump = default(Vector2);

		public bool exists;

		public bool detached;

		public bool loop;

		private Rotation constrainRot = new Rotation((float)Math.PI / 64f);

		protected int width;

		protected int height;

		protected int capacity;

		protected int pathCount;

		protected Vector3[] path;

		private ArrayList blobPath;

		private Vector3[] detachPath;

		private int nextDetachIdx = -1;

		private Vector2 blobOldJump = default(Vector2);

		private Blob attachedBlob;

		private int nextBlobI;

		private float averageDistBetweenPoints;

		private float averagePressure;

		private int color = 1;

		public Vector2 Jump
		{
			get
			{
				return jump;
			}
		}

		public int PathCount
		{
			get
			{
				return pathCount;
			}
		}

		public bool Attached
		{
			get
			{
				return attachedBlob != null;
			}
		}

		public Color Color
		{
			get
			{
				return Game.Instance.GetColor(color);
			}
		}

		public RibbonBase()
		{
			capacity = 1000;
			path = new Vector3[capacity];
			for (int i = 0; i < capacity; i++)
			{
				path[i] = default(Vector3);
			}
			pathCount = 0;
		}

		public void Init(int _color, int _width, int _height)
		{
			color = _color;
			width = _width;
			height = _height;
			exists = false;
			detached = false;
			jump.X = 0f;
			jump.Y = 0f;
		}

		public void Clear()
		{
			pathCount = 0;
			exists = false;
			detached = false;
			ClearPolys();
		}

		protected virtual void ClearPolys()
		{
		}

		public void DetachFromContact()
		{
			detached = true;
			if (pathCount < 4)
			{
				exists = false;
			}
			Smooth();
			averageDistBetweenPoints = 0f;
			for (int i = 1; i < pathCount; i++)
			{
				averageDistBetweenPoints += Vector2.Distance(new Vector2(path[i - 1].X, path[i - 1].Y), new Vector2(path[i].X, path[i].Y));
			}
			averageDistBetweenPoints /= pathCount - 1;
			averagePressure = 0f;
			float num = 0f;
			for (int j = 0; j < pathCount; j++)
			{
				averagePressure += path[j].Z;
				if (path[j].Z > num)
				{
					num = path[j].Z;
				}
			}
			averagePressure /= pathCount;
			float num2 = DistToLast(path[0].X, path[0].Y);
			if (num2 < 20f)
			{
				Vector3 vector = path[pathCount - 1];
				path[0] = vector;
				jump.X = 0f;
				jump.Y = 0f;
				for (int k = 0; k < pathCount; k++)
				{
					path[k].Z = averagePressure;
				}
				loop = true;
			}
			else
			{
				int num3 = pathCount / 2;
				for (int l = num3; l < pathCount; l++)
				{
					float num4 = pathCount - num3;
					path[l].Z = Util.Interpolate((float)(l - num3) / num4, num, 0f);
				}
			}
		}

		public void Update()
		{
			if (pathCount <= 0)
			{
				return;
			}
			for (int num = pathCount - 1; num > 0; num--)
			{
				path[num].X = path[num - 1].X;
				path[num].Y = path[num - 1].Y;
			}
			if (attachedBlob != null && blobPath.Count > 1)
			{
				Vector2 vector = (Vector2)blobPath[nextBlobI];
				path[0].X = vector.X;
				path[0].Y = vector.Y;
				nextBlobI = (nextBlobI + 1) % (blobPath.Count - 1);
			}
			else if (nextDetachIdx >= 0)
			{
				path[0].X = detachPath[nextDetachIdx].X;
				path[0].Y = detachPath[nextDetachIdx].Y;
				nextDetachIdx--;
				if (nextDetachIdx == -1)
				{
					jump = blobOldJump;
				}
			}
			else
			{
				path[0].X = path[pathCount - 1].X - jump.X;
				path[0].Y = path[pathCount - 1].Y - jump.Y;
			}
			if (attachedBlob == null && nextDetachIdx < 0)
			{
				CollideBlobs();
			}
			else if (attachedBlob != null && attachedBlob.BlobRemoved)
			{
				DetachBlob();
			}
			BuildPolys();
		}

		private void CollideBlobs()
		{
			if (Game.Instance.BlobMgr == null)
			{
				return;
			}
			float num = (int)path[0].X;
			float num2 = (int)path[0].Y;
			num = ((num < 0f) ? ((float)width - (0f - num) % (float)width) : (num % (float)width));
			num2 = ((num2 < 0f) ? ((float)height - (0f - num2) % (float)height) : (num2 % (float)height));
			Vector2 vector = new Vector2(num, num2);
			foreach (Blob blob in Game.Instance.BlobMgr.GetBlobs())
			{
				if (blob.IsPointInside(vector))
				{
					AttachBlob(blob, vector);
				}
			}
		}

		public void AttachBlob(Blob blob, Vector2 testPt)
		{
			blob.BlobIntersected = true;
			blobPath = blob.GetPointList(testPt, averageDistBetweenPoints);
			attachedBlob = blob;
			Vector3 vector = new Vector3(testPt.X, testPt.Y, 0f) - path[0];
			vector.Z = 0f;
			for (int i = 0; i < pathCount; i++)
			{
				path[i] += vector;
			}
			nextBlobI = 0;
			detachPath = new Vector3[capacity];
			path.CopyTo(detachPath, 0);
			blobOldJump = jump;
			jump = default(Vector2);
		}

		public void DetachBlob()
		{
			attachedBlob = null;
			Vector3 vector = path[0];
			Vector3 vector2 = vector - detachPath[pathCount - 1];
			vector2.Z = 0f;
			for (int i = 0; i < pathCount; i++)
			{
				detachPath[i] += vector2;
			}
			nextDetachIdx = pathCount - 1;
		}

		public Vector3 GetLastPoint()
		{
			if (pathCount > 0)
			{
				return path[pathCount - 1];
			}
			return Vector3.Zero;
		}

		public bool IsValid()
		{
			return true;
		}

		public void AddPoint(float x, float y, float area)
		{
			if (pathCount < capacity)
			{
				float v = DistToLast(x, y);
				float pressureFromVelocity = GetPressureFromVelocity(v);
				path[pathCount++] = new Vector3(x, y, pressureFromVelocity);
				if (pathCount > 1)
				{
					exists = true;
					jump.X = path[pathCount - 1].X - path[0].X;
					jump.Y = path[pathCount - 1].Y - path[0].Y;
				}
			}
		}

		private float GetPressureFromVelocity(float v)
		{
			float num = 20f;
			float num2 = 0.1f;
			float num3 = ((pathCount > 0) ? path[pathCount - 1].Z : 0f);
			return (num2 + Math.Max(0f, 1f - v / num) + damp1 * num3) * dampInv;
		}

		private void SetPressures()
		{
			double num = 0.0;
			double num2 = 1.0 / (double)(pathCount - 1) * twoPi;
			for (int i = 0; i < pathCount; i++)
			{
				float z = (float)Math.Sqrt((1.0 - Math.Cos(num)) * 0.5);
				path[i].Z = z;
				num += num2;
			}
		}

		public float DistToLast(float ix, float iy)
		{
			if (pathCount > 0)
			{
				Vector3 vector = path[pathCount - 1];
				float num = vector.X - ix;
				float num2 = vector.Y - iy;
				return (float)Math.Sqrt(num * num + num2 * num2);
			}
			return 30f;
		}

		public void Smooth()
		{
			float num = 18f;
			float num2 = 1f / (num + 2f);
			for (int i = 1; i < pathCount - 2; i++)
			{
				Vector3 vector = path[i - 1];
				Vector3 vector2 = path[i];
				Vector3 vector3 = path[i + 1];
				vector2.X = (vector.X + num * vector2.X + vector3.X) * num2;
				vector2.Y = (vector.Y + num * vector2.Y + vector3.Y) * num2;
				vector2.Z = (vector.Z + num * vector2.Z + vector3.Z) * num2;
			}
		}

		public virtual void BuildPolys()
		{
		}

		public void Draw(GraphicsDevice device, Effect effect)
		{
		}

		public virtual void Draw(GraphicsDevice device, Effect effect, SpriteBatch spriteBatch)
		{
		}

		public virtual Game.RibbonType GetRibbonType()
		{
			return Game.RibbonType.ERibbonSplat;
		}
	}
}
