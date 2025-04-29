using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* Public Domain - 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * ============= Description =============
 * Utility methods for generating and evaluating Catmull-Rom splines based on control points.
 * Supports both looping and non-looping splines with optional spacing or resolution control.
 * Ideal for smooth path generation, animation tracks, or camera rails.
 *
 * Based on the Catmull-Rom spline formulation by Paul Bourke.
 * See: http://paulbourke.net/miscellaneous/interpolation/
 */

namespace PiDev
{
    public static partial class Utils
    {

        /// <summary>
        /// Catmull Rom Spline equation estimation
        /// </summary>
        /// <param name="t">0...1, the percentage</param>
        /// <param name="p0">prev point</param>
        /// <param name="p1">current point</param>
        /// <param name="p2">next point</param>
        /// <param name="p3">next+1 point</param>
        /// <returns></returns>
        public static Vector3 CatmullRom(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 a = 2f * p1;
            Vector3 b = p2 - p0;
            Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

            //The cubic polynomial: a + b * t + c * t^2 + d * t^3
            Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

            return pos;
        }

        /// <summary>
        /// Renders a catmull-rom spline from list of control points
        /// </summary>
        /// <param name="controlPoints">List with control points</param>
        /// <param name="loop"></param>
        /// <param name="useLengths">If true, length of segment will be used, res is spacing</param>
        /// <param name="resolution">Resolution. num points between segments unless useLengths is true - in which case, spacing</param>
        /// <returns></returns>
        public static List<Vector3> PathSplineCatmullRom(List<Vector3> controlPoints, bool loop, bool useLengths = false, float resolution = 10)
        {
            // We will iterate this dataset to draw control points;
            List<Vector3> SplineDataset = new List<Vector3>();

            // Prepare data for path spline
            if (loop)
            {
                SplineDataset.Add(controlPoints.Last());
                SplineDataset.AddRange(controlPoints);
                SplineDataset.Add(controlPoints.First());
            }
            else
            {
                SplineDataset.Add(controlPoints.First());
                SplineDataset.AddRange(controlPoints);
                SplineDataset.Add(controlPoints.Last());
            }

            // Prepare result with first control point used as is!
            var res = new List<Vector3>();
            res.Add(SplineDataset[1]);

            // outer loop: Control points
            for (int i = 1; i < SplineDataset.Count - 2; i++)
            {
                int p0 = i - 1;
                int p1 = i + 0;
                int p2 = i + 1;
                int p3 = i + 2;
                float p;
                if (!useLengths) p = 1 / resolution;
                else
                {
                    var l = Vector3.Distance(SplineDataset[p1], SplineDataset[p2]);
                    p = 1 / l * resolution;
                }

                if (p <= float.Epsilon || float.IsInfinity(p) || float.IsNaN(p))
                {
                    res.Add(SplineDataset[p2]); // hang defense, just add next control point
                    continue;
                }
                float t = p; // 0 will not be iterated
                while (t < 1)
                {
                    res.Add(CatmullRom(t, SplineDataset[p0], SplineDataset[p1], SplineDataset[p2], SplineDataset[p3]));
                    t += p;
                }
                res.Add(CatmullRom(1, SplineDataset[p0], SplineDataset[p1], SplineDataset[p2], SplineDataset[p3]));
            }
            return res;
        }
    }
}