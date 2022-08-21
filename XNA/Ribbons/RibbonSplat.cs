using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ribbons
{
	public class RibbonSplat : RibbonBase
	{
		private float thickness = 20f;

		private int splatCapacity;

		private int splatMultiply = 10;

		private ArrayList splatLists = new ArrayList();

		public RibbonSplat()
		{
			splatCapacity = (capacity - 1) * 10;
		}

		public override void BuildPolys()
		{
			splatLists.Clear();
			for (int i = 0; i < pathCount - 1; i++)
			{
				AddSplats(path[i], path[i + 1]);
			}
		}

		private void AddSplats(Vector3 p1, Vector3 p2)
		{
			if (p2.Z != 0f)
			{
				float num = p1.Z * thickness;
				float num2 = (p2 - p1).Length();
				if (num2 != 0f)
				{
					num2 = num / num2;
				}
				ArrayList arrayList = new ArrayList();
				splatLists.Add(arrayList);
				for (int i = 0; i < splatMultiply; i++)
				{
					Vector2 pt = Vector2.Lerp(new Vector2(p1.X, p1.Y), new Vector2(p2.X, p2.Y), (float)i / (float)splatMultiply);
					pt = new Vector2((pt.X < 0f) ? ((float)width - (0f - pt.X) % (float)width) : (pt.X % (float)width), (pt.Y < 0f) ? ((float)height - (0f - pt.Y) % (float)height) : (pt.Y % (float)height));
					float scale = Util.Constrain(0.03f * num, 0.2f, 1f);
					float rotation = Util.Rand() * (float)Math.PI * 2f;
					Splat value = new Splat(this, pt, rotation, scale);
					arrayList.Add(value);
				}
			}
		}

		public override void Draw(GraphicsDevice device, Effect effect, SpriteBatch spriteBatch)
		{
			foreach (ArrayList splatList in splatLists)
			{
				foreach (Splat item in splatList)
				{
					item.Draw(device, effect, spriteBatch);
				}
			}
		}

		public override Game.RibbonType GetRibbonType()
		{
			return Game.RibbonType.ERibbonSplat;
		}
	}
}
