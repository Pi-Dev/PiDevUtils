using System;
using UnityEngine;

/* Public Domain - 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * ============= Description =============
 * A collection of mathematical utilities and vector extensions for common Unity tasks.
 * Includes remapping, damping, snapping, rounding, and component-wise vector operations.
 * Also provides circular lerp, angle snapping, and coordinate conversion helpers.
 *
 * ============= Usage =============
 * value.RemapRanges(oldMin, oldMax, newMin, newMax);
 * pos = pos.Damp(target, smoothing, deltaTime);
 * angle = Utils.clerp(startAngle, endAngle, t);
 * vec = vec.RoundMemberwise(); // Also: FloorMemberwise(), CeilMemberwise(), AbsMemberwise()
 * float snapped = value.Snap(0.5f); // or angle = SnapAngleDeg(angle, 45f);
 * v3 = v2.xy0(); v2 = v3.xz(); max = vec3.Max3();
 */

namespace PiDev
{
    public static partial class Utils
    {
        //tries to preserve negative spaces e.g. 0..3/4 = 0,  4..7/4 = 1,  but -1...-4 = -1, -5...-8 = -2 and so on
        // this corrects that. 
        public static int nfdiv(float a, float b)
        {
            return (int)(a > 0 ? a / b : (a - b + 1) / b);
        }
        public static float nfmod(float a, float b)
        {
            return a - b * Mathf.Floor(a / b);
        }

        public static float Damp(float source, float target, float smoothing, float dt)
        {
            return Mathf.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
        }
        public static Vector3 Damp(this Vector3 source, Vector3 target, float smoothing, float dt)
        {
            return Vector3.Lerp(source, target, 1.0f - Mathf.Pow(smoothing, dt));
        }
        public static Vector4 Damp(this Vector4 source, Vector4 target, float smoothing, float dt)
        {
            return Vector4.Lerp(source, target, 1.0f - Mathf.Pow(smoothing, dt));
        }
        public static Quaternion Damp(this Quaternion source, Quaternion target, float smoothing, float dt)
        {
            return Quaternion.Lerp(source, target, 1.0f - Mathf.Pow(smoothing, dt));
        }

        public static bool Is01(this float a)
        {
            return a > 0 && a < 1;
        }

        public static float clerp(float start, float end, float value)
        {
            float min = 0.0f;
            float max = 360.0f;
            float half = Mathf.Abs((max - min) * 0.5f);
            float retval = 0.0f;
            float diff = 0.0f;
            if ((end - start) < -half)
            {
                diff = ((max - start) + end) * value;
                retval = start + diff;
            }
            else if ((end - start) > half)
            {
                diff = -((max - end) + start) * value;
                retval = start + diff;
            }
            else retval = start + (end - start) * value;
            return retval;
        }

        public static float Snap(this float value, float interval)
        {
            return Mathf.Round(value / interval) * interval;
        }


        // Vectors
        public static Vector2 xy(this Vector3 v) => new Vector2(v.x, v.y);
        public static Vector2 xz(this Vector3 v) => new Vector2(v.x, v.z);
        public static Vector2 yz(this Vector3 v) => new Vector2(v.y, v.z);
        public static Vector3 xy0(this Vector2 v) => new Vector3(v.x, v.y, 0);
        public static Vector3 xz(this Vector2 v) => new Vector3(v.x, 0, v.y);
        public static Vector3 yz(this Vector2 v) => new Vector3(0, v.x, v.y);
        public static float Max3(this Vector3 v) => Mathf.Max(v.x, v.y, v.z);
        public static float Min3(this Vector3 v) => Mathf.Min(v.x, v.y, v.z);
        public static float Max2(this Vector2 v) => Mathf.Max(v.x, v.y);
        public static float Min2(this Vector2 v) => Mathf.Min(v.x, v.y);
        public static Vector3 Round(this Vector3 vector3, int decimalPlaces = 0)
        {
            float multiplier = 1;
            for (int i = 0; i < decimalPlaces; i++)
            {
                multiplier *= 10f;
            }
            return new Vector3(
                Mathf.Round(vector3.x * multiplier) / multiplier,
                Mathf.Round(vector3.y * multiplier) / multiplier,
                Mathf.Round(vector3.z * multiplier) / multiplier);
        }

        public static Vector3 RoundMemberwise(this Vector3 src)
        {
            src.x = Mathf.Round(src.x);
            src.y = Mathf.Round(src.y);
            src.z = Mathf.Round(src.z);
            return src;
        }
        public static Vector3 FloorMemberwise(this Vector3 src)
        {
            src.x = Mathf.Floor(src.x);
            src.y = Mathf.Floor(src.y);
            src.z = Mathf.Floor(src.z);
            return src;
        }
        public static Vector3 CeilMemberwise(this Vector3 src)
        {
            src.x = Mathf.Ceil(src.x);
            src.y = Mathf.Ceil(src.y);
            src.z = Mathf.Ceil(src.z);
            return src;
        }
        public static Vector3 AbsMemberwise(this Vector3 src)
        {
            src.x = Mathf.Abs(src.x);
            src.y = Mathf.Abs(src.y);
            src.z = Mathf.Abs(src.z);
            return src;
        }

        public static Vector3 DivideMembers(this Vector3 divident, Vector3 divisor)
        {
            divident.x /= divisor.x;
            divident.y /= divisor.y;
            divident.z /= divisor.z;
            return divident;
        }
        public static Vector3 MultiplyMembers(this Vector3 src, Vector3 mul)
        {
            src.Scale(mul);
            return src;
        }

        // AB based ranges
        public static float RemapRanges(this float v, float oldMin, float oldMax, float newMin, float newMax)
        {
            return (((v - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
        }
        public static double RemapRanges(this double v, double oldMin, double oldMax, double newMin, double newMax)
        {
            return (((v - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
        }
        public static decimal RemapRanges(this decimal v, decimal oldMin, decimal oldMax, decimal newMin, decimal newMax)
        {
            return (((v - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
        }
        public static long RemapRanges(this long v, long oldMin, long oldMax, long newMin, long newMax)
        {
            return (((v - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
        }
        public static Vector3 RemapBounds(this Vector3 v, Bounds oldBounds, Bounds newBounds)
        {
            return (v - oldBounds.center).MultiplyMembers(newBounds.extents.DivideMembers(oldBounds.extents)) + newBounds.center;
        }

        public static Vector2 RoundMemberwise(this Vector2 src)
        {
            src.x = Mathf.Round(src.x);
            src.y = Mathf.Round(src.y);
            return src;
        }
        public static Vector2 FloorMemberwise(this Vector2 src)
        {
            src.x = Mathf.Floor(src.x);
            src.y = Mathf.Floor(src.y);
            return src;
        }

        public static float SnapAngleDeg(float angle, float increment)
        {
            return Mathf.Round(angle / increment) * increment;
        }
    }
}