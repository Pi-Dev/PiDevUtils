#if UNITY_EDITOR
using UnityEditor;
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
 * Custom Unity property attribute and drawer allowing float fields to easily be set to NaN via an inspector button.
 * Useful for optional or undefined float values in components without needing extra boolean flags.
 *
 * ============= Usage =============
 * [NaNField] public float optionalValue;
 * Press the "NaN" button next to the field in the Inspector to set it to float.NaN.
 */

namespace PiDev.Utilities
{
    public sealed class NaNField : PropertyAttribute { }

    [CustomPropertyDrawer(typeof(NaNField))]
    public sealed class NaNFloatField : PropertyDrawer
    {
        const int buttonWidth = 40;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var fieldpos = position;
            fieldpos.width -= buttonWidth;
            float f = property.floatValue;
            f = EditorGUI.FloatField(fieldpos, label, property.floatValue);
            property.floatValue = f;
            var buttonpos = position;
            buttonpos.x = position.width - buttonWidth + 18;
            buttonpos.width = buttonWidth;
            if (GUI.Button(buttonpos, "NaN")) property.floatValue = float.NaN;
        }
    }
}
#endif