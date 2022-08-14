using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Surface.Presentation;

namespace Paddleball
{
    /// <summary>
    /// Enumeration of possible directions for a paddle to move.
    /// </summary>
    public enum Direction
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// Interaction logic for Paddle.xaml.
    /// </summary>
    public partial class Paddle : UserControl
    {
        #region Dependency Properties

        //==========================================================//
        /// <summary>
        /// Property that represents which direction (horizontal or vertical) a paddle moves.
        /// </summary>
        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(
            "Direction",
            typeof(Direction),
            typeof(Paddle),
            new PropertyMetadata(new PropertyChangedCallback(OnDirectionChanged)));

        //==========================================================//
        /// <summary>
        /// Property that represents how far a paddle can move in either direction
        /// </summary>
        public static readonly DependencyProperty MaximumOffsetProperty = DependencyProperty.Register(
            "MaximumOffset",
            typeof(double),
            typeof(Paddle));

        //==========================================================//
        /// <summary>
        /// Handles the Direction changed event.
        /// </summary>
        /// <param name="obj">The DependencyObject that had a property changed.</param>
        /// <param name="args">The event arguments.</param>
        private static void OnDirectionChanged ( DependencyObject obj, DependencyPropertyChangedEventArgs args )
        {
            Paddle paddle = (Paddle)obj;

            // If the paddle moves vertically, rotate the paddle 90 degrees
            RotateTransform transform = new RotateTransform();
            transform.Angle = (Direction)args.NewValue == Direction.Vertical ? 90 : 0;
            paddle.LayoutTransform = transform;

        }

        #endregion

        #region Public Properties

        //==========================================================//
        /// <summary>
        /// Gets or sets the direction in which a paddle moves.
        /// </summary>
        public Direction Direction
        {
            get
            {
                return (Direction)GetValue(DirectionProperty);
            }
            set
            {
                SetValue(DirectionProperty, value);
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets or sets the maximum amount a paddle can move.
        /// </summary>
        public double MaximumOffset
        {
            get
            {
                return (double)GetValue(MaximumOffsetProperty);
            }
            set
            {
                SetValue(MaximumOffsetProperty, value);
            }
        }

        #endregion

        #region Initalization

        //==========================================================//
        /// <summary>
        /// Constructor.
        /// </summary>
        public Paddle()
        {
            InitializeComponent();
        }

        #endregion

        #region Contact and Mouse Input

        //==========================================================//
        /// <summary>
        /// Handles the ContactDown event.
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="e">The event args</param>
        private void OnContactDown(object sender, ContactEventArgs e)
        {
            // Capture the contact if another contact is not already captured. This avoids 
            // the case where two contacts are both trying to "fight" over the same paddle
            if (!Contacts.GetIsAnyContactCaptured(rect))
            {
                // Capture the contact
                e.Contact.Capture(rect);

                // Remember the current position. It will be used in the contact changed 
                // event to calculate a movement delta
                Tag = e.GetPosition((Grid)Parent);

                e.Handled = true;
            }
        }

        //==========================================================//
        /// <summary>
        /// Handles the ContactChanged event.
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="e">The event args</param>
        private void OnContactChanged(object sender, ContactEventArgs e)
        {
            // If the contact was captured by this paddle earlier, the value in Contact.Captured
            // will be this paddle. If it is, then this contact is manipulating the paddle
            if (e.Contact.Captured == rect)
            {
                // Retrieve the previous position stored in the Tag property
                Point oldPt = (Point)Tag;
                Point newPt = e.Contact.GetPosition((Grid)Parent);

                // Update the paddle based on the delta between the previous position and the current position
                UpdatePaddle(oldPt, newPt);
            }
        }

        //==========================================================//
        /// <summary>
        /// Handles the ContactUp event
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="e">The event args</param>
        private void OnContactUp(object sender, ContactEventArgs e)
        {
            if (e.Contact.Captured == rect)
            {
                Contacts.ReleaseContactCapture(e.Contact);
            }
        }

        //==========================================================//
        /// <summary>
        /// Handles the MouseLeftButtonDown event.
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="e">The event args</param>
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Capture the mouse
            Mouse.Capture(rect);

            // Remember the current position. It will be used in the contact changed 
            // event to calculate a movement delta
            Tag = e.GetPosition((Grid)Parent);
        }

        //==========================================================//
        /// <summary>
        /// Handles the MouseMove event.
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="e">The event args</param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            // If the mouse is already captured by this paddle, it is manipulating the paddle
            if (rect.IsMouseCaptured)
            {
                Point oldPt = (Point)Tag;
                Point newPt = Mouse.GetPosition((Grid)Parent);

                UpdatePaddle(oldPt, newPt);
            }
        }

