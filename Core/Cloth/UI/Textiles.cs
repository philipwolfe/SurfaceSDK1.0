//---------------------------------------------------------------------
// <copyright file="Textiles.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
using System.Collections.Generic;

using Microsoft.Surface.Core;
using CoreInteractionFramework;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TextileManipulation;

namespace Cloth.UI
{
    /// <summary>
    /// This class encapsulates a TextileManipulationComponent as a UIElement.
    /// </summary>
    public class Textiles : UIElement
    {
        private readonly TextileManipulationComponent textileComponent;
        private Dictionary<int, Vector2> activeContacts;

        private const int flagCount = 3;
        private readonly Textile[] textiles = new Textile[flagCount];

        // Flag Textures.
        private Texture2D[] textures;
        private readonly string[] textureFiles = {
                                            @"Bright\SuperStock_1320R-18019.jpg",
                                            @"Bright\SuperStock_1320R-18018.jpg",
                                            @"Bright\SuperStock_1598R-114495.jpg",
                                            @"Coarse\SuperStock_1663R-11586.jpg",
                                            @"Coarse\SuperStock_1555R-283008.jpg",
                                            @"Coarse\SuperStock_1765R-11342.jpg",
                                            @"Denim\SuperStock_1663R-12849.jpg",
                                            @"Denim\SuperStock_1647R-134259.jpg",
                                            @"Denim\SuperStock_1647R-95365.jpg",
                                            @"Fur\SuperStock_1765R-11314.jpg",
                                            @"Fur\SuperStock_1555R-283021.jpg",
                                            @"Fur\SuperStock_1555R-283068.jpg",
                                            @"Natural\SuperStock_1436R-67079.jpg",
                                            @"Natural\SuperStock_1555R-283016.jpg",
                                            @"Natural\SuperStock_1663R-12064.jpg",
                                            @"Ribbed\SuperStock_1555R-283055.jpg",
                                            @"Ribbed\SuperStock_1436R-67048.jpg",
                                            @"Ribbed\SuperStock_1555R-283062.jpg",
                                            @"Satin\SuperStock_1647R-29879.jpg",
                                            @"Satin\SuperStock_1569R-17086.jpg",
                                            @"Satin\SuperStock_1647R-134263.jpg",
                                            @"Skin\SuperStock_1663R-12090.jpg",
                                            @"Skin\SuperStock_1491R-1013400.jpg",
                                            @"Skin\SuperStock_1647R-95386.jpg",
                                            @"Soft\SuperStock_1525R-54075.jpg",
                                            @"Soft\SuperStock_1433R-935522_1.jpg",
                                            @"Soft\SuperStock_1569R-17060.jpg",
                                            @"Weave\SuperStock_1555R-283061.jpg",
                                            @"Weave\SuperStock_1433R-935529.jpg",
                                            @"Weave\SuperStock_1555R-283046.jpg",
                                        };

        // Icon Textures.
        private Texture2D[] icons;
        private readonly string[] iconFiles = {
                                            @"Bright\Icon\SuperStock_1320R-18019.jpg",
                                            @"Coarse\Icon\SuperStock_1663R-11586.jpg",
                                            @"Denim\Icon\SuperStock_1663R-12849.jpg",
                                            @"Fur\Icon\SuperStock_1765R-11314.jpg",
                                            @"Natural\Icon\SuperStock_1436R-67079.jpg",
                                            @"Ribbed\Icon\SuperStock_1555R-283055.jpg",
                                            @"Satin\Icon\SuperStock_1647R-29879.jpg",
                                            @"Skin\Icon\SuperStock_1663R-12090.jpg",
                                            @"Soft\Icon\SuperStock_1525R-54075.jpg",
                                            @"Weave\Icon\SuperStock_1555R-283061.jpg",
                                        };

        private const int themeSize = flagCount;

        /// <summary>
        ///  Textile theme enumeration.
        /// </summary>
        /// <remarks>
        /// Each theme is composed of 3 (themeSize) textures
        /// </remarks>
        public enum Theme
        {
            Bright, Coarse, Denim, Fur, Natural, Ribbed, Satin, Skin, Soft, Weave
        }


