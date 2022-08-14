using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Paddleball
{

    //==========================================================//
    /// <summary>
    /// Enumeration of all paddles that users can control.
    /// </summary>
    public enum PaddlePosition
    {
        Top,
        Left, 
        Bottom, 
        Right,
        Unknown
    }

    //==========================================================//
    /// <summary>
    /// PlayingArea is the UserControl that hosts the main
    /// game play. It contains four paddles (Rectangles) that
    /// resond to surface input.
    /// </summary>
    public partial class PlayingArea : INotifyPropertyChanged
    {
        #region Private Members

        // These variables are used to control the movement of the ball
        private bool ballInitialized;
        private Point ballPosition;
        private Vector ballVelocity;
        private Random rand = new Random((int)DateTime.Now.Ticks);
        private const int maxSpeed = 20;
        private const int minSpeed = 7;
        private const int ballSize = 16;

        // A timer to invalidate the visual so we can redraw
        // the ball at it's new position.
        private readonly DispatcherTimer invalidateTimer;

        // These are used for score tracking.
        private int[] scores = new int[4];
        private Collection<int> scoreCollection;
        private int winningScore = 5;
        private PaddlePosition lastPaddleHit;

        #endregion

        #region Public Properties

        //==========================================================//
        /// <summary>
        /// Gets the player scores collection.
        /// </summary>
        public Collection<int> PlayerScores
        {
            get
            {
                if (scoreCollection == null)
                    scoreCollection = new Collection<int>(scores);
                return scoreCollection;
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets or sets the score that a player needs to win the game.
        /// </summary>
        public int WinningScore
        {
            get { return winningScore; }
            set { winningScore = value; }
        }

        #endregion

        #region Initalization

        //==========================================================//
        /// <summary>
        /// Constructor.
        /// </summary>
        public PlayingArea()
        {
            InitializeComponent();

            // Initalize the timer that controls drawing
            invalidateTimer = new DispatcherTimer();
            invalidateTimer.Interval = TimeSpan.FromMilliseconds(50);
            invalidateTimer.Tick += new EventHandler(OnTimerTick);
        }

        #endregion

        #region Game start and stop

        //==========================================================//
        /// <summary>
        /// Begin a new game:
        ///     1. Serve the ball.
        ///     2. Start the timer that controls playback.
        /// </summary>
        public void StartGame()
        {
            Serve();
            invalidateTimer.Start();
        }

        //==========================================================//
        /// <summary>
        /// Reset the game by putting everything back to the original state:
        ///    1. Stop the timer.
        ///    2. Center the ball.
        ///    3. Stop the ball from moving.
        ///    4. Reset all player scores.
        ///    5. Reset the last paddle to have hit the ball.
        /// </summary>
        private void ResetGame()
        {
            invalidateTimer.Stop();
            ResetBall();
            ballVelocity = new Vector();
            scores[0] = scores[1] = scores[2] = scores[3] = 0;
            OnPropertyChanged("PlayerScores");
            lastPaddleHit = PaddlePosition.Unknown;
        }

        //==========================================================//
        /// <summary>
        /// Serve the next ball after a point is scored (or the game is started):
        ///     1. Ensure that the playing area is clear of the result text.
        ///     2. Start the ball in the middle.
        ///     3. Reset the last paddle to have hit the ball.
        ///     4. Pick a random ball velocity.
        /// </summary>
        private void Serve()
        {
            resultText.Visibility = Visibility.Hidden;
            ResetBall();
            lastPaddleHit = PaddlePosition.Unknown;
            ballVelocity = new Vector(rand.Next(minSpeed, maxSpeed) * (rand.Next(1, 3) == 1 ? 1 : -1),
                                      rand.Next(minSpeed, maxSpeed) * (rand.Next(1, 3) == 1 ? 1 : -1));
        }

        //==========================================================//
        /// <summary>
        /// Put the ball back in the middle of the playing area.
        /// </summary>
        private void ResetBall()
        {
            ballPosition = new Point(ActualWidth / 2, ActualHeight / 2);
        }

        #endregion

        #region Layout and rendering

        //==========================================================//
        /// <summary>
        /// Force an update every time the timer fires.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event args.</param>
        void OnTimerTick(object sender, EventArgs e)
        {
            this.InvalidateVisual();
        }

        //==========================================================//
        /// <summary>
        /// Override of OnRender.
        /// </summary>
        /// <param name="drawingContext">
        /// The drawing instructions for a specific element. This context is provided to the layout system. 
        /// </param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            // Reset everything (done here to ensure the measure is complete)
            if (!ballInitialized)
            {
                ResetGame();
                ballInitialized = true;
            }

            // Calculate the new position of the ball
            UpdateBallPosition();

            // Draw the ball
            Point center = new Point ( ballPosition.X + ballSize/2, ballPosition.Y + ballSize / 2 );
            drawingContext.DrawEllipse(Brushes.White, null, center, ballSize / 2, ballSize / 2);

            base.OnRender(drawingContext);
        }

        #endregion

        #region Game Logic

        //==========================================================//
        /// <summary>
        /// Determine the new position of the ball for the current frame.
        /// </summary>
        private void UpdateBallPosition()
        {
            Point newPosition = ballPosition + ballVelocity;
            Rect ball = new Rect(newPosition.X - ballSize / 2, newPosition.Y - ballSize / 2, ballSize, ballSize);

            // Did the ball hit the top paddle?
            if (lastPaddleHit != PaddlePosition.Top && TopPaddle.HitTest(ball))
            {
                ballVelocity.Y *= -1;
                lastPaddleHit = PaddlePosition.Top;
            }

            // Did the ball hit the bottom paddle?
            else if (lastPaddleHit != PaddlePosition.Bottom && BottomPaddle.HitTest(ball))
            {
                ballVelocity.Y *= -1;
                lastPaddleHit = PaddlePosition.Bottom;
            }

            // Did the ball hit the left paddle?
            else if (lastPaddleHit != PaddlePosition.Left && LeftPaddle.HitTest(ball))
            {
                ballVelocity.X *= -1;
                lastPaddleHit = PaddlePosition.Left;
            }

            // Did the ball hit the right paddle?
            else if (lastPaddleHit != PaddlePosition.Right && RightPaddle.HitTest(ball))
            {
                ballVelocity.X *= -1;
                lastPaddleHit = PaddlePosition.Right;
            }

            // Did the ball get past a paddle and enter the scoring zone?
            else if ((ball.Top >= this.ActualHeight || ball.Bottom <= 0) || (ball.Right <= 0 || ball.Left >= this.ActualWidth))
            {
                // Determine who gets the score based on the last paddle to hit the ball (if no one hit it, no one scores)
                if (lastPaddleHit != PaddlePosition.Unknown)
                {
                    scores[(int)lastPaddleHit]++;
                    OnPropertyChanged("PlayerScores");

                    if (scores[(int)lastPaddleHit] == WinningScore)
                    {
                        // Reset the whole game
                        resultText.Visibility = Visibility.Visible;
                        resultText.Text = string.Format(CultureInfo.CurrentCulture, Paddleball.Properties.Resources.VictoryMessage, ((int)lastPaddleHit + 1));
                        if (GameOver != null)
                            GameOver(this, new EventArgs());

                        ResetGame();
                        return;
                    }
                }

                Serve();
            }
            else
            {
                // Normal case, ball just moves along
                ballPosition = newPosition;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        //==========================================================//
        /// <summary>
        /// Raise a PropertyChanged notification.
        /// </summary>
        /// <param name="property">the property that changed.</param>
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler GameOver;

        #endregion
    }
}