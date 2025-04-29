using UnityEditor;
using UnityEngine;
using static PiDev.Utilities.UI.AdaptiveLayoutMode;
using System.Reflection;

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