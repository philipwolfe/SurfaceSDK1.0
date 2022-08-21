using Microsoft.Xna.Framework;

namespace Ribbons
{
	public class Metaballs2D
	{
		protected float isovalue;

		protected int resx;

		protected int resy;

		protected float stepx;

		protected float stepy;

		protected float[] gridValue;

		protected int gridValueCount;

		protected int[] squareIndices;

		protected int[] voxel;

		protected int voxelCount;

		protected Vector2[] edgeVectors;

		protected int edgeVectorCount;

		public int[] lineToDraw;

		public int linesToDrawCount;

		public void Init(int resx, int resy)
		{
			this.resx = resx;
			this.resy = resy;
			stepx = 1f / (float)(resx - 1);
			stepy = 1f / (float)(resy - 1);
			gridValueCount = resx * resy;
			gridValue = new float[gridValueCount];
			squareIndices = new int[gridValueCount];
			voxelCount = gridValueCount;
			voxel = new int[voxelCount];
			edgeVectors = new Vector2[2 * voxelCount];
			edgeVectorCount = 2 * voxelCount;
			lineToDraw = new int[2 * voxelCount];
			linesToDrawCount = 0;
			int num = 0;
			for (int i = 0; i < resx; i++)
			{
				for (int j = 0; j < resy; j++)
				{
					int num2 = 2 * num;
					voxel[i + resx * j] = num2;
					edgeVectors[num2] = new Vector2((float)i * stepx, (float)j * stepy);
					edgeVectors[num2 + 1] = new Vector2((float)i * stepx, (float)j * stepy);
					num++;
				}
			}
		}

		public void SetIsovalue(float iso)
		{
			isovalue = iso;
		}
	}
}
