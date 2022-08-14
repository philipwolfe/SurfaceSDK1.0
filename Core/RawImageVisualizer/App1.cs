using System;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RawImageVisualizer
{
    /// <summary>
    /// This is the main type for your application.
    /// </summary>
    public class App1 : Microsoft.Xna.Framework.Game
    {
        private readonly GraphicsDeviceManager graphics;
        private ContactTarget contactTarget;
        private bool applicationLoadCompleteSignalled;
        private SpriteBatch foregroundBatch;
        private Texture2D contactSprite;
        private Vector2 spriteOrigin = new Vector2(0f, 0f);

        // normalizedImageUpdated is accessed from differet threads. Mark it
        // volatile to make sure that every read gets the latest value.
        private volatile bool normalizedImageUpdated;

        // For Normalized Raw Image
        private byte[] normalizedImage;
        private ImageMetrics normalizedMetrics;

        // For Scaling the RawImage back to full screen.
        private  float scale =
            (float) InteractiveSurface.DefaultInteractiveSurface.Width / InteractiveSurface.DefaultInteractiveSurface.Height;

        // application state: Activated, Previewed, Deactivated,
        // start in Activated state
        private bool isApplicationActivated = true;
        private bool isApplicationPreviewed;

        // Something to lock to deal with asynchronous frame updates
        private object syncObject = new object();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public App1()
        {
            graphics = new GraphicsDeviceManager(this);
        }

        /// <summary>
        /// Allows the app to perform any initialization it needs to before starting to run.
        /// This is where it calls to loads amd Initializes SurfaceInput ContactTarget.  
        /// Calling base.Initialize will enumerate through any components
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

            base.Initialize();
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

            // Turn raw image back on again
            contactTarget.EnableImage(ImageType.Normalized);
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

            // If the app isn't active, there's no need to keep the raw image enabled
            contactTarget.DisableImage(ImageType.Normalized);
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
            contactTarget = new ContactTarget(Window.Handle, EventThreadChoice.OnBackgroundThread);
            contactTarget.EnableInput();

            // Enable the normalized raw-image.
            contactTarget.EnableImage(ImageType.Normalized);

            // Hook up a callback to get notified when there is a new frame available
            contactTarget.FrameReceived += OnContactTargetFrameReceived;
        }

        /// <summary>
        /// Handler for the FrameReceived event. 
        /// Here we get the rawimage data from FrameReceivedEventArgs object.
        /// </summary>
        /// <remarks>
        /// When a frame is received, this event handler gets the normalized image 
        /// from the ContactTarget. The image is copied into the contact sprite in the 
        /// Update method. The reason for this separation is that this event handler is 
        /// called from a background thread and Update is called from the main thread. It 
        /// is not safe to get the normalized image from the ContactTarget in the Update 
        /// method because ContactTarget and Update run on different threads. It is 
        /// possible for ContactTarget to change the image while Update is accessing it.
        /// 
        /// To address the threading issue, the raw image is retrieved here, on the same 
        /// thread that updates and uses it on the main thread. It is stored in a variable 
        /// that is available to both threads, and access to the variable is controlled 
        /// through lock statements so that only one thread can use the image at a time.
        /// </remarks>
        /// <param name="sender">ContactTarget that received the frame</param>
        /// <param name="e">Object containing information about the current frame</param>
        private void OnContactTargetFrameReceived(object sender, FrameReceivedEventArgs e)
        {
            // Lock the syncObject object so normalizedImage isn't changed while the Update method is using it
            lock (syncObject)
            {
                if (normalizedImage == null)
                {
                    // get rawimage data for a specific area
                    e.TryGetRawImage(
                        ImageType.Normalized,
                        0, 0,
                        InteractiveSurface.DefaultInteractiveSurface.Width,
                        InteractiveSurface.DefaultInteractiveSurface.Height,
                        out normalizedImage,
                        out normalizedMetrics);
                }
                else
                {
                    // get the updated rawimage data for the specified area
                    e.UpdateRawImage(
                        ImageType.Normalized,
                        normalizedImage,
                        0, 0,
                        InteractiveSurface.DefaultInteractiveSurface.Width,
                        InteractiveSurface.DefaultInteractiveSurface.Height);
                }

                normalizedImageUpdated = true;
            }
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            foregroundBatch = new SpriteBatch(graphics.GraphicsDevice);
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
            // Lock the syncObject object so the normalized image and metrics aren't updated while this method is using them
            lock (syncObject)
            {
                // Don't bother if the app isn't visible, or if the image hasn't been updates since the last update
                if (normalizedImageUpdated && (isApplicationActivated || isApplicationPreviewed))
                {
                    if (normalizedMetrics != null)
                    {
                        if (contactSprite == null)
                        {
                            // Creating a Sprite from the rawimage metrics data
                            contactSprite = new Texture2D(graphics.GraphicsDevice,
                                                          normalizedMetrics.Width,
                                                          normalizedMetrics.Height,
                                                          1,
                                                          TextureUsage.AutoGenerateMipMap,
                                                          SurfaceFormat.Luminance8);
                        }

                        // Texture2D requires that the texture is not set on the device when updating it, so set it null               
                        graphics.GraphicsDevice.Textures[0] = null;

                        // Setting the Texture2D with normalized rawimage data.
                        contactSprite.SetData<Byte>(normalizedImage,
                                                    0,
                                                    normalizedMetrics.Width * normalizedMetrics.Height,
                                                    SetDataOptions.Discard);
                    }
                }
                // reset the flag
                normalizedImageUpdated = false;
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

            graphics.GraphicsDevice.Clear(Color.White);

            foregroundBatch.Begin();
            // draw the sprite of RawImage
            if (contactSprite != null)
            {
                // Adds the rawimage sprite to Spritebatch for drawing.
                foregroundBatch.Draw(contactSprite, spriteOrigin, null, Color.White,
                   0f, spriteOrigin, scale, SpriteEffects.FlipVertically, 0f);
            }
            foregroundBatch.End();

            base.Draw(gameTime);
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release  managed resources.
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