        //==========================================================//
        /// <summary>
        /// Handles the MouseLeftButtonUp event.
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="e">The event args</param>
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Release the mouse
            rect.ReleaseMouseCapture();
        }

        #endregion

        #region Paddle Manipulation

        //==========================================================//
        /// <summary>
        /// Move the paddle by the delta between two points.
        /// </summary>
        /// <param name="oldPt">The previous mouse/contact location.</param>
        /// <param name="newPt">The current mouse/contact location.</param>
        private void UpdatePaddle(Point oldPt, Point newPt)
        {
            TranslateTransform transform = (TranslateTransform)rect.RenderTransform;
 
            if (Direction == Direction.Horizontal)
            {
                double offsetX = newPt.X - oldPt.X;
                double newX = transform.X + offsetX;
                // Don't move the paddle if it would go out of range
                if (Math.Abs(newX) > MaximumOffset)
                    return;

                transform.X = newX;
            }
            else 
            {
                // Because a renderTransform rotates the paddle, use the x value 
                // of the transform and the y values of the contact positions
                double offsetY = newPt.Y - oldPt.Y;
                double newY = transform.X + offsetY;
                // Don't move the paddle if it would go out of range
                if (Math.Abs(newY) > MaximumOffset)
                    return;
                transform.X = newY;
            }

            // Save the last contact point
            Tag = newPt;
        }

        #endregion

        #region Hit Testing

        //==========================================================//
        /// <summary>
        /// See if the ball collides with the paddle.
        /// </summary>
        /// <param name="ball">The bounding rectangle for the ball</param>
        /// <returns>True if the ball is touching the paddle, false otherwise</returns>
        public bool HitTest(Rect ball)
        {
            // Get a rectangle that represents the paddle in PlayingArea coordinates 
            PlayingArea area = GetParent<PlayingArea>(this);
            Point topLeft = new Point ( 0, 0 );
            Point bottomRight = new Point(rect.ActualWidth, rect.ActualHeight);

            topLeft = rect.TranslatePoint(topLeft, area);
            bottomRight = rect.TranslatePoint(bottomRight, area);

            Rect paddleBounds = new Rect(topLeft, bottomRight);

            // Intersect the paddle rect with the ball rect
            paddleBounds.Intersect(ball);

            // If there is overlap, then there is a hit
            return !paddleBounds.IsEmpty;
        }

        //==========================================================//
        /// <summary>
        /// Searches a DependencyObject's ancestry in the visual tree to find an ancestor of
        /// a specific type.
        /// </summary>
        /// <param name="child">The DependencyObject whose parent should be found.</param>
        /// <returns>The parent of the specified type.</returns>
        private static T GetParent<T>(DependencyObject child) where T : DependencyObject
        {
            return (T)FindParent(child, delegate(DependencyObject d)
            {
                return d is T;
            });
        }

        //==========================================================//
        /// <summary>
        /// Searches for the first parent that satisfies the given predicate.
        /// </summary>
        internal static DependencyObject FindParent(DependencyObject child, Predicate<DependencyObject> predicate)
        {
            // go through visual tree
            Visual visualChild = (Visual)child;
            DependencyObject visualParent = null;
            if (child != null)
            {
                // get visual parent
                visualParent = VisualTreeHelper.GetParent(visualChild);

                if (visualParent != null)
                {
                    // check the the visualParent,
                    if (predicate(visualParent) ||
                        (visualParent = FindParent(visualParent, predicate)) != null)
                    {
                        return visualParent;
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
