using System.Windows;
using System.Windows.Media;

namespace FractalBrowser
{
    /// <summary>
    /// A class that contains static geometry functions.
    /// </summary>
    internal static class GeometryHelper
    {
        //==========================================================//
        /// <summary>
        /// Rotates a vector.
        /// </summary>
        /// <param name="rotateMe">The vector to be rotated.</param>
        /// <param name="degrees">The angle in degrees to rotate the vector.</param>
        /// <returns>The rotated vector.</returns>
        internal static Vector Rotate(Vector rotateMe, double degrees)
        {
            Matrix transform = Matrix.Identity;
            transform.Rotate(degrees);
            return transform.Transform(rotateMe);
        }

        //==========================================================//
        /// <summary>
        /// Rotates a point around another point.
        /// </summary>
        /// <param name="rotateMe">The point that will be rotated.</param>
        /// <param name="aroundMe">The point around which to rotate the other point.</param>
        /// <param name="degrees">The angle in degrees to rotate the point.</param>
        /// <returns>The rotated point.</returns>
        internal static Point Rotate(Point rotateMe, Point aroundMe, double degrees)
        {
            Matrix transform = Matrix.Identity;
            transform.RotateAt(degrees, aroundMe.X, aroundMe.Y);
            return transform.Transform(rotateMe);
        }

        //==========================================================//
        /// <summary>
        /// Finds the intersection of two line segments, if one exists.
        /// </summary>
        /// <remarks>
        /// This method simplifies a number of edge cases, such as overlapping lines into failure cases.
        /// For this application, it is fine, there is no need to render a line if it overlaps the edge 
        /// of the viewport. However, other applications that use this method may need to be more specific.
        /// </remarks>
        /// <param name="line1Start">The point where the first line begins.</param>
        /// <param name="line1End">The point where the first line ends.</param>
        /// <param name="line2Start">The point where the second line begins.</param>
        /// <param name="line2End">The point where the second line ends.</param>
        /// <param name="result">Parameter that will contain the intersection point after the operation completes.</param>
        /// <returns>True if an intersection exists, false otherwise.</returns>
        internal static bool Intersect(Point line1Start, Point line1End, Point line2Start, Point line2End, out Point result)
        {
            double denominator = ((line2End.Y - line2Start.Y) * (line1End.X - line1Start.X)) -
                                 ((line2End.X - line2Start.X) * (line1End.Y - line1Start.Y));

            double numeratorA = ((line2End.X - line2Start.X) * (line1Start.Y - line2Start.Y)) -
                                ((line2End.Y - line2Start.Y) * (line1Start.X - line2Start.X));

            double numeratorB = ((line1End.X - line1Start.X) * (line1Start.Y - line2Start.Y)) -
                                ((line1End.Y - line1Start.Y) * (line1Start.X - line2Start.X));

            if (denominator != 0.0)
            {
                double ua = numeratorA / denominator;
                double ub = numeratorB / denominator;

                if (ua >= 0.0 && ua <= 1.0 && ub >= 0.0 && ub <= 1.0)
                {
                    // Get the intersection point.
                    result = new Point(line1Start.X + ua * (line1End.X - line1Start.X), line1Start.Y + ua * (line1End.Y - line1Start.Y));
                    return true;
                }
            }

            result = new Point(double.NaN, double.NaN);
            return false;
        }
    }
}
