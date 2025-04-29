using System;
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
 * Adaptive layout switcher that toggles between horizontal and vertical layouts based on screen aspect ratio.
 * Supports activating different GameObjects for each mode and dynamically copying children across containers.
 * Designed for responsive UI layouts where orientation affects structure and placement.
 *
 * ============= Usage =============
 * Attach AdaptiveLayoutMode to a container, assign horizontal/vertical roots, and optionally auto-populate child items.
 * Layout transitions automatically based on screen aspect ratio or forced mode.
 */

namespace PiDev.Utilities.UI
{
    [ExecuteInEditMode]
    [AddComponentMenu("Layout/Adaptive Layout Mode")]
    public class AdaptiveLayoutMode : MonoBehaviour
    {
        public interface IAdaptiveAspectRatioElement
        {
            void SetView(RectTransform view);
        }

        public RectTransform view;

        public enum LayoutMode { Automatic, Horizontal, Vertical }
        public LayoutMode layoutMode = LayoutMode.Automatic;

        bool oldIsVertical;

        [Tooltip("Reference to the GameObject used in horizontal mode.")]
        public GameObject horizontalObject;

        [Tooltip("Reference to the GameObject used in vertical mode.")]
        public GameObject verticalObject;

        [Header("Content settings")]
        [Tooltip("Reference to the items collection, it will be filled when orientation is changed")]
        public RectTransform ContentContainer;
        public RectTransform HorizontalContentRoot, VerticalContentRoot;

        private void Awake()
        {
            view = view != null ? view : GetComponentInParent<RectTransform>();
            UpdateLayout();
            PopulateItems();
        }

        private void Update()
        {
            bool changed = UpdateLayout();
            if (changed || !Application.isPlaying) PopulateItems();
        }

        public void PopulateItems()
        {
            if (ContentContainer == null || HorizontalContentRoot == null || VerticalContentRoot == null)
                return;
            RectTransform targetContainerRoot = DetermineLayout() ? VerticalContentRoot : HorizontalContentRoot;
            if (targetContainerRoot.childCount > 0)
            {
                for (int i = targetContainerRoot.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(targetContainerRoot.GetChild(i).gameObject);
                }
            }
            string s = "";
            for (int i = 0; i < ContentContainer.childCount; i++)
            {
                var template = ContentContainer.GetChild(i);
                if (!template.gameObject.activeSelf) continue;
                s += template.name + ", ";
                var shadow = Instantiate(template, targetContainerRoot);
                shadow.name = $"[inst: {template.name}]";
                shadow.gameObject.hideFlags = HideFlags.DontSave;
                shadow.gameObject.SetActive(true);
                var ars = shadow.GetComponentsInChildren<IAdaptiveAspectRatioElement>();
                foreach (var ar in ars) ar.SetView(view);
                //shadow.SetParent(targetContainerRoot, false);
            }
        }

        private bool UpdateLayout()
        {
            bool shouldUseVertical = DetermineLayout();

            if (horizontalObject != null)
                horizontalObject.SetActive(!shouldUseVertical);

            if (verticalObject != null)
                verticalObject.SetActive(shouldUseVertical);

            bool changed = oldIsVertical != shouldUseVertical;
            oldIsVertical = shouldUseVertical;
            return changed;
        }

        private bool DetermineLayout()
        {
            if (layoutMode == LayoutMode.Horizontal)
                return false;

            if (layoutMode == LayoutMode.Vertical)
                return true;

            var rt = GetComponent<RectTransform>();
            return rt.rect.width / rt.rect.height <= 1;
        }
    }
}