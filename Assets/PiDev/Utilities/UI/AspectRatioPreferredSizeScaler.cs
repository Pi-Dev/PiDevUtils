using System.Collections;
using System.Collections.Generic;
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
 * UI layout element that dynamically scales its preferred size based on aspect ratio and fitting mode.
 * Supports absolute sizing, width-based, height-based, or auto-adaptive fitting based on parent dimensions.
 * Integrates with Unity's layout system for flexible, responsive UI designs.
 *
 * ============= Usage =============
 * Attach AspectRatioPreferredSizeScaler to a UI element and configure fittingMode and aspectRatio.
 * Automatically updates layout each frame to adapt to screen or container size changes.
 */

namespace PiDev.Utilities.UI
{
    [ExecuteAlways]
    public class AspectRatioPreferredSizeScaler : MonoBehaviour, ILayoutElement, IAdaptiveAspectRatioElement
    {
        public enum FittingMode { Absolute, FitWidth, FitHeight, AutoFit }

        public FittingMode fittingMode = FittingMode.Absolute;
        public float aspectRatio = 1.0f;
        [NaNField] public float width = float.NaN;
        [NaNField] public float height = float.NaN;
        public int LayoutPriority = 1;

        public RectTransform view;

        private void OnEnable()
        {
            view = view != null ? view : transform.parent.GetComponent<RectTransform>();
            UpdateLayout();
        }

        private void OnValidate()
        {
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        }

        void Update()
        {
            UpdateLayout();
        }

        public float minWidth => 0;
        public float preferredWidth { get; set; }
        public float flexibleWidth => 0;
        public float minHeight => 0;
        public float preferredHeight { get; set; }
        public float flexibleHeight => 0;
        public int layoutPriority => LayoutPriority;

        public void CalculateLayoutInputHorizontal()
        {
            if (view == null) return;

            float viewWidth = view.rect.width;
            float viewHeight = view.rect.height;
            float viewAspectRatio = viewWidth / viewHeight;
            FittingMode resolvedMode = fittingMode;

            if (fittingMode == FittingMode.AutoFit)
            {
                resolvedMode = viewAspectRatio > 1 ? FittingMode.FitHeight : FittingMode.FitWidth;
            }

            switch (resolvedMode)
            {
                case FittingMode.Absolute:
                    if (float.IsNaN(width) && !float.IsNaN(height))
                    {
                        preferredWidth = height * aspectRatio;
                        preferredHeight = height;
                    }
                    else if (!float.IsNaN(width) && float.IsNaN(height))
                    {
                        preferredWidth = width;
                        preferredHeight = width / aspectRatio;
                    }
                    else if (float.IsNaN(width) && float.IsNaN(height))
                    {
                        preferredWidth = aspectRatio;
                        preferredHeight = 1;
                    }
                    break;

                case FittingMode.FitWidth:
                    if (viewWidth > 0)
                    {
                        preferredWidth = viewWidth;
                        preferredHeight = viewWidth / aspectRatio;
                    }
                    break;

                case FittingMode.FitHeight:
                    if (viewHeight > 0)
                    {
                        preferredHeight = viewHeight;
                        preferredWidth = viewHeight * aspectRatio;
                    }
                    break;
            }
        }

        public void CalculateLayoutInputVertical()
        {
            CalculateLayoutInputHorizontal();
        }

        public void SetView(RectTransform view)
        {
            this.view = view;
        }
    }
}