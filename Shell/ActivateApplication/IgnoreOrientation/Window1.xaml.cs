using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Surface;

namespace IgnoreOrientationSample
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        /// <summary>
        /// The name of the xml file for the application that we will activate.
        /// </summary>
        private const string UniqueName = "ActivateApplication";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Window1()
        {
            this.InitializeComponent();

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


        /// <summary>
        /// Launches an application with normal orientation.
        /// </summary>
        /// <param name="sender">The Object that raised the event.</param>
        /// <param name="args">The RoutedEventArgs that contain the event data.</param>
        private void OnButtonClickReturn(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplicationLauncher.ActivateApplication(Window1.UniqueName);
            }
            catch (ApplicationLauncherException ex)
            {
                this.errorText.Text = string.Format(CultureInfo.CurrentCulture, "{0} activate failed. {1}.", Window1.UniqueName, ex.Message);
            }
            catch (ArgumentException ex)
            {
                this.errorText.Text = string.Format(CultureInfo.CurrentCulture, "{0} activate failed. {1}.", Window1.UniqueName, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                this.errorText.Text = string.Format(CultureInfo.CurrentCulture, "{0} activate failed. {1}.", Window1.UniqueName, ex.Message);
            }
        }


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
            // Clears the error string when the application is activated.
            this.errorText.ClearValue(TextBlock.TextProperty);
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