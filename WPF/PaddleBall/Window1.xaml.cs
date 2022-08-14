using System;
using System.Windows;
using Microsoft.Surface;

namespace Paddleball
{
    //==========================================================//
    /// <summary>
    /// This is our main window. At the top level, it contains
    /// the score indicators and some pre-game configuration.
    /// The main game logic is inside of PlayingArea.xaml
    /// </summary>
    public partial class Window1
    {
        #region Initalization, Activation, and Deactivation

        //==========================================================//
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Window1()
        {
            InitializeComponent();

            // Add handlers for Application activation events
            AddActivationHandlers();
        }

        //==========================================================//
        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e">The arguments for the closed event.</param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for Application activation events
            RemoveActivationHandlers();
        }



        //==========================================================//
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

        //==========================================================//
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

        //==========================================================//
        /// <summary>
        /// This is called when application has been activated.
        /// </summary>
        /// <param name="sender">the object that raised the event.</param>
        /// <param name="e">The arguments for the event.</param>
        private void OnApplicationActivated(object sender, EventArgs e)
        {
        }

        //==========================================================//
        /// <summary>
        /// This is called when application is in preview mode.
        /// </summary>
        /// <param name="sender">the object that raised the event.</param>
        /// <param name="e">The arguments for the event.</param>n
        private void OnApplicationPreviewed(object sender, EventArgs e)
        {
        }

        //==========================================================//
        /// <summary>
        ///  This is called when application has been deactivated.
        /// </summary>
        /// <param name="sender">the object that raised the event.</param>
        /// <param name="e">The arguments for the event.</param>
        private void OnApplicationDeactivated(object sender, EventArgs e)
        {
        }

        #endregion

        #region Game Control

        //==========================================================//
        /// <summary>
        /// When the start button is pressed, hide the configure controls
        /// and show the scores, then start the actual game.
        /// </summary>
        /// <param name="sender">the object that raised the event.</param>
        /// <param name="e">The arguments for the event.</param>
        private void OnStartGame(object sender, RoutedEventArgs e)
        {
            configureGamePanel.Visibility = Visibility.Hidden;
            player1Score.Visibility = Visibility.Visible;
            player2Score.Visibility = Visibility.Visible;
            player3Score.Visibility = Visibility.Visible;
            player4Score.Visibility = Visibility.Visible;
            
            gameBoard.StartGame();
        }

        //==========================================================//
        /// <summary>
        /// When the game ends, go back to the pre-game configuration.
        /// </summary>
        /// <param name="sender">the object that raised the event.</param>
        /// <param name="e">The arguments for the event.</param>
        private void OnGameOver(object sender, EventArgs e)
        {
            configureGamePanel.Visibility = Visibility.Visible;
            player1Score.Visibility = Visibility.Hidden;
            player2Score.Visibility = Visibility.Hidden;
            player3Score.Visibility = Visibility.Hidden;
            player4Score.Visibility = Visibility.Hidden;
        }

        #endregion
    }
}