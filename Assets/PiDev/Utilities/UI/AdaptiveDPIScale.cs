using UnityEditor;
using UnityEngine;
using static PiDev.Utilities.UI.AdaptiveLayoutMode;
using System.Reflection;

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
namespace PiDev.Utilities.UI
{
    [ExecuteAlways]
    public class AdaptiveDPIScale : MonoBehaviour, IAdaptiveAspectRatioElement
    {
        public enum ScaleMode { PC, MobileHorizontal, MobileVertical }

        public bool testMobile = false;

        public RectTransform view;
        public Vector3 pcScale = Vector3.one;
        public Vector3 mobileHorizontalScale = Vector3.one;
        public Vector3 mobileVerticalScale = Vector3.one;

        private void OnEnable()
        {
            if (view == null)
                view = transform.parent as RectTransform;

            UpdateScale();
        }

        private void OnValidate()
        {
            UpdateScale();
        }

        private void Update()
        {
            UpdateScale();
        }

        private void UpdateScale()
        {
            float dpi = Screen.dpi;
            if (dpi <= 0)
            {
                dpi = Application.isMobilePlatform ? 160f : 96f;
            }

            float aspectRatio = view != null ? view.rect.width / view.rect.height : (float)Screen.width / Screen.height;
            bool isMobile = IsMobilePlatform();

            ScaleMode scaleMode = !isMobile ? ScaleMode.PC : (aspectRatio > 1.0f ? ScaleMode.MobileHorizontal : ScaleMode.MobileVertical);

            switch (scaleMode)
            {
                case ScaleMode.PC:
                    transform.localScale = pcScale;
                    break;
                case ScaleMode.MobileHorizontal:
                    transform.localScale = mobileHorizontalScale;
                    break;
                case ScaleMode.MobileVertical:
                    transform.localScale = mobileVerticalScale;
                    break;
            }
        }

        private bool IsMobilePlatform()
        {
#if UNITY_EDITOR
            return testMobile;
#else
        return Application.isMobilePlatform;
#endif
        }

        public void SetView(RectTransform view)
        {
            this.view = view;
            UpdateScale();
        }
    }
}