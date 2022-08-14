using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;

namespace ControlsBox
{
    /// <summary>
    /// Collection to host the test data.
    /// </summary>
    public class TestDataCollection : ObservableCollection<BitmapImage>
    {
       
    }

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

        /// <summary>
        /// Change the content of the display area to show the newly selected control.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event args.</param>
        private void OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Grid content = ((SurfaceListBoxItem)ContentSelector.SelectedItem).Tag as Grid;

            ContentArea.Children.Clear();
            ContentArea.Children.Add(content);
        }

        /// <summary>
        /// Gets called when the library container view mode changes.
        /// </summary>
        /// <param name="sender">The library container that raised the event.</param>
        /// <param name="args">The event args.</param>
        /// <remarks>
        /// It is necessary to set the minimum and maximum width of the scatter view item when the 
        /// library container changes mode because each mode in the library container has different minimum sizes.
        /// </remarks>
        private void OnViewingModeChanged(object sender, RoutedPropertyChangedEventArgs<LibraryContainerViewingMode> args)
        {
            LibraryContainer libraryContainer = args.OriginalSource as LibraryContainer;
            ScatterViewItem scatterViewItem = sender as ScatterViewItem;

            // Update the maximum and minimum height of the scatter view based on 
            // the library container view mode in order to prevent the scatterview item from
            // clipping the library container.

            /// Given that the ratio between width and height stays constant, it is sufficient to update only the 
            /// minimum and maximum height. The values hardcoded here are those that you would receive if you 
            /// queried the properties on the library container after the mode changed.
            if (libraryContainer.ViewingMode == LibraryContainerViewingMode.Stack)
            {
                scatterViewItem.MaxHeight = 800;
                scatterViewItem.MinHeight = 80;
            }
            else
            {
                scatterViewItem.MinHeight = 50;
                scatterViewItem.MaxHeight = 500;
            }

            // Ensure that the library container is still within the bounds of the scatter view 
            // after it changes size.
            scatterViewItem.BringIntoBounds();
        }


        /// <summary>
        /// Clear the strokes from the InkCanvas.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event args.</param>
        private void OnInkCanvasClearClick(object sender, RoutedEventArgs e)
        {
            SampleInkCanvas.Strokes.Clear();
        }
    }
}