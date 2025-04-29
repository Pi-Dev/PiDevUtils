using UnityEngine;

/* Public Domain - 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * ============= Description =============
 * Converts a RectTransform to a screen-space Rect based on world position and scale.
 * Useful for hit testing, UI interaction zones, or screen alignment calculations.
 *
 * ============= Usage =============
 * Rect screenRect = Utils.RectTransformToScreenSpace(myRectTransform);
 */

namespace PiDev
{
    public static partial class Utils
    {
        public static Rect RectTransformToScreenSpace(RectTransform transform)
        {
            Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
            return new Rect((Vector2)transform.position - (size * 0.5f), size);
        }

        public static Rect GetWorldSpaceRect(this RectTransform rt)
        {
            var r = rt.rect;
            r.center = rt.TransformPoint(r.center);
            r.size = rt.TransformVector(r.size);
            return r;
        }

    }
}