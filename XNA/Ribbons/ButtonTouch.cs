using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ribbons
{
	public class ButtonTouch
	{
		public static float ANIM_TIME = 0.5f;

		private float elapsedTime;

		private Vector2 startPt;

		public ButtonTouch(Vector2 _startPt)
		{
			startPt = _startPt;
		}

		public bool Update(GameTime gameTime)
		{
			elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (elapsedTime > ANIM_TIME)
			{
				return false;
			}
			return true;
		}

		public void Draw(GraphicsDevice device)
		{
			float num = Util.Map(elapsedTime, 0f, ANIM_TIME, 0f, 75f);
			Color color = new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)Util.Map(elapsedTime, 0f, ANIM_TIME, 200f, 0f));
			RenderUtil.DrawRing(device, startPt, num, num + 3f, color, color);
		}
	}
}