        private Theme currentTheme;

        /// <summary>
        /// Read-write property that gets or sets the current textile theme.
        /// </summary>
        public Theme CurrentTheme
        {
            get { return currentTheme; }
            set
            {
                if (currentTheme != value)
                {
                    currentTheme = value;
                    UpdateTextures(currentTheme);
                }
            }
        }

        /// <summary>
        /// Creates a Textiles UIElement.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="controller"></param>
        /// <param name="contactTarget"></param>
        public Textiles(Game game,
                        UIController controller)
            : base(game, controller, null, null, 0, 0, null)
        {
            textileComponent = new TextileManipulationComponent(game);
            game.Components.Add(textileComponent);

            StateMachine = new TextilesStateMachine(controller, this) { Tag = this };
        }

        /// <summary>
        /// Read-only property that gets the TextileManipulationCompoent for this UIElement.
        /// </summary>
        public TextileManipulationComponent TextileComponent
        {
            get { return textileComponent; }
        }

        /// <summary>
        /// Read-only property that gets the ActiveContacts for this UIElement.
        /// </summary>
        public Dictionary<int, Vector2> ActiveContacts
        {
            get { return activeContacts; }
        }


        /// <summary>
        /// Get or set this textile's DrawOrder.
        /// </summary>
        /// <remarks>
        /// Must set the DrawOrder for the underlying TextileManipulationComponent that
        /// is actually registered in Game.Components.
        /// This property hides the base DrawOrder property which is not virtual.
        /// </remarks>
        public new int DrawOrder
        {
            get
            {
                return base.DrawOrder;
            }
            set
            {
                textileComponent.DrawOrder = value;
                base.DrawOrder = value;
            }
        }


        /// <summary>
        /// Pass through wrapper for TextileManipulationComponent.SetActiveContacts().
        /// </summary>
        /// <param name="contacts">Dictionary of contactId and WorldVectors representing
        /// contacts not already captured by another UIElement.</param>
        public void SetActiveContacts(Dictionary<int, Vector2> contacts)
        {
            // Keep a reference.
            activeContacts = contacts;
            textileComponent.SetActiveContacts(contacts);
        }

        /// <summary>
        /// Transform x and y screen coordinates into the world of the textileComponent.
        /// </summary>
        /// <remarks>
        /// Since the WorldMatrix transform is only applied when the mesh are rendered
        /// to the screen, the correct transform is to apply the inverse WorldMatrix
        /// to get back to the space in which the textileComponent operates.</remarks>
        /// <param name="x">The X screen coordinate.</param>
        /// <param name="y">The Y screen coordinate</param>
        /// <returns>A Vector2 containing coordinates in the textileComponent space.</returns>
        public Vector2 WorldVector(float screenX, float screenY)
        {
            Vector2 contact = new Vector2(screenX, screenY);
            return Vector2.Transform(contact, Matrix.Invert(textileComponent.WorldMatrix));
        }

        /// <summary>
        /// Updates the WorldMatrix of the textileComponent as the meshCanvas background is scrolled.
        /// </summary>
        /// <param name="delta">A vector representing the translation of the meshCavanse backround
        /// during one update cycle.</param>
        public void Translate(Vector2 delta)
        {
            Matrix translation = Matrix.CreateTranslation(delta.X, delta.Y, 0.0f);
            textileComponent.WorldMatrix *= translation;
        }

        /// <summary>
        /// Gets the icon texture for a textile theme.
        /// </summary>
        /// <param name="theme">The selected textile theme./param>
        /// <returns>A Texture2D containing the icon image for the selected theme.</returns>
        public Texture2D GetIcon(Theme theme)
        {
            int index = (int) theme;
            if (index >= 0 && index < icons.Length)
            {
                return icons[index];
            }

            return null;
        }

