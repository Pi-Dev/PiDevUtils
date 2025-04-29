using System;
using System.Collections.Generic;
using UnityEngine;

/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 * 
 * The MIT License (MIT)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * ============= Description =============
 * Distributes a configurable number of points in 3D space based on selected shape: Line, Circle, or Path.
 * Supports centering, custom overrides, and integration with external path providers like DoTweenPath.
 * Automatically visualizes the distribution in the Unity Editor when selected.
 *
 * ============= Usage =============
 * var points = pointDistributor.GetPoints(count);
 * Supports override sets for exact point configurations and Catmull-Rom interpolation for path shape.
 */

namespace PiDev.Utilities
{
    public class PointDistributor : MonoBehaviour
    {
        public enum DistributionShape { Line, Circle, Path }
        public DistributionShape distributionShape = DistributionShape.Line;

        public Vector3 axis = new Vector3(1, 0, 0); // Axis along which the points will be spaced
        public bool center; // Whether to center the points around the origin

        public List<Vector3> path = new List<Vector3>(); // Control points for the Catmull-Rom spline

        [Serializable] public struct CustomOverride { public int count; public Vector3[] points; }
        public List<CustomOverride> customOverrides;

        public Vector3[] GetPoints(int count)
        {
            foreach (var t in customOverrides) if (t.count == count) return t.points;

            count = Mathf.Max(1, count);
            var res = new Vector3[count];

            if (distributionShape == DistributionShape.Line)
            {
                float offset = center ? -(count - 1) / 2f : 0f;
                for (int i = 0; i < count; i++)
                {
                    Vector3 localPosition = offset * axis + i * axis; // Local space position
                    res[i] = transform.TransformPoint(localPosition); // Convert to world space
                }
            }
            else if (distributionShape == DistributionShape.Circle)
            {
                Vector3 normalizedAxis = axis.normalized;
                float radius = axis.magnitude;

                Vector3 tangent = Vector3.Cross(normalizedAxis, Vector3.forward).normalized;
                if (tangent == Vector3.zero)
                {
                    tangent = Vector3.Cross(normalizedAxis, Vector3.up).normalized;
                }
                Vector3 bitangent = Vector3.Cross(normalizedAxis, tangent).normalized;

                float angleOffset = center ? Mathf.PI / count : 0f;
                for (int i = 0; i < count; i++)
                {
                    float angle = 2 * Mathf.PI * i / count + angleOffset;
                    Vector3 localPosition = radius * (Mathf.Cos(angle) * tangent + Mathf.Sin(angle) * bitangent);
                    res[i] = transform.TransformPoint(localPosition);
                }
            }
            else if (distributionShape == DistributionShape.Path)
            {
                // Fetch path from DoTweenPath if available
                var dtp = GetComponent<IPointsProvider>();
                if (dtp != null) path = dtp.GetPoints();
                var points = PiDev.Utils.PathSplineCatmullRom(path, false);

                if (points.Count == 0)
                {
                    res[0] = transform.position;
                    return res;
                }

                // Special case: Return midpoint if count == 1
                if (count == 1)
                {
                    float midIndex = (points.Count - 1) / 2f; // Midpoint index
                    int floorIndex = Mathf.FloorToInt(midIndex);
                    int ceilIndex = Mathf.CeilToInt(midIndex);
                    float lerpFactor = midIndex - floorIndex;

                    res[0] = Vector3.Lerp(points[floorIndex], points[ceilIndex], lerpFactor); // Interpolate if fractional
                    return res;
                }

                // General case: Distribute evenly along the path indices
                float step = (points.Count - 1f) / (count - 1f); // Fractional step between points
                for (int i = 0; i < count; i++)
                {
                    float targetIndex = i * step;
                    int floorIndex = Mathf.FloorToInt(targetIndex);
                    int ceilIndex = Mathf.CeilToInt(targetIndex);

                    // Ensure indices stay within bounds
                    floorIndex = Mathf.Clamp(floorIndex, 0, points.Count - 1);
                    ceilIndex = Mathf.Clamp(ceilIndex, 0, points.Count - 1);

                    float lerpFactor = targetIndex - floorIndex; // Fractional part
                    res[i] = Vector3.Lerp(points[floorIndex], points[ceilIndex], lerpFactor); // Interpolate
                }

                return res;
            }
            return res;
        }

        void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                var points = GetPoints(Mathf.Max(1, previewPoints));
                foreach (var point in points)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(point, 0.5f);
                }
            }
        }

        public int previewPoints = 4; // Number of points to preview in editor
    }
}