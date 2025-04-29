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
 * Adaptive Layout Group that automatically switches between horizontal and vertical layouts based on aspect ratio.
 * Provides fully customizable layout settings for each mode including padding, alignment, spacing, and child controls.
 * Dynamically creates and applies either a HorizontalLayoutGroup or VerticalLayoutGroup component at runtime.
 *
 * ============= Usage =============
 * Attach AdaptiveLayoutGroup to a GameObject and configure horizontalSettings and verticalSettings.
 * Layout mode will switch automatically unless explicitly set to Horizontal or Vertical.
 */

namespace PiDev.Utilities.UI
{

    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class AdaptiveLayoutGroup : MonoBehaviour, IAdaptiveAspectRatioElement
    {
        public RectTransform view;
        public enum LayoutMode { Automatic, Horizontal, Vertical }
        public LayoutMode layoutMode = LayoutMode.Automatic;

        [Serializable]
        public class LayoutSettings
        {
            public float spacing = 10f;
            public RectOffset padding;
            public TextAnchor alignment = TextAnchor.UpperLeft;
            public bool childForceExpandWidth = true;
            public bool childForceExpandHeight = false;
            public bool controlChildWidth = false;
            public bool controlChildHeight = false;
            public bool useChildScaleWidth = false;
            public bool useChildScaleHeight = false;
        }

        public LayoutSettings horizontalSettings;
        public LayoutSettings verticalSettings;

        private HorizontalLayoutGroup horizontalLayout;
        private VerticalLayoutGroup verticalLayout;

        private void Awake()
        {
            view = view != null ? view : GetComponentInParent<RectTransform>();
            if (horizontalSettings == null) horizontalSettings = new();
            if (verticalSettings == null) verticalSettings = new();
            EnsureLayoutGroups();
            UpdateLayout();
        }

        private void Update()
        {
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            bool shouldUseHorizontal = DetermineLayout();

            if (shouldUseHorizontal)
            {
                if (horizontalLayout == null)
                {
                    horizontalLayout = gameObject.AddComponent<HorizontalLayoutGroup>();
                    if (horizontalLayout) horizontalLayout.hideFlags = HideFlags.DontSave;
                }

                if (verticalLayout != null)
                    DestroyImmediate(verticalLayout);

                ApplySettings(horizontalLayout, horizontalSettings);
            }
            else
            {
                if (verticalLayout == null)
                {
                    verticalLayout = gameObject.AddComponent<VerticalLayoutGroup>();
                    if (verticalLayout) verticalLayout.hideFlags = HideFlags.DontSave;
                }

                if (horizontalLayout != null)
                    DestroyImmediate(horizontalLayout);

                ApplySettings(verticalLayout, verticalSettings);
            }
        }

        private bool DetermineLayout()
        {
            if (layoutMode == LayoutMode.Horizontal)
                return true;

            if (layoutMode == LayoutMode.Vertical)
                return false;

            return view.rect.width / view.rect.height > 1;
        }

        private void ApplySettings(LayoutGroup layoutGroup, LayoutSettings settings)
        {
            if (layoutGroup == null) return;

            layoutGroup.childAlignment = settings.alignment;

            if (layoutGroup is HorizontalLayoutGroup hlg)
            {
                hlg.spacing = settings.spacing;
                hlg.childForceExpandWidth = settings.childForceExpandWidth;
                hlg.childForceExpandHeight = settings.childForceExpandHeight;
                hlg.padding.left = settings.padding?.left ?? 0;
                hlg.padding.right = settings.padding?.right ?? 0;
                hlg.padding.top = settings.padding?.top ?? 0;
                hlg.padding.bottom = settings.padding?.bottom ?? 0;

                // Apply Control Child Size and Use Child Scale settings
                hlg.childControlWidth = settings.controlChildWidth;
                hlg.childControlHeight = settings.controlChildHeight;
                hlg.childScaleWidth = settings.useChildScaleWidth;
                hlg.childScaleHeight = settings.useChildScaleHeight;
            }
            else if (layoutGroup is VerticalLayoutGroup vlg)
            {
                vlg.spacing = settings.spacing;
                vlg.childForceExpandWidth = settings.childForceExpandWidth;
                vlg.childForceExpandHeight = settings.childForceExpandHeight;
                vlg.padding.left = settings.padding?.left ?? 0;
                vlg.padding.right = settings.padding?.right ?? 0;
                vlg.padding.top = settings.padding?.top ?? 0;
                vlg.padding.bottom = settings.padding?.bottom ?? 0;

                // Apply Control Child Size and Use Child Scale settings
                vlg.childControlWidth = settings.controlChildWidth;
                vlg.childControlHeight = settings.controlChildHeight;
                vlg.childScaleWidth = settings.useChildScaleWidth;
                vlg.childScaleHeight = settings.useChildScaleHeight;
            }
        }

        private void EnsureLayoutGroups()
        {
            horizontalLayout = GetComponent<HorizontalLayoutGroup>();
            verticalLayout = GetComponent<VerticalLayoutGroup>();

            if (horizontalLayout != null && verticalLayout != null)
            {
                DestroyImmediate(verticalLayout);
            }
        }

        public void SetView(RectTransform v)
        {
            view = v;
        }
    }
}