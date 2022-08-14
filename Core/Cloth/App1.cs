using System;
using System.Collections.Generic;

using Microsoft.Surface;
using Microsoft.Surface.Core;
using CoreInteractionFramework;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Cloth.UI;

namespace Cloth
{
    /// <summary>
    /// This is the main class for the cloth application.
    /// </summary>
    public class App1 : Game
    {
        private GraphicsDeviceManager graphics;

        // Application state: Activated, Previewed, Deactivated.
        // Start in Activated state.
        private bool isApplicationActivated = true;
        private bool isApplicationPreviewed;

        private bool applicationLoadCompleteSignalled;

        // This orients batches of sprites.
        private UserOrientation currentOrientation;
        private Matrix screenTransform = Matrix.Identity;
        private Matrix inverted;

        // The UI controller.
        private UIController controller;
        private ContactTarget contactTarget;

        // Top-Level UIElements
        private MeshCanvas meshCanvas;       // Background ScrollViewer.
        private Textiles textiles;           // Contains the cloth simulation component.

        private UIContainer hudContainer;    // Container for Heads Up Display (HUD) elements.

        private ListBox listbox;             // HUD Control. Consists of listbox, button, and scrollbar.

        // Captures contacts that should be passed on to the cloth simulation component.
        private Dictionary<int, Vector2> activeContacts = new Dictionary<int, Vector2>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public App1()
        {
            graphics = new GraphicsDeviceManager(this);
            System.Diagnostics.Debug.Assert(graphics.GraphicsDevice == GraphicsDevice);
        }

        #region Initialization

        /// <summary>
        /// Creates the visual elements of the game and adds them to game Components.
        /// </summary>
        private void PopulateGame()
        {
            int drawOrder = 0;

            // Determine screen size.
            InteractiveSurface interactiveSurface = InteractiveSurface.DefaultInteractiveSurface;
            int maxWidth = graphics.PreferredBackBufferWidth;
            int maxHeight = graphics.PreferredBackBufferHeight;
            if (interactiveSurface != null)
            {
                maxWidth = interactiveSurface.Width;
                maxHeight = interactiveSurface.Height;
            }

            // Create the background Canvas and add it to game Components
            meshCanvas = new MeshCanvas(this, controller, @"Content\ApplicationBackground.jpg", maxWidth * 2, maxHeight * 2)
                             {
                                 Name = "meshCanvas",
                                 DrawOrder = drawOrder++,
                                 AutoScaleTexture = false,
                                 SpriteBlendMode = SpriteBlendMode.AlphaBlend
                             };

            Components.Add(meshCanvas);

            // Created the UIElement that encapsulates the cloth simulation component.
            textiles = new Textiles(this, controller);

            // Once we created the textiles, we can enable the tap gesture.
            contactTarget.ContactTapGesture += OnContactTapGesture;

            textiles.DrawOrder = drawOrder++;

            // Create the HUD Container and add it to game Components
            hudContainer = new UIContainer(this, controller, @"Content\Transparent256x256.png", null, 256, 256, null)
                               {
                                   Name = "hudContainer",
                                   Center = new Vector2(maxWidth / 2f, maxHeight / 2f),
                                   DrawOrder = drawOrder++,
                                   LayerDepth = 1.0f,
                                   SpriteBlendMode = SpriteBlendMode.AlphaBlend,
                                   Active = false
                               };

            Components.Add(hudContainer);

            // Create and position listbox and add it to the hudCanvas.
            Vector2 position = new Vector2(0f, 0f);
            listbox = new ListBox(hudContainer, position, 3 * ListBox.ItemWidth, ListBox.ItemHeight, textiles)
                          {
                             Name = "listbox",
                             AutoScaleTexture = false,
                          };


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
            contactTarget = new ContactTarget(Window.Handle, EventThreadChoice.OnCurrentThread);
            contactTarget.EnableInput();

            // Create the UIController for the StateMachines.
            controller = new UIController(contactTarget, HitTestCallback);
        }


        /// <summary>
        /// Resets the application's orientation and screen transform
        /// based on the current launcher orientation.
        /// </summary>
        private void ResetOrientation(UserOrientation newOrientation)
        {
            if (newOrientation == currentOrientation) { return; }

            currentOrientation = newOrientation;

            switch (currentOrientation)
            {
                case UserOrientation.Bottom:
                    screenTransform = Matrix.Identity;
                    break;
                case UserOrientation.Top:
                    screenTransform = inverted;
                    break;
            }

            // Re-orient top-level components.
            textiles.ScreenTransform = screenTransform;
            foreach (GameComponent component in Components)
            {
                UIElement element = component as UIElement;
                if (element != null)
                {
                   if (element == meshCanvas) continue;
                   element.ResetOrientation(newOrientation, screenTransform);
                }
            }
        }

        #endregion

        #region Hit Testing

