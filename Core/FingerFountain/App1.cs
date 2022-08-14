using System;
using System.Collections.Generic;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FingerFountain
{
    /// <summary>
    /// This sample demonstrates a very simple drawing technique.
    /// </summary>
    public class App1 : Microsoft.Xna.Framework.Game
    {
        private readonly GraphicsDeviceManager graphics;
        private ContactTarget contactTarget;
        private bool applicationLoadCompleteSignalled;
        private const int millisecondsToDisappear = 3000;
        private SpriteBatch foregroundBatch;
        private Texture2D contactSprite;
        private Vector2 spriteOrigin;
        private LinkedList<SpriteData> sprites = new LinkedList<SpriteData>();

        // application state: Activated, Previewed, Deactivated,
        // start in Activated state
        private bool isApplicationActivated = true;
        private bool isApplicationPreviewed;

        #region FingerFountain Sprites

        /// <summary>
        /// Reduces the scale of each sprite in the sprites list by
        /// the specified amount.
        /// </summary>
        /// <param name="shrinkBy">amount to shrink by</param>
        private void ShrinkSprites(float shrinkBy)
        {
            // go through the whole list and decrement the scale value
            LinkedListNode<SpriteData> currentNode = sprites.First;
            while (currentNode != null)
            {
                currentNode.Value.Scale -= shrinkBy;
                currentNode = currentNode.Next;
            }
        }

        /// <summary>
        /// Removes any sprites with a scale of zero or lower.
        /// </summary>
        private void RemoveInvisibleSprites()
        {
            // Since the sprites are always added to the end,
            // removals will always come from the beginning.
            while (sprites.First != null && sprites.First.Value.Scale <= 0.0f)
                sprites.RemoveFirst();
        }

        /// <summary>
        /// Creates a new sprite at each contact location.
        /// </summary>
        private int InsertSpritesAtContactPositions(ReadOnlyContactCollection contacts)
        {
            int count = 0;
            foreach (Contact contact in contacts)
            {
                // Create a sprite for each contact that has been recognized as a finger.
                if (contact.IsFingerRecognized)
                {
                    SpriteData sprite = new SpriteData(new Vector2(contact.X, contact.Y),
                        contact.Orientation,
                        1.0f);
                    sprites.AddLast(sprite); // always add to the end
                    count++;
                }
            }
            return count;
        }

        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        public App1()
        {
            graphics = new GraphicsDeviceManager(this);
        }

        /// <summary>
        /// Allows the app to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            IsMouseVisible = true; // easier for debugging not to "lose" mouse
            IsFixedTimeStep = false; // we will update based on time
            SetWindowOnSurface();
            InitializeSurfaceInput();

            // Subscribe to surface application activation events
            ApplicationLauncher.ApplicationActivated += OnApplicationActivated;
            ApplicationLauncher.ApplicationPreviewed += OnApplicationPreviewed;
            ApplicationLauncher.ApplicationDeactivated += OnApplicationDeactivated;

            base.Initialize();
        }

        /// <summary>
        /// Moves and sizes the window to cover the input surface.
        /// </summary>
        private void SetWindowOnSurface()
        {
            System.Diagnostics.Debug.Assert(Window.Handle != System.IntPtr.Zero,
                "Window initialization must be complete before SetWindowOnSurface is called");
            if (Window.Handle == System.IntPtr.Zero)
                return;

            // We don't want to run in full-screen mode because we need
            // overlapped windows, so instead run in windowed mode
            // and resize to take up the whole surface with no border.

            // Make sure the graphics device has the correct back buffer size.
            InteractiveSurface interactiveSurface = InteractiveSurface.DefaultInteractiveSurface;
            if (interactiveSurface != null)
            {
                graphics.PreferredBackBufferWidth = interactiveSurface.Width;
                graphics.PreferredBackBufferHeight = interactiveSurface.Height;
                graphics.ApplyChanges();

                // Remove the border and position the window.
                Program.RemoveBorder(Window.Handle);
                Program.PositionWindow(Window);
            }
        }

        /// <summary>
        /// Initializes the surface input system. This should be called after any window
        /// initialization is done, and should only be called once.
        /// </summary>
        private void InitializeSurfaceInput()
        {
            System.Diagnostics.Debug.Assert(Window.Handle != System.IntPtr.Zero,
                "Window initialization must be complete before InitializeSurfaceInput is called");
            if (Window.Handle == System.IntPtr.Zero)
                return;
            System.Diagnostics.Debug.Assert(contactTarget == null,
                "Surface input already initialized");
            if (contactTarget != null)
                return;

            // Create a target for surface input.
            contactTarget = new Microsoft.Surface.Core.ContactTarget(Window.Handle, EventThreadChoice.OnBackgroundThread);
            contactTarget.EnableInput();
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            string filename = System.Windows.Forms.Application.ExecutablePath;
            string path = System.IO.Path.GetDirectoryName(filename) + "\\FingerFountainContent\\";

            foregroundBatch = new SpriteBatch(graphics.GraphicsDevice);
            contactSprite = Texture2D.FromFile(graphics.GraphicsDevice,
                path + "sprite.png");
            spriteOrigin = new Vector2((float)contactSprite.Width / 2.0f,
                (float)contactSprite.Height / 2.0f);
        }
        
        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
        }
        
        /// <summary>
        /// Allows the app to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (isApplicationActivated || isApplicationPreviewed)
            {
                // get the current state
                ReadOnlyContactCollection contacts = contactTarget.GetState();

                // first update the state of any existing sprites
                ShrinkSprites((float)gameTime.ElapsedRealTime.Milliseconds /
                    (float)millisecondsToDisappear);

                // next update the sprites list with new additions
                InsertSpritesAtContactPositions(contacts);

                // finally remove any invisible sprites
                RemoveInvisibleSprites();
            }

            base.Update(gameTime);
        }
        
        /// <summary>
        /// This is called when the app should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (!applicationLoadCompleteSignalled)
            {
                // Dismiss the loading screen now that we are starting to draw
                ApplicationLauncher.SignalApplicationLoadComplete();
                applicationLoadCompleteSignalled = true;
            }

            graphics.GraphicsDevice.Clear(Color.Black);

            foregroundBatch.Begin();
            // draw all the sprites in the list
            foreach (SpriteData sprite in sprites)
            {
                foregroundBatch.Draw(contactSprite, sprite.Location, null, Color.White,
                    sprite.Orientation, spriteOrigin, sprite.Scale, SpriteEffects.None, 0f);
            }
            foregroundBatch.End();

            base.Draw(gameTime);
        }


        /// <summary>
        /// This is called when application has been activated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationActivated(object sender, EventArgs e)
        {
            // update application state
            isApplicationActivated = true;
            isApplicationPreviewed = false;
        }

        /// <summary>
        /// This is called when application is in preview mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationPreviewed(object sender, EventArgs e)
        {
            // update application state
            isApplicationActivated = false;
            isApplicationPreviewed = true;
        }

        /// <summary>
        ///  This is called when application has been deactivated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationDeactivated(object sender, EventArgs e)
        {
            // update application state
            isApplicationActivated = false;
            isApplicationPreviewed = false;
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources.              
                contactSprite.Dispose();
                foregroundBatch.Dispose();
                contactTarget.Dispose();

                IDisposable graphicsDispose = graphics as IDisposable;
                if (graphicsDispose != null)
                {
                    graphicsDispose.Dispose();
                }
            }

            // Release unmanaged Resources.

            base.Dispose(disposing);
        }

        #endregion       

    }

}