        /// <summary>
        /// Loads the flag and icon texture files.
        /// </summary>
        private void LoadTextures()
        {

            textures = new Texture2D[textureFiles.Length];
            icons = new Texture2D[iconFiles.Length];

            for (int i = 0; i < textures.Length; i++)
            {
                string file = @"Content\" + textureFiles[i];
                textures[i] = Texture2D.FromFile(GraphicsDevice, file);
            }

            for (int i = 0; i < icons.Length; i++)
            {
                string file = @"Content\" + iconFiles[i];
                icons[i] = Texture2D.FromFile(GraphicsDevice, file);
            }

        }

        /// <summary>
        /// Loads textile content.
        /// </summary>
        protected override void LoadContent()
        {
            GraphicsDevice.PresentationParameters.AutoDepthStencilFormat = DepthFormat.Depth32;
            GraphicsDevice.PresentationParameters.EnableAutoDepthStencil = true;

            textileComponent.BackgroundTexture = Texture2D.FromFile(GraphicsDevice, @"Content\Blank.png");

            Vector2 screenSize = new Vector2(Game.Window.ClientBounds.Width, 
                                             Game.Window.ClientBounds.Height);

            // Create and position the textiles using some hand-picked values.

            const int cols = 32;  // number of stitches horizontally
            const int rows = 24;  // number of stitches vertically

            textiles[0] = new Textile(cols, rows,                         
                                      new Vector2(512, 368),               // center
                                      0.6f,                                // scale
                                      -10,                                 // Orientation
                                      new Vector3(0.65f, 0.24f, 0.1f),     // startColor
                                      new Vector3(0.45f, 0.76f, 0.5f),     // endColor
                                      screenSize);

            textiles[1] = new Textile(cols, rows,
                                      new Vector2(680, 500),
                                      1.2f,
                                      15,
                                      new Vector3(0.24f, 0.5f, 0.65f),
                                      new Vector3(0.20f, 0.9f, 0.65f),
                                      screenSize);

            textiles[2] = new Textile(cols, rows,
                                      new Vector2(600, 800),
                                      1.1f, 
                                      38,
                                      new Vector3(0.1f, 0.0f, 0.74f),
                                      new Vector3(0.1f, 0.9f, 0.74f),
                                      screenSize);

            LoadTextures();

            // Initialize each flag and add it to the textileComponent manager.
            for (int i = 0; i < textiles.Length; i++)
            {
                textiles[i].Texture = textures[(int)CurrentTheme + i];
                textiles[i].RenderStyle = RenderStyles.Texture;
                textileComponent.Textiles.Add(textiles[i]);
            }
        }
      

        /// <summary>
        /// Updates each mesh with one of the textures from the selected theme.
        /// </summary>
        /// <param name="theme"></param>
        public void UpdateTextures(Theme theme)
        {
            foreach (Textile textile in textileComponent.Textiles)
            {
                // The theme determines the group and the id selects an element in that group.
                int index = (int)theme * themeSize + textile.Id % themeSize;
                textile.Texture = textures[index];
            }
        }


        /// <summary>
        /// Overrides Draw method from UIElement.
        /// Prevents base.draw(gameTime) from beeing called.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            // Do not draw this element.
            // The encapsulated TextileManipulation component does the drawing.
        }

        #region Contact Tracking and Hit Test

        /// <summary>
        /// Hit test the textile components.
        /// </summary>
        /// <param name="contact">The contact to hit test.</param>
        /// <param name="captured">Boolean indicating if the contact was previously captured.</param>
        /// <returns>Returns true if the contact hits a textile component.</returns>
        public override bool HitTest(Contact contact, bool captured)
        {
            return textileComponent.HitTest(WorldVector(contact.CenterX, contact.CenterY));
        }

        #endregion

        #region Textile Component Event Handlers

        internal void OnContactTapGesture(object sender, ContactEventArgs args)
        {
            Vector2 worldVector;
            if (activeContacts.TryGetValue(args.Contact.Id, out worldVector))
            {
                textileComponent.ContactTap(worldVector);
            }
        }

        #endregion

    }
}
