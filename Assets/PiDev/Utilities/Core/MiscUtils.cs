using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

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
 * Miscellaneous utility functions for component management, layer handling, hashing, and string checks.
 * Includes support for AnimationClip to PlayableGraph conversion and enumerable iteration shortcuts.
 *
 * ============= Usage =============
 * var mc = gameObject.GetOrAddComponent<MyComponent>();
 * gameObject.SetLayerRecursively(targetLayer);
 * if (layer.IsInLayerMask(mask)) { ... }
 * if (string.ContainsAny("check", "a", "b", "c")) { ... }
 * myList.Each(item => Debug.Log(item));
 * var graph = myClip.CreatePlayableGraph(targetObject);
 */


namespace PiDev
{
    public static partial class Utils
    {
        public static T GetOrAddComponent<T>(this GameObject o) where T : Component
        {
            T x = o.GetComponent<T>();
            if (x != null) return x;
            return o.AddComponent<T>();
        }

        public static bool IsInLayerMask(this int layer, LayerMask layermask)
        {
            return layermask == (layermask | (1 << layer));
        }

        public static void SetLayerRecursively(this GameObject obj, int layer, int maxLevels = 100)
        {
            SetLayerRecursivelyInternal(obj, layer, maxLevels, 0);
        }

        private static void SetLayerRecursivelyInternal(GameObject obj, int layer, int maxLevels, int currentLevel)
        {
            if (currentLevel > maxLevels) return;

            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursivelyInternal(child.gameObject, layer, maxLevels, currentLevel + 1);
            }
        }

        public static void Swap<T>(ref T a, ref T b) where T : class
        {
            T c = a;
            a = b;
            b = c;
        }

        public static PlayableGraph CreatePlayableGraph(this AnimationClip clip, GameObject target, DirectorUpdateMode updateMode = DirectorUpdateMode.GameTime)
        {
            PlayableGraph playableGraph = PlayableGraph.Create("Graph-" + clip.name);
            playableGraph.SetTimeUpdateMode(updateMode);
            var playableOutput = AnimationPlayableOutput.Create(playableGraph, clip.name, target.GetComponent<Animator>());
            var clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
            playableOutput.SetSourcePlayable(clipPlayable);
            return playableGraph;
        }

        public static int hash32i(string source)
        {
            const int MULTIPLIER = 36;
            int h = 0;
            for (int i = 0; i < source.Length; ++i)
                h = MULTIPLIER * h + source[i];
            return h;
        }

        public static void Each<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
                action(item);
        }

        public static bool ContainsAny(this string input, params string[] stringsToCheck)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            foreach (var value in stringsToCheck)
            {
                if (string.IsNullOrEmpty(value))
                    continue;

                if (input.Contains(value))
                    return true;
            }

            return false;
        }
    }
}