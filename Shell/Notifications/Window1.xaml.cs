using System;
using System.Windows;
using Microsoft.Surface;

namespace NotificationsSample
{
    /// <summary>
    /// Interaction logic for Window1.xaml
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


        private void BackgroundThreadProc(object stateInfo)
        {
            // Wait for 10 seconds to give the user a chance to launch a second application.
            System.Threading.Thread.Sleep(10000);

            // Send a notification to the user
            Microsoft.Surface.UserNotifications.RequestNotification("Delayed Notification",
                "This is a typical notification sent from a non-foreground application.  You can touch the application's icon to switch to the application that sent this notification.");
        }

        private void ImmediateButtonClick(object sender, RoutedEventArgs e)
        {
            // Send a notification to the user that will appear for 6 seconds
            Microsoft.Surface.UserNotifications.RequestNotification("Immediate Notification",
                "Touch and hold to keep this notification visible for longer than 6 seconds. Touch the X or drag the notification down to make it disappear.",
                TimeSpan.FromSeconds(6));
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
            // If the user has checked the option to send a delayed notification, queue a 
            // threadpool work item to do so.  We queue a threadpool work item so that we
            // don't hang the UI thread in case the user returns to this application.
            if (this.showBackgroundNotificationsCheck.IsChecked.HasValue && this.showBackgroundNotificationsCheck.IsChecked.Value)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(this.BackgroundThreadProc));

                // When notification appears the application state becomes Previewed, if user doesn't activate the
                // application, it goes into Deactivate state. To avoid triggering the notification again,
                // set showBackgroundNotificationsCheck to false
                this.showBackgroundNotificationsCheck.IsChecked = false;
            }
        }
    }
}