        /// <summary>
        /// Performs hit testing for all game components.
        /// </summary>
        private void HitTestCallback(ReadOnlyHitTestResultCollection uncapturedContacts,
                             ReadOnlyHitTestResultCollection capturedContacts)
        {
            // First check captured contacts.
            UIElement.HitTestCapturedContacts(capturedContacts);

            // Now test the uncaptured contacts.
            // The order is important for overlapping elements.
            // Test from front to back (reverse of DrawOrder).

            // Test the HUD elements first
            hudContainer.HitTestUncapturedContacts(uncapturedContacts);

            // Now the textiles.
            textiles.HitTestUncapturedContacts(uncapturedContacts);

            SetActiveContacts();

            // And finally the background canvas.
            meshCanvas.HitTestUncapturedContacts(uncapturedContacts);

        }

        /// <summary>
        /// Captures active contacts on the Surface and bundles them into a dictionary
        /// to be sent to the textile manipulation component.
        /// </summary>
        /// <remarks>
        /// This routine is call in the middle of hit testing, after the HUD elements and textiles,
        /// but before the meshCanvas.
        /// The contacts are transformed using the current WorldMatrix of the textile component,
        /// before the next translation is applied
        /// </remarks>
        private void SetActiveContacts()
        {
            IInputElementStateMachine capturingElement;
            activeContacts.Clear();

            foreach (Contact contact in contactTarget.GetState())
            {
                capturingElement = controller.GetCapturingElement(contact);
                if (capturingElement == null || capturingElement == textiles.StateMachine)
                {
                     activeContacts.Add(contact.Id, textiles.WorldVector(contact.X, contact.Y));
                }
            }
        }


        #endregion Hit Testing

        #region Overridden Game Methods.

        //
        // The following virtual methods from the Game class have been overridden:
        //
        //    Initialize, LoadContent, Update, Draw, UnLoadContent
        //
        // The following methods are also available for override, but are not overridden
        // at this time:
        //
        //    BeginRun, BeginDraw, EndDraw, EndRun, OnActivated, OnDeactivated, OnExiting
        //

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
            ApplicationLauncher.OrientationChanged += OnOrientationChanged;

            // Create a rotation matrix to orient the screen so it is viewed correctly,
            // when the user orientation is 180 degress different.
            Matrix rotation = Matrix.CreateRotationZ(MathHelper.ToRadians(180));
            Matrix translation = Matrix.CreateTranslation(graphics.GraphicsDevice.Viewport.Width,
                                                          graphics.GraphicsDevice.Viewport.Height, 0);
            inverted = rotation * translation;

            currentOrientation = UserOrientation.Bottom;
            screenTransform = Matrix.Identity;

            PopulateGame();


            // Initialize UIElements which are not in Game.Components
            // and are not children of another UIElement.
            textiles.Initialize();

            base.Initialize();
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();
            ResetOrientation(ApplicationLauncher.Orientation);
        }

        /// <summary>
        /// Unload your graphics content here.
        /// </summary>
        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        /// <summary>
        /// Allows the app to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (!isApplicationActivated && !isApplicationPreviewed ) { return; }

            controller.Update();

            textiles.SetActiveContacts(activeContacts);
            textiles.Translate(meshCanvas.Delta);

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
                // Dismiss the loading screen now that we are starting to draw.
                applicationLoadCompleteSignalled = true;
                ApplicationLauncher.SignalApplicationLoadComplete();
            }

            graphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color.Black, 0, 0);

            base.Draw(gameTime);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// This is called when the application's user orientation has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            ResetOrientation(e.Orientation);
        }

        /// <summary>
        /// This is called when the contact target receives a tap.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnContactTapGesture(object sender, ContactEventArgs args)
        {
            if (hudContainer != null)
            {
                // Only pass taps to the textiles if the HUD container is not the one being tapped.
                UIElement touched = hudContainer.HitTesting(args.Contact, false);

                if (touched == null && textiles != null)
                {
                    // forward the event to the textiles UI element.
                    textiles.OnContactTapGesture(sender, args);
                }
            }
        }


        /// <summary>
        /// This is called when application has been activated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationActivated(object sender, EventArgs e)
        {
            // Update application state
            isApplicationActivated = true;
            isApplicationPreviewed = false;

            // Orientation can change between activations.
            ResetOrientation(ApplicationLauncher.Orientation);
        }

        /// <summary>
        /// This is called when application is in preview mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationPreviewed(object sender, EventArgs e)
        {
            // Update application state.
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
            // Update application state.
            isApplicationActivated = false;
            isApplicationPreviewed = false;
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    IDisposable graphicsDispose = graphics as IDisposable;
                    if (graphicsDispose != null)
                    {
                        graphicsDispose.Dispose();
                    }

                    if (contactTarget != null)
                    {
                        contactTarget.Dispose();
                        contactTarget = null;
                    }
                    if (textiles != null)
                    {
                        textiles.Dispose();
                        textiles = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        #endregion

    }
}
