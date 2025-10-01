using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting;

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
 */

namespace PiDev.Utilities
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class FixedSizeLayoutElement : MonoBehaviour, ILayoutElement
    {
        [NaNField]
        public float width = float.NaN;

        [NaNField]
        public float height = float.NaN;

        public int layoutPriority = 1;

        public void CalculateLayoutInputHorizontal() { }

        public void CalculateLayoutInputVertical() { }

        public float minWidth => float.IsNaN(width) ? -1 : width;
        public float preferredWidth => float.IsNaN(width) ? -1 : width;
        public float flexibleWidth => 1;

        public float minHeight => float.IsNaN(height) ? -1 : height;
        public float preferredHeight => float.IsNaN(height) ? -1 : height;
        public float flexibleHeight => 1;

        int ILayoutElement.layoutPriority => layoutPriority;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                LayoutRebuilder.MarkLayoutForRebuild(GetComponent<RectTransform>());
            }
        }
#endif
    }
}