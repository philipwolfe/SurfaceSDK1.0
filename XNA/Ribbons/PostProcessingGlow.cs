using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Ribbons
{
	public class PostProcessingGlow
	{
		private SpriteBatch spriteBatch;

		private ResolveTexture2D resolveTarget;

		private RenderTarget2D renderTarget1;

		private RenderTarget2D renderTarget2;

		private GraphicsDevice device;

		private Effect Saturate;

		private Effect GaussianBlur;

		private Effect Combine;

		public PostProcessingGlow(GraphicsDevice device)
		{
			this.device = device;
		}

		public void Initialize()
		{
			PresentationParameters presentationParameter = device.PresentationParameters;
			resolveTarget = new ResolveTexture2D(device, device.Viewport.Width, device.Viewport.Height, 1, device.PresentationParameters.BackBufferFormat);
			renderTarget1 = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, 1, device.DisplayMode.Format);
			renderTarget2 = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, 1, device.DisplayMode.Format);
		}

		public void LoadContent(ContentManager content)
		{
			spriteBatch = new SpriteBatch(device);
			Saturate = content.Load<Effect>("Content/Effects/Saturate");
			GaussianBlur = content.Load<Effect>("Content/Effects/GaussianBlur");
			Combine = content.Load<Effect>("Content/Effects/GlowCombine");
		}

		public void Draw(float factor)
		{
			DepthStencilBuffer depthStencilBuffer = device.DepthStencilBuffer;
			device.DepthStencilBuffer = null;
			device.ResolveBackBuffer(resolveTarget);
			device.SetRenderTarget(0, renderTarget1);
			DrawQuad(resolveTarget, Saturate);
			device.SetRenderTarget(0, renderTarget2);
			GaussianHelper.SetBlurEffectParameters(ref GaussianBlur, factor / (float)device.Viewport.Width, 0f);
			DrawQuad(renderTarget1.GetTexture(), GaussianBlur);
			device.SetRenderTarget(0, renderTarget1);
			GaussianHelper.SetBlurEffectParameters(ref GaussianBlur, 0f, factor / (float)device.Viewport.Height);
			DrawQuad(renderTarget2.GetTexture(), GaussianBlur);
			device.SetRenderTarget(0, null);
			Combine.Parameters["glowFactor"].SetValue(factor);
			device.Textures[1] = renderTarget1.GetTexture();
			DrawQuad(resolveTarget, Combine);
			device.DepthStencilBuffer = depthStencilBuffer;
		}

		private void DrawQuad(Texture2D texture, Effect effect)
		{
			effect.Begin();
			spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Begin();
				spriteBatch.Draw(texture, Vector2.Zero, Color.White);
				pass.End();
			}
			spriteBatch.End();
			effect.End();
		}
	}
}
