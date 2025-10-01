using UnityEngine.UI;

/* Public Domain - 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * ============= Description =============
 * A lightweight UI component that acts as a raycast target without rendering any visual mesh.
 * Useful for invisible UI interaction zones or for blocking clicks behind transparent areas.
 *
 * ============= Usage =============
 * Add to a UI GameObject to make its RectTransform respond to raycasts without drawing anything.
 */

namespace PiDev.Utilities
{
    public class RaycastTarget : Graphic
    {
        public override void SetMaterialDirty() { return; }
        public override void SetVerticesDirty() { return; }
    }
}