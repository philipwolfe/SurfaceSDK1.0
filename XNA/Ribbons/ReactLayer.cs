using System.Collections;
using System.Collections.Generic;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ribbons
{
	public class ReactLayer
	{
		private BasicEffect basicEffect;

		private SpriteBatch spriteBatch;

		private Texture2D fingerTexture;

		private Dictionary<int, double> contactTimeMap = new Dictionary<int, double>();

		public void Init(GraphicsDevice device)
		{
			basicEffect = new BasicEffect(device, null);
			basicEffect.LightingEnabled = false;
			basicEffect.VertexColorEnabled = true;
			basicEffect.World = Game.Instance.World;
			basicEffect.View = Game.Instance.View;
			basicEffect.Projection = Game.Instance.Projection;
			spriteBatch = new SpriteBatch(device);
			fingerTexture = Game.Instance.ContentManager.Load<Texture2D>("Content/ReactFinger");
		}

		public void Update(GameTime gameTime)
		{
			ReadOnlyContactCollection state = Game.Instance.ContactTarget.GetState();
			ArrayList arrayList = new ArrayList();
			arrayList.AddRange(contactTimeMap.Keys);
			foreach (Contact item in state)
			{
				double value = 0.0;
				if (contactTimeMap.TryGetValue(item.Id, out value))
				{
					contactTimeMap[item.Id] = value + gameTime.ElapsedGameTime.TotalSeconds;
					arrayList.Remove(item.Id);
				}
				else
				{
					contactTimeMap[item.Id] = 0.0;
				}
			}
			foreach (int item2 in arrayList)
			{
				contactTimeMap.Remove(item2);
			}
		}

		public void Draw(GraphicsDevice device)
		{
			ReadOnlyContactCollection state = Game.Instance.ContactTarget.GetState();
			basicEffect.Begin();
			foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
			{
				pass.Begin();
				spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
				foreach (Contact item in state)
				{
					double value = 0.0;
					if (contactTimeMap.TryGetValue(item.Id, out value) && item.IsFingerRecognized)
					{
						float num = Util.Constrain((float)value, 0f, 0.25f);
						Color color = new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)Util.Map(num * 4f, 0f, 1f, 0f, 255f));
						float scale = Util.Map(num * 4f, 0f, 1f, 0f, 0.75f);
						spriteBatch.Draw(fingerTexture, new Vector2(item.X, item.Y), null, color, 0f, new Vector2(64f, 64f), scale, SpriteEffects.None, 0f);
					}
				}
				spriteBatch.End();
				pass.End();
			}
			basicEffect.End();
		}
	}
}
