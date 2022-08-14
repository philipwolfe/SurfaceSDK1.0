using System;
using System.Windows;
using System.Windows.Media;

namespace FractalBrowser
{
    /// <summary>
    /// A class that draws a fractal.
    /// </summary>
    public class HFractal : FrameworkElement
    {
        #region Private Members

        int maxDepth;

        private Pen[] pens = new Pen[] { 
            new Pen (Brushes.Wheat, 2),
            new Pen (Brushes.Pink, 2),
            new Pen (Brushes.LightGreen, 2),
            new Pen (Brushes.LightSkyBlue, 2),
            new Pen (Brushes.RosyBrown, 2)};

        Viewport viewport;

        // Size of individual lines decreases by sqrt(2) with each new level of recursion.
        private double logBase = Math.Sqrt(2);

        #endregion

        #region Public Properties

        //==========================================================//
        /// <summary>
        /// Gets the center of the fractal in screen corrdinates.
        /// </summary>
        public Point Center
        {
            get
            {
                return new Point(ActualWidth / 2, ActualHeight / 2);
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets the bounding rect of the fractal in screen coordinates.
        /// </summary>
        public Rect Bounds
        {
            get
            {
                return new Rect(0, 0, ActualWidth, ActualHeight);
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets the viewport that looks into the fractal.
        /// </summary>
        internal Viewport Viewport
        {
            get
            {
                // Make sure a viewport exists
                if (viewport == null)
                {
                    viewport = new Viewport(this);
                    viewport.Resize(new Size(ActualWidth, ActualHeight));
                }

                return viewport;
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets the point at which the fractal name should be placed.
        /// </summary>
        internal Point FractalNamePoint
        {
            get
            {
                return Viewport.ConvertViewportCoordinateToParentCoordinate(new Point(ActualWidth / 2, ActualHeight * 5 / 8));
            }
        }

        #endregion

        #region Initalization

        //==========================================================//
        /// <summary>
        /// Constructor.
        /// </summary>
        public HFractal():base()
        {

        }

        #endregion

        #region Layout and Rendering

        //==========================================================//
        /// <summary>
        /// Override of OnRender.
        /// </summary>
        /// <param name="drawingContext">The DrawingContext being used to render the object.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            // Update maxDepth so the recursion goes down to the proper depth
            RecalculateMaxDepth();

            // Calculate the two points 
            Point p1 = new Point(ActualWidth / 4, ActualHeight / 2);
            Point p2 = new Point(ActualWidth * 3 / 4, ActualHeight / 2);

            // Draw the line
            DrawLine(drawingContext, p1, p2, 0);

            // Start recursion 
            DrawFractal(drawingContext, new Rect ( 0, 0, ActualWidth, ActualHeight ), 1);
        }

        //==========================================================//
        /// <summary>
        /// Recursively draws the fractal.
        /// </summary>
        /// <param name="drawingContext">The DrawingContext being used to render the object.</param>
        /// <param name="rectangle">The rectangle inside of which to draw the fractal.</param>
        /// <param name="depth">The current level of recursion.</param>
        private void DrawFractal(DrawingContext drawingContext, Rect rectangle, int depth)
        {
            if (depth < maxDepth + 9)
            {
                // Split the rectangle in half
                Rect rect1 = new Rect(rectangle.X, rectangle.Y, depth % 2 == 0 ? rectangle.Width : rectangle.Width / 2, depth % 2 == 1 ? rectangle.Height : rectangle.Height / 2);
                Rect rect2 = new Rect(rectangle.X + (depth % 2 == 0 ? 0 : rectangle.Width / 2), rectangle.Y + (depth % 2 == 0 ? rectangle.Height / 2 : 0), depth % 2 == 0 ? rectangle.Width : rectangle.Width / 2, depth % 2 == 1 ? rectangle.Height : rectangle.Height / 2);

                // Get the centers of each rectangle
                Point p1 = new Point(rect1.X + rect1.Width / 2, rect1.Y + rect1.Height / 2);
                Point p2 = new Point(rect2.X + rect2.Width / 2, rect2.Y + rect2.Height / 2);

                // Draw a line between the center points
                DrawLine(drawingContext, p1, p2, depth);

                // Recurse down rect1, if it is visible
                if (Viewport.ViewportOverlapsRectangle(rect1))
                {
                    DrawFractal(drawingContext, rect1, depth + 1);
                }

                // Recurse down rect2, if it is visible
                if (Viewport.ViewportOverlapsRectangle(rect2))
                {
                    DrawFractal(drawingContext, rect2, depth + 1);
                }
            }
        }

        //==========================================================//
        /// <summary>
        /// Draws a line segment between 2 points.
        /// </summary>
        /// <param name="drawingContext">The DrawingContext being used to render the object.</param>
        /// <param name="p1">The starting point of the line.</param>
        /// <param name="p2">The end point of the line.</param>
        /// <param name="depth">The current level of recursion.</param>
        private void DrawLine(DrawingContext drawingContext, Point p1, Point p2, int depth)
        {
            // Translate the points through the viewport to get their final screen coordinates
            Point p1Translated = Viewport.ConvertViewportCoordinateToParentCoordinate(p1);
            Point p2Translated = Viewport.ConvertViewportCoordinateToParentCoordinate(p2);

            // Set the pen's color
            Pen pen = pens[depth % pens.Length];

            // Draw the line.
            drawingContext.DrawLine(pen, p1Translated, p2Translated);
        }

        #endregion

        #region Math

        //==========================================================//
        /// <summary>
        /// Recalculates the maximum recursion depth based on the size 
        /// of the fractal and the size of the viewbox.
        /// </summary>
        private void RecalculateMaxDepth()
        {
            maxDepth = (int)Math.Log(ActualWidth / Viewport.RotatedWidth, logBase);
        }

        #endregion
    }
}
