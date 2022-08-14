using System;
using System.Collections.Generic;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using CoreInteractionFramework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XnaScatter
{
    /// <summary>
    /// This is the main type for your application.
    /// </summary>
    public class App1: Microsoft.Xna.Framework.Game
    {
        private UIController controller;

        private Random r = new Random();
        private readonly GraphicsDeviceManager graphics;
        private ContactTarget contactTarget;
        private bool applicationLoadCompleteSignalled;
        private Matrix screenTransform = Matrix.Identity;
        private Matrix inverted;
        private UserOrientation currentOrientation;

        private XnaScatterView scatterView;

        private SpriteBatch spriteBatch;

        private readonly List<XnaScatterView> gameObjects = new List<XnaScatterView>();

        // Application state: Activated, Previewed, or Deactivated.
        // Start in Activated state.
        private bool isApplicationActivated = true;
        private bool isApplicationPreviewed;

        /// <summary>
        /// The graphics device manager for the application.
        /// </summary>
        protected GraphicsDeviceManager Graphics
        {
            get { return graphics; }
        }

        /// <summary>
        /// The target receiving all surface input for the application.
        /// </summary>
        protected ContactTarget ContactTarget
        {
            get { return contactTarget; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public App1()
        {
            graphics = new GraphicsDeviceManager(this);
        }

        #region Initialization

        /// <summary>
        /// Populates the game.  Creates an XNAScatterView and five XNAScatterViewItems.
        /// Places the items randomly in the XNAScatterView
        /// </summary>
        private void PopulateGameWorld()
        {
            InteractiveSurface interactiveSurface = InteractiveSurface.DefaultInteractiveSurface;

            int maxHeight = graphics.PreferredBackBufferHeight;
            int maxWidth = graphics.PreferredBackBufferWidth;
            if (interactiveSurface != null)
            {
                maxHeight = interactiveSurface.Height;
                maxWidth = interactiveSurface.Width;
            }

            scatterView = new XnaScatterView(controller, "Canvas.jpg", 0, maxHeight, 0, maxWidth);
            scatterView.Center = new Vector2(maxWidth / 2, maxHeight / 2);

            // Item 1 - Translate, Rotate
            XnaScatterViewItem item1 = new XnaScatterViewItem(controller, "Card01.png", scatterView);
            item1.CanTranslateFlick = false;
            item1.CanRotateFlick = false;
            item1.CanScale = false;
            item1.CanScaleFlick = false;
            item1.Center = new Vector2(r.Next(maxWidth), r.Next(maxHeight));
            scatterView.AddItem(item1);

            // Item 2 
            XnaScatterViewItem item2 = new XnaScatterViewItem(controller, "Card02.png", scatterView);
            item2.CanRotate = false;
            item2.CanRotateFlick = false;
            item2.CanScaleFlick = false;
            item2.Center = new Vector2(r.Next(maxWidth), r.Next(maxHeight));
            scatterView.AddItem(item2);

            // Item 3
            XnaScatterViewItem item3 = new XnaScatterViewItem(controller, "Card04.png", scatterView);
            item3.CanRotate = false;
            item3.CanRotateFlick = false;
            item3.Center = new Vector2(r.Next(maxWidth), r.Next(maxHeight));
            scatterView.AddItem(item3);

            // Item 4
            XnaScatterViewItem item4 = new XnaScatterViewItem(controller, "Card03.png", scatterView);
            item4.CanScale = false;
            item4.CanScaleFlick = false;
            item4.Center = new Vector2(r.Next(maxWidth), r.Next(maxHeight));
            scatterView.AddItem(item4);

            // Item 5
            XnaScatterViewItem item5 = new XnaScatterViewItem(controller, "Card05.png", scatterView);
            item5.Center = new Vector2(r.Next(maxWidth), r.Next(maxHeight));
            scatterView.AddItem(item5);

            gameObjects.Add(scatterView);
        }

        /// <summary>
        /// Moves and sizes the window to cover the input surface.
        /// </summary>
        private void SetWindowOnSurface()
        {
            System.Diagnostics.Debug.Assert(Window.Handle != System.IntPtr.Zero,
                "Window initialization must be complete before SetWindowOnSurface is called");

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
            System.Diagnostics.Debug.Assert(contactTarget == null,
                "Surface input already initialized");

            // Create a target for surface input.
            contactTarget = new ContactTarget(Window.Handle, EventThreadChoice.OnBackgroundThread);
            contactTarget.EnableInput();

            controller = new UIController(contactTarget, HitTestCallback);
        }

        /// <summary>
        /// Reset the application's orientation and transform based on the current launcher orientation.
        /// </summary>
        private void ResetOrientation()
        {
            UserOrientation newOrientation = ApplicationLauncher.Orientation;

            if (newOrientation == currentOrientation) { return; }

            currentOrientation = newOrientation;

            if (currentOrientation == UserOrientation.Top)
            {
                screenTransform = inverted;
            }
            else
            {
                screenTransform = Matrix.Identity;
            }

            if (scatterView != null)
            {
                scatterView.Transform = screenTransform;
            }
        }

        #endregion

        #region Overridden Game Methods

        /// <summary>
        /// Allows the app to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            SetWindowOnSurface();
            InitializeSurfaceInput();

            // Subscribe to surface application activation events
            ApplicationLauncher.ApplicationActivated += OnApplicationActivated;
            ApplicationLauncher.ApplicationPreviewed += OnApplicationPreviewed;
            ApplicationLauncher.ApplicationDeactivated += OnApplicationDeactivated;

            // Create a rotation matrix to orient the screen so it is viewed correctly,
            // when the user orientation is 180 degress different.
            Matrix rotation = Matrix.CreateRotationZ(MathHelper.ToRadians(180));
            Matrix translation = Matrix.CreateTranslation(graphics.GraphicsDevice.Viewport.Width,
                                                          graphics.GraphicsDevice.Viewport.Height, 0);
            inverted = rotation * translation;

            PopulateGameWorld();

            currentOrientation = UserOrientation.Bottom;
            screenTransform = Matrix.Identity;
            scatterView.Transform = screenTransform;

            base.Initialize();
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            string filename = System.Windows.Forms.Application.ExecutablePath;
            string path = System.IO.Path.GetDirectoryName(filename) + "\\Resources\\";

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            foreach (XnaScatterView gameObject in gameObjects)
            {
                gameObject.LoadContent(graphics.GraphicsDevice, path);
            }
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
            base.Update(gameTime);

            if (isApplicationActivated || isApplicationPreviewed)
            {
                // Let the UIController process new contacts and changes to old contacts
                controller.Update();

                // All contacts have been processed, have all children process their manipulations
                foreach (XnaScatterView child in gameObjects)
                {
                    child.ProcessContacts();
                }
              
            }
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
                applicationLoadCompleteSignalled = true;
                ApplicationLauncher.SignalApplicationLoadComplete();
            }

            graphics.GraphicsDevice.Clear(Color.Black);

            // Prepare the graphics device for rendering.
            // Pass in the screenTransform to orient the display correctly.
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.BackToFront, SaveStateMode.None, screenTransform);

            // draw all the shapes in the list
            foreach (XnaScatterView gameObject in gameObjects)
            {
                gameObject.Draw(spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion

        #region Hit Testing

        /// <summary>
        /// Used by the UIController to determine what IContactableObject is being touched by a given contact.
        /// </summary>
        /// <param name="uncapturedContacts">All contacts touching the app that have not been captured.</param>
        /// <param name="capturedContacts">All contacts touching the app that have already been captured.</param>
        private void HitTestCallback(ReadOnlyHitTestResultCollection uncapturedContacts, 
                                     ReadOnlyHitTestResultCollection capturedContacts)
        {
            // Hit test and assign all new contacts
            foreach (HitTestResult result in uncapturedContacts)
            {
                // Hit test the contact to determine which object it is touching
                UIElementStateMachine touched = HitTest(result.Contact);

                if (touched != null)
                {
                    // Only using state machines to do capture, not to track contact position inside the object, 
                    // so just use (0, 0)
                    XnaScatterHitTestDetails details = new XnaScatterHitTestDetails(0, 0);

                    // Set the hit test details
                    result.SetUncapturedHitTestInformation(touched, details);
                }
                else
                {
                    // Must call SetUncapturedHitTestInformation, but since nothing was touched, pass null
                    result.SetUncapturedHitTestInformation(null, null);
                }
            }

            // Hit test all previously captured contacts
            foreach (HitTestResult result in capturedContacts)
            {
                // Only using state machines to do capture, not to track contact position inside the object, 
                // so just use (0, 0)
                XnaScatterHitTestDetails details = new XnaScatterHitTestDetails(0, 0);

                // Set the hit test details
                result.SetCapturedHitTestInformation(true, details);
            }
        }

        /// <summary>
        /// Compare the contact's location to the XNAScatterView to see if the contact
        /// is touching an XNAScatterView item or the scatter view itself
        /// </summary>
        /// <param name="c">The contact to be hit tested</param>
        private UIElementStateMachine HitTest(Contact contact)
        {
            // Hit test against scatterView, it will test against its children
            return scatterView.HitTest(contact);
        }

        #endregion

        #region Application Event Handlers

        /// <summary>
        /// This is called when application has been activated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationActivated(object sender, EventArgs e)
        {
            isApplicationActivated = true;
            isApplicationPreviewed = false;
            ResetOrientation();
        }

        /// <summary>
        /// This is called when application is in preview mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationPreviewed(object sender, EventArgs e)
        {
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
            isApplicationActivated = false;
            isApplicationPreviewed = false;
        }

        #endregion

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            if (disposing) 
            {
                IDisposable graphicsDispose = graphics as IDisposable;
                if (graphicsDispose != null)
                {
                    graphicsDispose.Dispose();
                }
                if (spriteBatch != null)
                {
                    spriteBatch.Dispose();
                    spriteBatch = null;
                }
                if (contactTarget != null)
                {
                    contactTarget.Dispose();
                    contactTarget = null;
                }
                if (scatterView != null)
                {
                    scatterView.Dispose();
                    scatterView = null;
                }
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}

