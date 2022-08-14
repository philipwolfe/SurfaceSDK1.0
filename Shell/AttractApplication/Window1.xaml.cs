using System;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Surface;

namespace AttractApplicationSample
{
    /// <summary>
    /// This window is a simple attract application that handles mouse and 
    /// touch input.
    /// </summary>
    public partial class Window1
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Window1()
        {
            InitializeComponent();

            // Add handlers for Application activation events
            AddActivationHandlers();
        }

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for Application activation events
            RemoveActivationHandlers();
        }

        #region Handle mouse and touch input
        /// <summary>
        /// Occurs when an object touches this element.
        /// </summary>
        /// <param name="sender">Object that is sending the event.</param>
        /// <param name="e">ContactEventArgs</param>
        protected override void OnContactDown(Microsoft.Surface.Presentation.ContactEventArgs e)
        {
            // Toggle the color of the window's background.
            this.Background = (this.Background == Brushes.CornflowerBlue) ? Brushes.Sienna : Brushes.CornflowerBlue;
            e.Handled = true;
        }

        /// <summary>
        /// Occurs when a mouse button is pressed within this element. 
        /// </summary>
        /// <param name="e">MouseButtonEventArgs</param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            // Toggle the color of the window's background.
            this.Background = (this.Background == Brushes.CornflowerBlue) ? Brushes.Sienna : Brushes.CornflowerBlue;
            e.Handled = true;
        }
        #endregion

        /// <summary>
        /// Adds handlers for Application activation events.
        /// </summary>
        private void AddActivationHandlers()
        {
            // Subscribe to surface application activation events
            ApplicationLauncher.ApplicationActivated += OnApplicationActivated;
            ApplicationLauncher.ApplicationPreviewed += OnApplicationPreviewed;
            ApplicationLauncher.ApplicationDeactivated += OnApplicationDeactivated;
        }

        /// <summary>
        /// Removes handlers for Application activation events.
        /// </summary>
        private void RemoveActivationHandlers()
        {
            // Unsubscribe from surface application activation events
            ApplicationLauncher.ApplicationActivated -= OnApplicationActivated;
            ApplicationLauncher.ApplicationPreviewed -= OnApplicationPreviewed;
            ApplicationLauncher.ApplicationDeactivated -= OnApplicationDeactivated;
        }

        /// <summary>
        /// This is called when application has been activated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationActivated(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// This is called when application is in preview mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationPreviewed(object sender, EventArgs e)
        {
        }

        /// <summary>
        ///  This is called when application has been deactivated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationDeactivated(object sender, EventArgs e)
        {
        }
    }
}