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
 * Automatically scales the preferred size of a Unity UI Image based on width, height, or scale settings.
 * Implements ILayoutElement to integrate with layout groups and dynamically adjust sizing based on image aspect ratio.
 * Useful for maintaining consistent image proportions in flexible and adaptive UI layouts.
 *
 * ============= Usage =============
 * Attach to a GameObject with an Image component and set width, height, or scale values.
 * Preferred size will be calculated and applied to layout automatically.
 */

namespace PiDev.Utilities.UI
{

    [ExecuteAlways]
    public class ImagePreferredSizeScaler : MonoBehaviour, ILayoutElement
    {
        Image image;
        public float scale = 1;
        [NaNField] public float width = float.NaN;
        [NaNField] public float height = float.NaN;
        public int LayoutPriority = 1;

        private void OnEnable()
        {
            image = GetComponent<Image>();
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

        public float minWidth => image.minWidth;

        public float preferredWidth { get; set; }

        public float flexibleWidth => image.flexibleWidth;

        public float minHeight => image.minHeight;

        public float preferredHeight { get; set; }

        public float flexibleHeight => image.flexibleHeight;

        public int layoutPriority => LayoutPriority;

        public void CalculateLayoutInputHorizontal()
        {
            if (float.IsNaN(width) && !float.IsNaN(height))
            {
                float ratio = height / image.sprite.rect.height;
                preferredWidth = image.sprite.rect.width * ratio;
                preferredHeight = height;
            }
        }

        public void CalculateLayoutInputVertical()
        {
            if (!float.IsNaN(width) && float.IsNaN(height))
            {
                float ratio = width / image.sprite.rect.width;
                preferredWidth = width;
                preferredHeight = image.sprite.rect.height * ratio;
            }

            else if (float.IsNaN(width) && float.IsNaN(height))
            {
                preferredWidth = image.sprite.rect.width * scale;
                preferredHeight = image.sprite.rect.height * scale;
            }
        }

    }
}