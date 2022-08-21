using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ribbons
{
	internal class Splat
	{
		private RibbonSplat ribbon;

		private Vector2 pt = default(Vector2);

		private float rotation;

		private float scale;

		public Splat(RibbonSplat _ribbon, Vector2 _pt, float _rotation, float _scale)
		{
			ribbon = _ribbon;
			pt = _pt;
			rotation = _rotation;
			scale = _scale;
		}

		public void Draw(GraphicsDevice device, Effect effect, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Game.Instance.SprayTexture, pt, null, ribbon.Color, rotation, new Vector2(128f, 128f), scale, SpriteEffects.None, 0f);
		}
	}
}
