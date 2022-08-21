namespace Ribbons
{
	public class MetaBallsTable
	{
		public static int[,] edgeCut = new int[16, 5]
		{
			{ -1, -1, -1, -1, -1 },
			{ 0, 3, -1, -1, -1 },
			{ 0, 1, -1, -1, -1 },
			{ 3, 1, -1, -1, -1 },
			{ 1, 2, -1, -1, -1 },
			{ 1, 2, 0, 3, -1 },
			{ 0, 2, -1, -1, -1 },
			{ 3, 2, -1, -1, -1 },
			{ 3, 2, -1, -1, -1 },
			{ 0, 2, -1, -1, -1 },
			{ 1, 2, 0, 3, -1 },
			{ 1, 2, -1, -1, -1 },
			{ 3, 1, -1, -1, -1 },
			{ 0, 1, -1, -1, -1 },
			{ 0, 3, -1, -1, -1 },
			{ -1, -1, -1, -1, -1 }
		};

		public static int[,] edgeOffsetInfo = new int[4, 3]
		{
			{ 0, 0, 0 },
			{ 1, 0, 1 },
			{ 0, 1, 0 },
			{ 0, 0, 1 }
		};

		public static int[] edgeToCompute = new int[16]
		{
			0, 3, 1, 2, 0, 3, 1, 2, 2, 1,
			3, 0, 2, 1, 3, 0
		};

		public static byte[] neighborVoxel = new byte[16]
		{
			0, 10, 9, 3, 5, 15, 12, 6, 6, 12,
			12, 5, 3, 9, 10, 0
		};

		public static void computeNeighborTable()
		{
			for (int i = 0; i < 16; i++)
			{
				neighborVoxel[i] = 0;
				int num = 0;
				int num2;
				while ((num2 = edgeCut[i, num++]) != -1)
				{
					switch (num2)
					{
					case 0:
						neighborVoxel[i] |= 8;
						break;
					case 1:
						neighborVoxel[i] |= 1;
						break;
					case 2:
						neighborVoxel[i] |= 4;
						break;
					case 3:
						neighborVoxel[i] |= 2;
						break;
					}
				}
			}
		}
	}
}
