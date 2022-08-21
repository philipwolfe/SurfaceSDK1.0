using System;
using System.Collections;
using Microsoft.Xna.Framework;

namespace Ribbons
{
	public class BlobDetectMgr : EdgeDetection
	{
		private ArrayList blobs = new ArrayList();

		private bool[] gridVisited;

		public BlobDetectMgr(int imgWidth, int imgHeight)
			: base(imgWidth, imgHeight)
		{
			gridVisited = new bool[gridValueCount];
		}

		public ArrayList GetBlobs()
		{
			return blobs;
		}

		public void UpdateBlobs(GameTime gameTime, byte[] pixels)
		{
			SetImage(pixels);
			for (int i = 0; i < gridValueCount; i++)
			{
				gridVisited[i] = false;
			}
			ComputeIsovalue();
			ArrayList arrayList = new ArrayList();
			linesToDrawCount = 0;
			float num = 0f;
			for (int j = 0; j < resx - 1; j++)
			{
				float num2 = 0f;
				for (int k = 0; k < resy - 1; k++)
				{
					int num3 = j + resx * k;
					if (gridVisited[num3])
					{
						continue;
					}
					int squareIndex = GetSquareIndex(j, k);
					if (squareIndex > 0 && squareIndex < 15)
					{
						Blob blob = FindBlob(j, k);
						if (blob != null)
						{
							arrayList.Add(blob);
						}
					}
					num2 += stepy;
				}
				num += stepx;
			}
			linesToDrawCount /= 2;
			foreach (Blob item in arrayList)
			{
				Blob blob6 = item;
			}
			ArrayList arrayList2 = (ArrayList)blobs.Clone();
			ArrayList arrayList3 = new ArrayList();
			foreach (Blob item2 in arrayList)
			{
				bool flag = false;
				foreach (Blob blob5 in blobs)
				{
					if (!flag && Vector2.DistanceSquared(item2.center, blob5.center) < 200f)
					{
						blob5.Update(item2);
						flag = true;
						arrayList2.Remove(blob5);
					}
				}
				if (!flag)
				{
					item2.SetScale(1.1f);
					arrayList3.Add(item2);
				}
			}
			foreach (Blob item3 in arrayList2)
			{
				item3.lineCount = Math.Min(item3.lineCount, linesToDrawCount);
				item3.BlobMarkForRemove = true;
			}
			foreach (Blob item4 in arrayList3)
			{
				blobs.Add(item4);
			}
		}

		public void UpdateBlobsMarkForRemove(GameTime gameTime)
		{
			ArrayList arrayList = (ArrayList)blobs.Clone();
			foreach (Blob item in arrayList)
			{
				if (item.BlobMarkForRemove)
				{
					if (item.BlobRemoveCount < 0 || item.BlobRemoveTimer > Game.Instance.Config.BlobTimeout)
					{
						item.BlobRemoved = true;
						blobs.Remove(item);
					}
					item.BlobRemoveCount -= 2;
					item.BlobRemoveTimer += gameTime.ElapsedGameTime.TotalSeconds;
				}
				else
				{
					item.BlobRemoveCount++;
				}
			}
		}

		public Blob FindBlob(int x, int y)
		{
			Blob blob = new Blob(this);
			blob.xMin = 1000f;
			blob.xMax = -1000f;
			blob.yMin = 1000f;
			blob.yMax = -1000f;
			blob.lineCount = 0;
			ComputeEdgeVector(blob, x, y);
			if (blob.xMin >= 1000f || blob.xMax <= -1000f || blob.yMin >= 1000f || blob.yMax <= -1000f)
			{
				return null;
			}
			blob.Update();
			return blob;
		}

		private void ComputeEdgeVector(Blob blob, int x, int y)
		{
			int num = x + resx * y;
			if (gridVisited[num])
			{
				return;
			}
			gridVisited[num] = true;
			int squareIndex = GetSquareIndex(x, y);
			float num2 = (float)x * stepx;
			float num3 = (float)y * stepy;
			int num4 = 0;
			int num5;
			while ((num5 = MetaBallsTable.edgeCut[squareIndex, num4++]) != -1)
			{
				int num6 = MetaBallsTable.edgeOffsetInfo[num5, 0];
				int num7 = MetaBallsTable.edgeOffsetInfo[num5, 1];
				int num8 = MetaBallsTable.edgeOffsetInfo[num5, 2];
				if (blob.lineCount < Blob.MAX_NBLINE)
				{
					lineToDraw[linesToDrawCount++] = (blob.line[blob.lineCount++] = voxel[x + num6 + resx * (y + num7)] + num8);
					continue;
				}
				return;
			}
			int num9 = MetaBallsTable.edgeToCompute[squareIndex];
			float num10 = 0f;
			float num11 = 0f;
			if (num9 > 0)
			{
				if ((num9 & 1) > 0)
				{
					num10 = (isovalue - gridValue[num]) / (gridValue[num + 1] - gridValue[num]);
					num11 = num2 * (1f - num10) + num10 * (num2 + stepx);
					edgeVectors[voxel[num]].X = num11;
					if (num11 < blob.xMin)
					{
						blob.xMin = num11;
					}
					if (num11 > blob.xMax)
					{
						blob.xMax = num11;
					}
				}
				if ((num9 & 2) > 0)
				{
					num10 = (isovalue - gridValue[num]) / (gridValue[num + resx] - gridValue[num]);
					num11 = num3 * (1f - num10) + num10 * (num3 + stepy);
					edgeVectors[voxel[num] + 1].Y = num11;
					if (num11 < blob.yMin)
					{
						blob.yMin = num11;
					}
					if (num11 > blob.yMax)
					{
						blob.yMax = num11;
					}
				}
			}
			byte b = MetaBallsTable.neighborVoxel[squareIndex];
			if (x < resx - 2 && (b & 1) > 0)
			{
				ComputeEdgeVector(blob, x + 1, y);
			}
			if (x > 0 && (b & 2) > 0)
			{
				ComputeEdgeVector(blob, x - 1, y);
			}
			if (y < resy - 2 && (b & 4) > 0)
			{
				ComputeEdgeVector(blob, x, y + 1);
			}
			if (y > 0 && (b & 8) > 0)
			{
				ComputeEdgeVector(blob, x, y - 1);
			}
		}
	}
}
