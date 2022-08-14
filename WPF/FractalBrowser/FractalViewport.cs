using System;
using System.Windows;
using System.Windows.Media;

namespace FractalBrowser
{
    /// <summary>
    /// An internal class that assists in translating points inside a FrameworkElement to screen coordinates.
    /// </summary>
    internal class Viewport
    {
        #region DependencyProperties

        /// <summary>
        /// A dependency property that lets the viewport be animated. Changing this property translates
        // the viewport by the delta between the old and new values. The deltas are the important things
        // here, the actual value of this property means nothing and never has any impact on anything.
        /// </summary>
        internal static readonly DependencyProperty OffsetProperty = DependencyProperty.RegisterAttached(
            "Offset",
            typeof(Point),
            typeof(Viewport),
            new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback (OnOffsetChanged)));

        //==========================================================//
        /// <summary>
        /// Callback for the property changed event for the offset property.
        /// </summary>
        /// <param name="dependencyObject">The dependency object that fired the event.</param>
        /// <param name="args">The arguments for the event.</param>
        private static void OnOffsetChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            HFractal fractal = (HFractal)dependencyObject;
            Vector delta = (Point)args.NewValue - (Point)args.OldValue;

            fractal.Viewport.Translate(delta);
        }

        #endregion

        #region PrivateMembers

        private Point initalCenter;
        private Point initalTopLeft;
        private Point initalTopRight;
        private Point initalBottomLeft;
        private Point initalBottomRight;

        private Matrix transformation = Matrix.Identity;

        private FrameworkElement parent;

        // Content may be anywhere within the inner 80% of the viewport. However, 
        // this measurement is performed as a vector from the center of the viewport. 
        // 60% of Center -> Edge = 80% of Edge -> Other edge.
        private const double allowedRegionRatio = 0.6;

        #endregion

        #region Internal Properties

        //==========================================================//
        /// <summary>
        /// Gets the ratio of the viewport size to the parent size.
        /// </summary>
        internal double ZoomFactor
        {
            get
            {
                // Make a vector from the top left to the top right
                Vector longSide = new Vector(TopRight.X - TopLeft.X, TopRight.Y - TopLeft.Y);
                return parent.ActualWidth / Math.Abs(longSide.Length);
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets the amount of rotation that has been applied to the viewport.
        /// </summary>
        internal double Rotation
        {
            get
            {
                return Vector.AngleBetween ( new Vector ( 1, 0 ), transformation.Transform(new Vector(1, 0)));
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets the distance between the top left and bottom left of the viewport. This is not the height of the 
        /// bounding box, it is calculated using the points after rotation has been applied.
        /// </summary>
        internal double RotatedHeight
        {
            get
            {
                Vector side = BottomLeft - TopLeft;
                return side.Length;
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets the distance between the top left and top right of the viewport. This is not the width of the 
        /// bounding box, it is calculated using the points after rotation has been applied.
        /// </summary>
        internal double RotatedWidth
        {
            get
            {
                Vector side = TopRight - TopLeft;
                return side.Length;
            }
        }

        //==========================================================//
        /// <summary>
        ///Gets the matrix that accumulates transformations applied to the viewport.
        /// </summary>
        internal Matrix Transformation
        {
            get
            {
                return transformation;
            }
        }


        //==========================================================//
        /// <summary>
        /// Gets the center of the viewport.
        /// </summary>
        internal Point Center
        {
            get
            {
                return transformation.Transform(initalCenter);

            }
        }

        //==========================================================//
        /// <summary>
        /// Gets the top left of the viewport.
        /// </summary>
        internal Point TopLeft
        {
            get
            {
                return transformation.Transform(initalTopLeft);
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets the top right of the viewport.
        /// </summary>
        internal Point TopRight
        {
            get
            {
                return transformation.Transform(initalTopRight);
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets the bottom left of the viewport.
        /// </summary>
        internal Point BottomLeft
        {
            get
            {
                return transformation.Transform(initalBottomLeft);
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets the bottom right of the viewport.
        /// </summary>
        internal Point BottomRight
        {
            get
            {
                return transformation.Transform(initalBottomRight);
            }
        }

        #endregion

        #region Initalization

        //==========================================================//
        /// <summary>
        /// Constructor for the viewport.
        /// </summary>
        /// <param name="parent">The FrameworkElement into which this viewport provides a view.</param>
        internal Viewport (FrameworkElement viewportParent)
        {
            parent = viewportParent;
        }

        //==========================================================//
        /// <summary>
        /// Removes all rotation and sets viewport to a specific size.
        /// </summary>
        /// <param name="finalSize">The size which the viewport should take up.</param>
        internal void Resize (Size allowedSize)
        {
            initalCenter = new Point(allowedSize.Width / 2, allowedSize.Height / 2);
            initalTopLeft = new Point(0, 0);
            initalTopRight = new Point(allowedSize.Width, 0);
            initalBottomLeft = new Point(0, allowedSize.Height);
            initalBottomRight = new Point(allowedSize.Width, allowedSize.Height);
        }

        #endregion

        #region PointComparison

        //==========================================================//
        /// <summary>
        /// Determines if the viewport overlaps a rectangle.
        /// </summary>
        /// <returns>True if the viewport overlaps the rectangle, false otherwise</returns>
        internal bool ViewportOverlapsRectangle(Rect rectangle )
        {
            // Get a bounding rectangle for the viewport
            Point boundsTopLeft = new Point(Math.Min(Math.Min(TopLeft.X, TopRight.X), Math.Min(BottomLeft.X, BottomRight.X)),
                                              Math.Min(Math.Min(TopLeft.Y, TopRight.Y), Math.Min(BottomLeft.Y, BottomRight.Y)));
            Point boundsBottomRight = new Point(Math.Max(Math.Max(TopLeft.X, TopRight.X), Math.Max(BottomLeft.X, BottomRight.X)),
                                                  Math.Max(Math.Max(TopLeft.Y, TopRight.Y), Math.Max(BottomLeft.Y, BottomRight.Y)));
            Rect bounds = new Rect(boundsTopLeft, boundsBottomRight);

            // Intersect the two bounding boxes to see if they overlap.
            rectangle.Intersect(bounds);

            return !rectangle.IsEmpty;
        }

        //==========================================================//
        /// <summary>
        /// Converts a coordinate relative to the parent to a coordinate that 
        /// is in the same position relative to the viewport.
        /// </summary>
        /// <param name="screenCoordinate">The point that is relative to the parent.</param>
        /// <returns>A point is in the same position relative to the viewport</returns>
        internal Point ConvertParentCoordinateToViewportCoordinate(Point screenCoordinate)
        {
            return transformation.Transform(screenCoordinate);
        }

        //==========================================================//
        /// <summary>
        /// Converts a coordinate relative to the viewport to a coordinate that 
        /// is in the same position relative to the parent.
        /// </summary>
        /// <param name="point">The point that is relative to the viewport.</param>
        /// <returns>A point is in the same position relative to the screen</returns>
        internal Point ConvertViewportCoordinateToParentCoordinate(Point point)
        {
            Matrix inverse = transformation;
            inverse.Invert();
            return inverse.Transform(point);
        }

        #endregion

        #region Viewport Manipulation

        //==========================================================//
        /// <summary>
        /// Rotates the viewport around a point.
        /// </summary>
        /// <param name="rotation">The amount of rotation to apply to the viewport.</param>
        /// <param name="origin">The point around which to rotate the viewport.</param>
        internal void RotateAroundPoint(double rotation, Point origin)
        {
            transformation.RotateAt(rotation, origin.X, origin.Y);
            parent.InvalidateVisual();
        }

        //==========================================================//
        /// <summary>
        /// Scales the viewport around a point.
        /// </summary>
        /// <param name="scale">The amount of scale to apply to the viewport.</param>
        /// <param name="origin">The point around which to scale the viewport.</param>
        internal void ScaleAroundPoint(double scale, Point origin)
        {
            transformation.ScaleAt(scale, scale, origin.X, origin.Y);
            parent.InvalidateVisual();
        }

        //==========================================================//
        /// <summary>
        /// Translates the viewport.
        /// </summary>
        /// <param name="offset">The amount of translation to apply to the viewport.</param>
        internal void Translate(Vector offset)
        {
            transformation.Translate(offset.X, offset.Y); 
            parent.InvalidateVisual();
        }

        #endregion

        //==========================================================//
        /// <summary>
        /// Gets a point that represents the boundary of the elastic 
        /// region when given a point on the edge of the viewport.
        /// </summary>
        /// <param name="corner">A viewport corner.</param>
        /// <returns>The elastic region border associated with that corner.</returns>
        internal Point GetAllowedRegionCorner(Point corner)
        {
            // Get a Vector from the center to the corner
            Vector toCorner = new Vector(corner.X - Center.X, corner.Y - Center.Y);
            Vector toRegionCorner = toCorner * allowedRegionRatio;

            return new Point(Center.X + toRegionCorner.X, Center.Y + toRegionCorner.Y);
        }
    }
}
