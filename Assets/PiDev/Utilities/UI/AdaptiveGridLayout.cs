using System;
using UnityEngine;
using UnityEngine.UI;
using static PiDev.Utilities.UI.AdaptiveLayoutMode;

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
 * Adaptive Grid Layout for Unity that dynamically adjusts between horizontal and vertical layouts based on aspect ratio.
 * Supports customizable settings for each layout direction, container filling, and preferred size ranges.
 * Automatically ensures a GridLayoutGroup is present and updates layout in real-time.
 *
 * ============= Usage =============
 * Attach AdaptiveGridLayout to a GameObject with a GridLayoutGroup.
 * Configure horizontalSettings and verticalSettings to control behavior per aspect mode.
 */

namespace PiDev.Utilities.UI
{
    [Serializable]
    public class AdaptiveGridSettings
    {
        public Vector2 cellSize = new Vector2(100, 100);
        public Vector2 spacing = new Vector2(10, 10);
        public RectOffset padding;
        public GridLayoutGroup.Constraint constraint;
        public int constraintCount = 2;
        public bool UsePreferredSizeRange;
        public Vector2 sizeRange = new Vector2(90, 110);
    }

    [ExecuteInEditMode]
    [AddComponentMenu("Layout/Adaptive Grid Layout")]
    public class AdaptiveGridLayout : MonoBehaviour, IAdaptiveAspectRatioElement
    {
        public RectTransform view;
        public enum LayoutMode { Automatic, Horizontal, Vertical }
        public LayoutMode layoutMode = LayoutMode.Automatic;

        public float aspectRatioThreshold = 1.0f;
        public bool fillContainer;

        public AdaptiveGridSettings horizontalSettings;
        public AdaptiveGridSettings verticalSettings;

        private GridLayoutGroup gridLayout;

        private void Awake()
        {
            view = view != null ? view : GetComponentInParent<RectTransform>();

            if (horizontalSettings == null)
                horizontalSettings = new AdaptiveGridSettings { constraint = GridLayoutGroup.Constraint.FixedRowCount };

            if (verticalSettings == null)
                verticalSettings = new AdaptiveGridSettings { constraint = GridLayoutGroup.Constraint.FixedColumnCount };

            EnsureGridLayout();
            UpdateLayout();
        }

        private void Update()
        {
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            bool isVertical = DetermineLayout();
            ApplySettings(isVertical ? verticalSettings : horizontalSettings, isVertical);
        }

        private bool DetermineLayout()
        {
            if (view == null) return false;
            if (layoutMode == LayoutMode.Horizontal)
                return false;
            if (layoutMode == LayoutMode.Vertical)
                return true;
            return view.rect.width / view.rect.height < aspectRatioThreshold;
        }

        private void ApplySettings(AdaptiveGridSettings settings, bool isVertical)
        {
            gridLayout.constraint = settings.constraint;

            if (fillContainer && view != null)
            {
                float availableSize = isVertical ? view.rect.width : view.rect.height;
                float aspectRatio = settings.cellSize.x / settings.cellSize.y;

                float paddingSize = isVertical
                    ? gridLayout.padding.left + gridLayout.padding.right
                    : gridLayout.padding.top + gridLayout.padding.bottom;

                if (settings.UsePreferredSizeRange)
                {
                    float minSize = settings.sizeRange.Min2();
                    float maxSize = settings.sizeRange.Max2();
                    settings.constraintCount = Mathf.Clamp(
                        Mathf.FloorToInt(availableSize / maxSize),
                        1,
                        Mathf.FloorToInt(availableSize / minSize)
                    );
                }

                float spacingTotal = settings.spacing.x * (settings.constraintCount - 1);
                float primarySize = (availableSize - paddingSize - spacingTotal) / settings.constraintCount;
                float secondarySize = isVertical ? primarySize / aspectRatio : primarySize * aspectRatio;

                gridLayout.cellSize = new Vector2(
                    isVertical ? primarySize : secondarySize,
                    isVertical ? secondarySize : primarySize
                );

                gridLayout.spacing = settings.spacing;
                gridLayout.constraintCount = settings.constraintCount;
            }
            else
            {
                gridLayout.cellSize = settings.cellSize;
                gridLayout.spacing = settings.spacing;
                gridLayout.constraintCount = settings.constraintCount;
            }

            gridLayout.padding.left = settings.padding?.left ?? 0;
            gridLayout.padding.right = settings.padding?.right ?? 0;
            gridLayout.padding.top = settings.padding?.top ?? 0;
            gridLayout.padding.bottom = settings.padding?.bottom ?? 0;
        }

        private void EnsureGridLayout()
        {
            gridLayout = GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
                gridLayout = gameObject.AddComponent<GridLayoutGroup>();
        }

        public void SetView(RectTransform view)
        {
            this.view = view;
        }
    }
}