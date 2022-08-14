using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Manipulations;

namespace FractalBrowser
{
    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class SurfaceWindow1
    {

        #region Private Members

        private Affine2DManipulationProcessor manipulationProcessor;
        private Affine2DInertiaProcessor inertiaProcessor;

        private const int minZoom = 1;
        private const int maxZoom = 1 << 24;

        // Deceleration rates

        // 100 rotations/second squared, specified in radians (200 pi / (1000ms/s)^2)
        private const double angularDeceleration = 200 * Math.PI / (1000.0 * 1000.0);

        // 24 inches/second squared (24 inches * 96 pixels per inch / (1000ms/s )^2)
        private const double deceleration = 24.0 * 96.0 / (1000.0 * 1000.0);

        // When inertia delta values get scaled by more than this amount, stop the inertia early
        private const double inertiaScaleStop = 0.8;

        // The following variables help us remember the angular velocity the fractal
        // has when the second contact is lost, allowing rotational inertia where there are less 
        // than 2 contacts touching the fractal.
        private double oldAngularVelocity;
        bool startIntertiaProcessorWithAngularVelocity;

        #endregion

        #region Properties

        //==========================================================//
        /// <summary>
        /// Gets the manipulation processor.
        /// </summary>
        private Affine2DManipulationProcessor ManipulationProcessor
        {
            get
            {
                if (manipulationProcessor == null)
                {
                    InitializeManipulationAndInertiaProcessors();
                }
                return manipulationProcessor;
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets the inertia processor.
        /// </summary>
        private Affine2DInertiaProcessor InertiaProcessor
        {
            get
            {
                if (inertiaProcessor == null)
                {
                    InitializeManipulationAndInertiaProcessors();
                }
                return inertiaProcessor;
            }
        }

        #endregion

        #region Initalization

        //==========================================================//
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SurfaceWindow1()
        {

        }

        //==========================================================//
        /// <summary>
        /// Create manipulation and inertia processors.
        /// </summary>
        private void InitializeManipulationAndInertiaProcessors()
        {
            // Use content of the window as the container instead of the actual window itself. In the case where the app
            // is inverted, using the window as container will result in delta values that don't take the inversion
            // into account, since the inversion is applied to the window's content, not the window itself. Using content
            // correctly takes into account inversion.
            manipulationProcessor = new Affine2DManipulationProcessor(Affine2DManipulations.Rotate | Affine2DManipulations.Scale | Affine2DManipulations.TranslateX | Affine2DManipulations.TranslateY, container);
            inertiaProcessor = new Affine2DInertiaProcessor();

            manipulationProcessor.Affine2DManipulationStarted += OnManipulationStarted;
            manipulationProcessor.Affine2DManipulationDelta += OnManipulationDelta;
            manipulationProcessor.Affine2DManipulationCompleted += OnManipulationCompleted;

            inertiaProcessor.Affine2DInertiaDelta += OnManipulationDelta;
            inertiaProcessor.Affine2DInertiaCompleted += OnInertiaCompleted;
        }

        #endregion

        #region Input Events

        //==========================================================//
        /// <summary>
        /// Called when a contact down event is recieved.
        /// </summary>
        /// <param name="o">The object that fired the event.</param>
        /// <param name="args">The arguments for the event</param>
        private void OnContactDown(object o, ContactEventArgs args)
        {
            CaptureContact(args.Contact);
            args.Handled = true;
        }

        //==========================================================//
        /// <summary>
        /// Called when a contact down event is recieved.
        /// </summary>
        /// <param name="o">The object that fired the event.</param>
        /// <param name="args">The arguments for the event</param>
        private void OnMouseDown(object o, MouseEventArgs args)
        {
            CaptureMouse();
            args.Handled = true;
        }

        //==========================================================//
        /// <summary>
        /// Called when a contact up is recieved.
        /// </summary>
        /// <param name="o">The object that fired the event.</param>
        /// <param name="args">The arguments for the event</param>
        private void OnMouseUp(object o, MouseEventArgs args)
        {
            ReleaseMouseCapture();
        }

        //==========================================================//
        /// <summary>
        /// Override of OnGotContactCapture.
        /// </summary>
        /// <param name="e">The arguments.</param>
        protected override void OnGotContactCapture(ContactEventArgs e)
        {
            base.OnGotContactCapture(e);

            ReadOnlyContactCollection contactCollection = Contacts.GetContactsCaptured(this);
            if (contactCollection.Count >= 2)
            {
                // Two or more contacts pin the control. Stop inertia processor.
                if (InertiaProcessor.IsRunning)
                {
                    InertiaProcessor.End();
                }
            }

            // If the contact is touching the fractal content, stop any movement 
            // and tell the manipulation processor to track this contact.
            // Get position relative to the manpiulation processor's container.
            if (Fractal.Bounds.Contains(Fractal.Viewport.ConvertParentCoordinateToViewportCoordinate(e.Contact.GetPosition(container))))
            {
                // Tell the manipulation processor to track the contact
                ManipulationProcessor.BeginTrack(e.Contact);
            }
        }

        //==========================================================//
        /// <summary>
        /// Override of OnLostContactCapture.
        /// </summary>
        /// <param name="e">The arguments.</param>
        protected override void OnLostContactCapture(ContactEventArgs e)
        {
            base.OnLostContactCapture(e);

            ReadOnlyContactCollection contactCollection = Contacts.GetContactsCaptured(this);
            if (contactCollection != null && contactCollection.Count == 1)
            {
                // we went from 2 contacts to 1 contact, we need to start the inertia processor 
                // with some angular velocity.
                startIntertiaProcessorWithAngularVelocity = true;
            }
            // Stop tracking this contact in the manipulation processor
            ManipulationProcessor.EndTrack(e.Contact);
        }

        //==========================================================//
        /// <summary>
        /// Override of OnGotContactCapture.
        /// </summary>
        /// <param name="e">The arguments.</param>
        protected override void OnGotMouseCapture(MouseEventArgs e)
        {
            base.OnGotMouseCapture(e);

            // If the cursor is touching the fractal content, stop any movement 
            // and tell the manipulation processor to track this contact.
            // Get position relative to the manpiulation processor's container.
            if (Fractal.Bounds.Contains(Fractal.Viewport.ConvertParentCoordinateToViewportCoordinate(e.GetPosition(container))))
            {
                // Tell the manipulation processor to track the contact
                ManipulationProcessor.BeginTrack(e.MouseDevice);
            }
        }

        //==========================================================//
        /// <summary>
        /// Override of OnLostContactCapture.
        /// </summary>
        /// <param name="e">The arguments.</param>
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);

            // Stop tracking this contact in the manipulation processor
            ManipulationProcessor.EndTrack(e.MouseDevice);
        }

        #endregion

        #region Manipulation Events

        private void OnManipulationStarted(object sender, Affine2DOperationStartedEventArgs e)
        {
            // Stop any animations if they are running
            ReleasePropertyFromAnimation(Fractal, Viewport.OffsetProperty);
            ReleasePropertyFromAnimation(FractalNameContainer, Canvas.LeftProperty);
            ReleasePropertyFromAnimation(FractalNameContainer, Canvas.TopProperty);
        }


        /// <summary>
        /// Starts the inertia processor.
        /// </summary>
        /// <param name="origin">The point to use as the intertia processor origin.</param>
        /// <param name="velocity">The initial velocity.</param>
        /// <param name="angularVelocity">The initial angular velocity.</param>
        private void StartInertiaProcessor(Point origin, Vector velocity, double angularVelocity)
        {
            InertiaProcessor.InitialOrigin = origin;

            // Set the deceleration rates
            InertiaProcessor.DesiredDeceleration = deceleration;
            InertiaProcessor.DesiredExpansionDeceleration = 0.0; // Can't scale flick, don't need deceleration
            InertiaProcessor.DesiredAngularDeceleration = angularDeceleration;

            // Set the inital values
            InertiaProcessor.InitialVelocity = velocity;
            InertiaProcessor.InitialExpansionVelocity = 0.0; // Can't scale flick
            InertiaProcessor.InitialAngularVelocity = angularVelocity;

            // Initial Radius should be the average radius (width / 2 + height / 2) / 2 = width+height / 4, but Inertia Processor says value must be at least 1.
            InertiaProcessor.InitialRadius = Math.Max(1, (Fractal.Viewport.RotatedWidth + Fractal.Viewport.RotatedHeight) / 4);

            // Translation constraints should ensure content stays in bounds, no need to use boundary
            InertiaProcessor.Bounds = new Thickness(double.NegativeInfinity, double.NegativeInfinity, double.PositiveInfinity, double.PositiveInfinity);

            // Start the inertia
            InertiaProcessor.Begin();
        }

        //==========================================================//
        /// <summary>
        /// Handles the manipulation delta event for both the inertia and manipulation processors.
        /// </summary>
        /// <param name="sender">The manipulation/inertia processor that fired the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnManipulationDelta(object sender, Affine2DOperationDeltaEventArgs e)
        {
            // Calculate the position of the manipulationOrigin relative to the viewport, and the offset to the original center
            Point currentManipulationOrigin = Fractal.Viewport.ConvertParentCoordinateToViewportCoordinate(e.ManipulationOrigin);

            // Get constrained scale and translation values before the fractal is manipulated
            Vector constrainedTranslation = ConstrainTranslation(e.Delta);
            Vector transformedTranslation = Fractal.Viewport.Transformation.Transform(constrainedTranslation);
            double constrainedScale = ConstrainScale(e.ScaleDelta);

            // Transform the matrix

            // Scale
            if (constrainedScale != 1.0)
            {
                Fractal.Viewport.ScaleAroundPoint(1 / constrainedScale, currentManipulationOrigin);
            }

            // Remember the old angular velocity in order to give the fractal rotational inertia
            // when the last contact gets released.
            oldAngularVelocity = e.AngularVelocity;

            // Rotation
            if (e.RotationDelta != 0.0)
            {
                Fractal.Viewport.RotateAroundPoint(-e.RotationDelta, currentManipulationOrigin);
            }

            if (startIntertiaProcessorWithAngularVelocity)
            {
                // start the intertia processor only with angular velocity.
                if (e.AngularVelocity != 0)
                {
                    StartInertiaProcessor(e.ManipulationOrigin, new Vector(0, 0), e.AngularVelocity);
                }

                startIntertiaProcessorWithAngularVelocity = false;
            }

            // Translation
            if (transformedTranslation.LengthSquared != 0.0)
            {
                Fractal.Viewport.Translate(-transformedTranslation);
            }

            // Get a new point for the fractal name
            Point newNameCenter = Fractal.FractalNamePoint;

            // Move the fractal name center to the new point
            Canvas.SetLeft(FractalNameContainer, newNameCenter.X);
            Canvas.SetTop(FractalNameContainer, newNameCenter.Y);

            // rotate the fractal name.
            RotateTransform rotation = (RotateTransform)FractalNameContainer.RenderTransform;
            rotation.Angle = -Fractal.Viewport.Rotation;

            // Once the inertia deltas start getting scaled by more than inertiaScaleStop, stop the inertia and begin the snap back immediately
            if (sender is Affine2DInertiaProcessor && constrainedTranslation.LengthSquared / e.Delta.LengthSquared < inertiaScaleStop * inertiaScaleStop )
            {
                InertiaProcessor.End();
            }
        }

        //==========================================================//
        /// <summary>
        /// Handles the manipulation processor's manipulation completed event.
        /// </summary>
        /// <param name="sender">The manipulation processor that fired the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnManipulationCompleted(object sender, Affine2DOperationCompletedEventArgs e)
        {
            // Get the point to compare to the boundary
            Vector relevantPointOffset = GetRelevantPointOffset();

            // If content is not in the fractal region, snap back immediately
            if (relevantPointOffset.LengthSquared > 0)
            {
                // Snap the fractal back by the offset
                SnapBack(relevantPointOffset);

                // Don't start the inertia processor
                return;
            }

            // Start the inertia processor with both velocity and and angular velocity.
            StartInertiaProcessor(e.ManipulationOrigin, e.Velocity, oldAngularVelocity);
            InertiaProcessor.InitialOrigin = e.ManipulationOrigin;
        }

        //==========================================================//
        /// <summary>
        /// Handles the inertia processor's intetia completed event.
        /// </summary>
        /// <param name="sender">The inertia processor that fired the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnInertiaCompleted(object sender, Affine2DOperationCompletedEventArgs e)
        {
            // Get the point to compare to the boundary
            Vector relevantPointOffset = GetRelevantPointOffset();

            // Snap back should only occur if relevantPointOffset has length > 0
            if (relevantPointOffset.LengthSquared > 0)
            {
                // Snap the fractal back by the offset
                SnapBack(relevantPointOffset);
            }
        }

        #endregion

        #region Viewbox Manipulation

        //==========================================================//
        /// <summary>
        /// Animate the viewport by the specidied adjustment.
        /// </summary>
        /// <param name="adjustment">The amount of adjustment to apply to the viewport.</param>
        private void SnapBack(Vector adjustment)
        {
            // Create the animation that will move the viewbox
            PointAnimation viewportAnimation = ((PointAnimation)Resources["ViewportAnimation"]).Clone();
            viewportAnimation.By = new Point(adjustment.X, adjustment.Y);

            // Get the fractal name adjustment. Can't use the viewport matrix to transform becuase that will
            // include translation, and the adjustment only wants the rotation and scale values.
            Vector rotatedAdjustment = GeometryHelper.Rotate(adjustment, -Fractal.Viewport.Rotation);
            rotatedAdjustment *= Fractal.Viewport.ZoomFactor;

            // Create the animations that move the fractal name to the correct position
            DoubleAnimation nameXAnimation = ((DoubleAnimation)Resources["NameAnimation"]).Clone();
            nameXAnimation.By = -rotatedAdjustment.X;

            DoubleAnimation nameYAnimation = ((DoubleAnimation)Resources["NameAnimation"]).Clone();
            nameYAnimation.By = -rotatedAdjustment.Y;

            // Set up the callbacks to release the points from animation
            viewportAnimation.Completed += delegate(object s, EventArgs args)
            {
                ReleasePropertyFromAnimation(Fractal, Viewport.OffsetProperty);
            };

            nameXAnimation.Completed += delegate(object s, EventArgs args)
            {
                ReleasePropertyFromAnimation(FractalNameContainer, Canvas.LeftProperty);
            };

            nameYAnimation.Completed += delegate(object s, EventArgs args)
            {
                ReleasePropertyFromAnimation(FractalNameContainer, Canvas.TopProperty);
            };
            
            // Start all the animations
            Fractal.BeginAnimation(Viewport.OffsetProperty, viewportAnimation);
            FractalNameContainer.BeginAnimation(Canvas.LeftProperty, nameXAnimation);
            FractalNameContainer.BeginAnimation(Canvas.TopProperty, nameYAnimation);
        }
 
        //==========================================================//
        /// <summary>
        /// Releases a property that is being animated from the animation, allowing it to be modified
        /// by other methods, like manipulation delta events.
        /// </summary>
        /// <param name="element">The object that is being animated</param>
        /// <param name="dependencyProperty">The dependency property for the point that is being animated</param>
        private static void ReleasePropertyFromAnimation(UIElement element, DependencyProperty dependencyProperty)
        {
            // When the property is released from animation, it will go back to its un-animated value.
            // Save the value and reset it once the animation is removed.
            object newValue = element.GetValue(dependencyProperty);
            element.BeginAnimation(dependencyProperty, null);
            element.SetValue(dependencyProperty, newValue);
        }

        #endregion

        #region Math

        //==========================================================//
        /// <summary>
        /// Restrict the scale that will be applied to the viewbox so it does not 
        /// scale past the scale limits.
        /// </summary>
        /// <param name="suggestedScale">The scale suggested be the interia or manipulation processor.</param>
        /// <returns>The maximum scale that can be applied to the viewbox.</returns>
        public double ConstrainScale(double suggestedScale)
        {
            if (Fractal.Viewport.ZoomFactor * suggestedScale < minZoom)
            {
                return minZoom / Fractal.Viewport.ZoomFactor;
            }
            
            if (Fractal.Viewport.ZoomFactor * suggestedScale > maxZoom)
            {
                return maxZoom / Fractal.Viewport.ZoomFactor;
            }

            return suggestedScale;
        }

        //==========================================================//
        /// <summary>
        /// Scales translation if the viewport is in the elastic region.
        /// </summary>
        /// <param name="suggestedTranslation">The translation suggested by the inertia or manipulation processor.</param>
        /// <returns>A vector that represents the correct amount of translation to apply to the viewport.</returns>
        public Vector ConstrainTranslation(Vector suggestedTranslation)
        {
            Rect fractalBounds = new Rect ( 0, 0, Fractal.ActualWidth, Fractal.ActualHeight );

            // Get the point to compare to the boundary
            Vector relevantPointOffset = GetRelevantPointOffset();

            // Translation should be scaled only if relevantPointOffset has length > 0
            if (relevantPointOffset.LengthSquared == 0)
            {
                return suggestedTranslation;
            }

            // This translation will eventually be rotated, so rotate it before any scaling is applied to it.
            Vector rotatedTranslation = GeometryHelper.Rotate(suggestedTranslation, Fractal.Viewport.Rotation);

            // Not interested in pos/neg values, just the absolute value of each component of the offset
            relevantPointOffset.X = Math.Abs(relevantPointOffset.X);
            relevantPointOffset.Y = Math.Abs(relevantPointOffset.Y);

            // Find the max offset, then find the percentage the offset is of the max offset.
            Vector maxOffset = new Vector(Fractal.Viewport.RotatedWidth * 0.1, Fractal.Viewport.RotatedHeight * 0.1);

            double xScalar = Math.Max(0, 1 - (relevantPointOffset.X / maxOffset.X));
            double yScalar = Math.Max(0, 1 - (relevantPointOffset.Y / maxOffset.Y));

            Vector scaledTranslation = new Vector(rotatedTranslation.X * xScalar, rotatedTranslation.Y * yScalar);

            // Rotate it back because whoever called this method expects unrotated translation
            return GeometryHelper.Rotate(scaledTranslation, -Fractal.Viewport.Rotation);
        }

        //==========================================================//
        /// <summary>
        /// Determines the point to use in calculations to determine how 
        /// far out of bounds the fractal content is.
        /// </summary>
        /// <returns>
        /// The closest point outside of the viewport that must be in the viewport, 
        /// or a point conaining double.NaN if the viewport is in a valid position.
        /// </returns>
        private Vector GetRelevantPointOffset()
        {
            // See how many of the viewport corners are contained in the Fractal drawing area
            List<Point> containedPoints = new List<Point>();

            if (Fractal.Bounds.Contains(Fractal.Viewport.TopLeft))
            {
                containedPoints.Add(Fractal.Viewport.TopLeft);
            }

            if (Fractal.Bounds.Contains(Fractal.Viewport.TopRight))
            {
                containedPoints.Add(Fractal.Viewport.TopRight);
            }

            if (Fractal.Bounds.Contains(Fractal.Viewport.BottomLeft))
            {
                containedPoints.Add(Fractal.Viewport.BottomLeft);
            }

            if (Fractal.Bounds.Contains(Fractal.Viewport.BottomRight))
            {
                containedPoints.Add(Fractal.Viewport.BottomRight);
            }

            // What to do next depends on how many corners are contained
            if (containedPoints.Count >= 3)
            {
                // Content must be in a valid position
                return new Vector(0, 0);
            }

            else if (containedPoints.Count == 2)
            {
                Point allowedPoint1 = Fractal.Viewport.GetAllowedRegionCorner(containedPoints[0]);
                Point allowedPoint2 = Fractal.Viewport.GetAllowedRegionCorner(containedPoints[1]);

                if (Fractal.Bounds.Contains(allowedPoint1) || Fractal.Bounds.Contains(allowedPoint2))
                {
                    return new Vector(0, 0);
                }

                // Neither allowed point is in the fractal border, see which one is closer
                Vector allowedPointOneOffset = GetOffsetFromFractalRegion(allowedPoint1);
                Vector allowedPointTwoOffset = GetOffsetFromFractalRegion(allowedPoint2);

                Point relevantPoint = allowedPointOneOffset.LengthSquared < allowedPointTwoOffset.LengthSquared ? allowedPoint1 : allowedPoint2;
                return GetOffsetFromFractalRegion(relevantPoint);
            }

            else if (containedPoints.Count == 1)
            {
                Point relevantPoint = Fractal.Viewport.GetAllowedRegionCorner(containedPoints[0]);
                return GetOffsetFromFractalRegion(relevantPoint);
            }

            else
            {
                // See if  the viewport contains any corners of the fractal border
                List<Point> containedCorners = FindFractalCornersContainedInViewport();

                if (containedCorners.Count == 1)
                {
                    Point intersection;

                    if (GeometryHelper.Intersect(Fractal.Center, containedCorners[0], Fractal.Viewport.TopLeft, Fractal.Viewport.TopRight, out intersection))
                    {
                        return GetOffsetFromFractalRegion(Fractal.Viewport.GetAllowedRegionCorner(intersection));
                    }

                    if (GeometryHelper.Intersect(Fractal.Center, containedCorners[0], Fractal.Viewport.TopRight, Fractal.Viewport.BottomRight, out intersection))
                    {
                        return GetOffsetFromFractalRegion(Fractal.Viewport.GetAllowedRegionCorner(intersection));
                    }

                    if (GeometryHelper.Intersect(Fractal.Center, containedCorners[0], Fractal.Viewport.BottomRight, Fractal.Viewport.BottomLeft, out intersection))
                    {
                        return GetOffsetFromFractalRegion(Fractal.Viewport.GetAllowedRegionCorner(intersection));
                    }

                    if (GeometryHelper.Intersect(Fractal.Center, containedCorners[0], Fractal.Viewport.BottomLeft, Fractal.Viewport.TopLeft, out intersection))
                    {
                        return GetOffsetFromFractalRegion(Fractal.Viewport.GetAllowedRegionCorner(intersection));
                    }

                    return new Vector(0, 0);
                }

                // Process for 0 contained corners or 2 or more is the same
                else
                {
                    // Find the closest allowable point and use that
                    Point allowedPoint1 = Fractal.Viewport.GetAllowedRegionCorner(Fractal.Viewport.TopLeft);
                    Point allowedPoint2 = Fractal.Viewport.GetAllowedRegionCorner(Fractal.Viewport.TopRight);
                    Point allowedPoint3 = Fractal.Viewport.GetAllowedRegionCorner(Fractal.Viewport.BottomLeft);
                    Point allowedPoint4 = Fractal.Viewport.GetAllowedRegionCorner(Fractal.Viewport.BottomRight);

                    Point[] points = new Point[] { allowedPoint1, allowedPoint2, allowedPoint3, allowedPoint4 };
                    Vector closestOffset = new Vector(double.MaxValue, double.MaxValue);
                    for (int i = 0; i < points.Length; i++)
                    {
                        Vector offset = GetOffsetFromFractalRegion(points[i]);
                        if (offset.LengthSquared < closestOffset.LengthSquared)
                        {
                            closestOffset = offset;
                        }
                    }

                    return closestOffset;
                }
            }
        }

        //==========================================================//
        /// <summary>
        /// Determines how far outside of the fractal bounds a point is.
        /// </summary>
        /// <param name="point">The point to compare to the fractal bounds.</param>
        /// <returns>A vector that represents the point's distance from the fractal bounds.</returns>
        private Vector GetOffsetFromFractalRegion(Point point)
        {
            Vector offset = new Vector(0, 0);

            if (point.X < Fractal.Bounds.Left)
            {
                offset.X += Fractal.Bounds.Left - point.X;
            }

            if (point.X > Fractal.Bounds.Right)
            {
                offset.X += Fractal.Bounds.Right - point.X;
            }

            if (point.Y < Fractal.Bounds.Top)
            {
                offset.Y += Fractal.Bounds.Top - point.Y;
            }

            if (point.Y > Fractal.Bounds.Bottom)
            {
                offset.Y += Fractal.Bounds.Bottom - point.Y;
            }

            return offset;
        }

        //==========================================================//
        /// <summary>
        /// Finds all the corners of the fractal region contained in the fractal viewport.
        /// </summary>
        /// <returns>A list that contains the coordinates of all contained corners.</returns>
        private List<Point> FindFractalCornersContainedInViewport()
        {
            // A list to track the contained points
            List<Point> containedPoints = new List<Point>();

            // Rotate the viewport backwards to cancel any rotation currently applied to it
            Point origin = Fractal.Viewport.Center;
            Point ViewportTLRotated = GeometryHelper.Rotate(Fractal.Viewport.TopLeft, origin, -Fractal.Viewport.Rotation);
            Point ViewportBRRotated = GeometryHelper.Rotate(Fractal.Viewport.BottomRight, origin, -Fractal.Viewport.Rotation);
            Rect viewport = new Rect(ViewportTLRotated, ViewportBRRotated);

            // Rotate the fractal corners. If any of them are contained in the rotated viewport, add them to the list.
            if (viewport.Contains(GeometryHelper.Rotate(Fractal.Bounds.TopLeft, origin, -Fractal.Viewport.Rotation)))
            {
                containedPoints.Add(Fractal.Bounds.TopLeft);
            }
            if (viewport.Contains(GeometryHelper.Rotate(Fractal.Bounds.TopRight, origin, -Fractal.Viewport.Rotation)))
            {
                containedPoints.Add(Fractal.Bounds.TopRight);
            }
            if (viewport.Contains(GeometryHelper.Rotate(Fractal.Bounds.BottomLeft, origin, -Fractal.Viewport.Rotation)))
            {
                containedPoints.Add(Fractal.Bounds.BottomLeft);
            }
            if (viewport.Contains(GeometryHelper.Rotate(Fractal.Bounds.BottomRight, origin, -Fractal.Viewport.Rotation)))
            {
                containedPoints.Add(Fractal.Bounds.BottomRight);
            }

            return containedPoints;
        }

        #endregion
    }
}
