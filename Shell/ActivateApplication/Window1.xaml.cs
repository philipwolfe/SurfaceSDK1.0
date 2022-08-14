using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Surface;

namespace ActivateApplicationSample
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 
    {
        /// <summary>
        /// The name of the xml file for the application that we will activate.
        /// </summary>
        private const string UniqueName = "IgnoreOrientation";

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

        /// <summary>
        /// Launches an application with normal orientation.
        /// </summary>
        /// <param name="sender">The Object that raised the event.</param>
        /// <param name="args">The RoutedEventArgs that contain the event data.</param>
        private void OnClickActivateWithNormalOrientation(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplicationLauncher.ActivateApplication(Window1.UniqueName);
                string text = string.Format(CultureInfo.CurrentCulture, "Activated {0} with orientation {1}.", Window1.UniqueName, ApplicationLauncher.Orientation.ToString());
                this.LogMessage(text);
            }
            catch (ApplicationLauncherException ex)
            {
                string text = string.Format(CultureInfo.CurrentCulture, "{0} activate failed. {1}.", Window1.UniqueName, ex.Message);
                this.LogMessage(text);
            }
            catch (ArgumentException ex)
            {
                string text = string.Format(CultureInfo.CurrentCulture, "{0} activate failed. {1}.", Window1.UniqueName, ex.Message);
                this.LogMessage(text);
            }
            catch (InvalidOperationException ex)
            {
                // Can happen when the application that's calling ActivateApplication is not in foreground
                // Example: When activating two applications at the same time, pressing on two buttons
                // Since two applications will start and both of them will call ActivateApplication, only
                // the first one that makes it to be the foreground application will succeed with the call
                string text = string.Format(CultureInfo.CurrentCulture, "{0} activate failed. {1}.", Window1.UniqueName, ex.Message);
                this.LogMessage(text);
            }
        }

        /// <summary>
        /// Launches an application with inverted orientation.
        /// </summary>
        /// <param name="sender">The Object that raised the event.</param>
        /// <param name="args">The RoutedEventArgs that contain the event data.</param>
        private void OnClickActivateWithInvertedOrientation(object sender, RoutedEventArgs e)
        {
            UserOrientation orientation = ApplicationLauncher.Orientation == UserOrientation.Top ? UserOrientation.Bottom : UserOrientation.Top;

            try
            {
                ApplicationLauncher.ActivateApplication(Window1.UniqueName, orientation);
                string text = string.Format(CultureInfo.CurrentCulture, "Activated {0} with orientation {1}.", Window1.UniqueName, orientation.ToString());
                this.LogMessage(text);
            }
            catch (ApplicationLauncherException ex)
            {
                string text = string.Format(CultureInfo.CurrentCulture, "{0} activate failed. {1}.", Window1.UniqueName, ex.Message);
                this.LogMessage(text);
            }
            catch (ArgumentException ex)
            {
                string text = string.Format(CultureInfo.CurrentCulture, "{0} activate failed. {1}.", Window1.UniqueName, ex.Message);
                this.LogMessage(text);
            }
            catch (InvalidOperationException ex)
            {
                // Can happen when the application that's calling ActivateApplication is not in foreground
                // Example: When activating two applications at the same time, pressing on two buttons
                // Since two applications will start and both of them will call ActivateApplication, only
                // the first one that makes it to be the foreground application will succeed with the call
                string text = string.Format(CultureInfo.CurrentCulture, "{0} activate failed. {1}.", Window1.UniqueName, ex.Message);
                this.LogMessage(text);
            }
        }

        private void LogMessage(string msg)
        {
            TextBlock text = new TextBlock();
            text.Text = string.Format(CultureInfo.CurrentCulture, 
                                      "[{0}] {1}",
                                      DateTime.Now.ToString(),
                                      msg);
   
            this.eventLog.Children.Add(text);
            (this.eventLog.Parent as ScrollViewer).ScrollToBottom();
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