using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace TextileManipulation
{
    public class TextileManipulationComponent : DrawableGameComponent
	{
        private readonly IList<Textile> textiles = new List<Textile>();
        private readonly IList<Textile> selectedTextiles = new List<Textile>();

        private Texture2D backgroundTexture;
        private SpriteBatch spriteBatch;
        private Rectangle screenRect;
        private Effect effectLines;
        private Effect effectTexture;

        private Dictionary<int, Vector2> activeContacts;
        private Matrix viewProjMatrix;

        public TextileManipulationComponent(Game game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            screenRect = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            viewProjMatrix = Matrix.CreateOrthographicOffCenter(0, screenRect.Width, 0, screenRect.Height, 1f, -1f)
                        * Matrix.CreateScale(1f, -1f, 1f);
            WorldMatrix = Matrix.Identity;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    DisposeGraphics();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected override void LoadContent()
        {
            DisposeGraphics();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            effectLines = LoadEffect(@"TextileManipulationContent\SimpleColorEffect.fx");
            effectTexture = LoadEffect(@"TextileManipulationContent\TexturedEffect.fx");
        }

        private void DisposeGraphics()
        {
            if (spriteBatch != null)
            {
                spriteBatch.Dispose();
                spriteBatch = null;
            }

            if (effectLines != null)
            {
                effectLines.Dispose();
                effectLines = null;
            }

            if (effectTexture != null)
            {
                effectTexture.Dispose();
                effectTexture = null;
            }
        }

        private Effect LoadEffect(string path)
        {
            // TODO : We should use XNB
            CompiledEffect compiled = Effect.CompileEffectFromFile(path, null, null,
                         CompilerOptions.None, TargetPlatform.Windows);
            return new Effect(GraphicsDevice, compiled.GetEffectCode(), CompilerOptions.None, null);
        }

        public IList<Textile> Textiles
        {
            get { return textiles; }
        }

        public IList<Textile> SelectedTextiles
        {
            get { return selectedTextiles; }
        }

        /// <summary>
        /// Get a capturing Textile object for the given contactId
        /// </summary>
        /// <param name="contactId">Id of captured contact</param>
        /// <param name="capturingTextile">Textile which is capturing the contact</param>
        /// <returns></returns>
        public bool TryFindCapturingTextile(int contactId, out Textile capturingTextile)
        {
            foreach (Textile textile in textiles)
            {
                if (textile.IsCapturing(contactId))
                {
                    capturingTextile = textile;
                    return true;
                }
            }

            capturingTextile = null;
            return false;
        }

        /// <summary>
        /// Return true if contact position hits one of the contained textiles.
        /// </summary>
        /// <param name="position">Contact positon in World coordinates.</param>
        /// <returns></returns>
        public bool HitTest(Vector2 position)
        {
            for (int index = textiles.Count - 1; index >= 0; index--)
            {
                if (textiles[index].HitTest(position))
                {
                    return true;
                }
            }
            return false;
        }


        public void ContactAdd(int contactId, Vector2 position)
        {
            Textile targetCloth = null;
            for (int index = textiles.Count - 1; index >= 0; index--)
            {
                Textile textile = textiles[index];
                if (textile.TryAddContact(contactId, position))
                {
                    targetCloth = textile;
                    selectedTextiles.Add(textile);
                    break;
                }
            }

            if (targetCloth != null && textiles.IndexOf(targetCloth) != textiles.Count - 1)
            {
                textiles.Remove(targetCloth);
                textiles.Add(targetCloth);
            }
        }

        public void ContactRemove(int contactId, Vector2 position)
        {
            foreach (Textile textile in textiles)
            {
                textile.RemoveContact(contactId, position);

                if (selectedTextiles.Contains(textile) && textile.CapturedContactCount == 0)
                {
                    selectedTextiles.Remove(textile);
                }
            }
        }

        public void ContactTap(Vector2 position)
        {
            foreach (Textile textile in textiles)
            {
                textile.Ripple(position, TextileConstants.RippleDiameter, -1f);
            }
        }

        public void SetActiveContacts(Dictionary<int, Vector2> contacts)
        {
            activeContacts = contacts;
        }

        /// <summary>
        /// World Matrix for entire Textile canvas
        /// </summary>
        public Matrix WorldMatrix { get; set; }

        /// <summary>
        /// Background texture if there is any.
        /// Client is responsible to dicard this
        /// </summary>
        public Texture2D BackgroundTexture
        {
            get { return backgroundTexture; }
            set { backgroundTexture = value; }
        }

        public override void Update(GameTime gameTime)
        {
            if (activeContacts != null)
            {
                foreach (Textile textile in textiles)
                {
                    textile.Update(activeContacts);
                }
            }

            Matrix worldViewProj = WorldMatrix * viewProjMatrix;
            effectLines.Parameters["WorldViewProj"].SetValue(worldViewProj);
            effectTexture.Parameters["WorldViewProj"].SetValue(worldViewProj);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (backgroundTexture != null)
            {
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                spriteBatch.Draw(backgroundTexture, screenRect, new Color(255, 255, 255, 0));
                spriteBatch.End();
            }

            foreach (Textile textile in textiles)
            {
                textile.Draw(GraphicsDevice, effectLines, effectTexture);
            }

            base.Draw(gameTime);
        }
	}
}
