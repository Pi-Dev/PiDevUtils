using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting;

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