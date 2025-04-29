using UnityEngine;
using System.Collections.Generic;
using System;
using DG.Tweening;
using System.Linq;

/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * Based on the original iTweenPath by Bob Berkebile (c) 2010
 * but heavily modified and adapted for DOTween instead.
 * https://www.pixelplacement.com
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
 * A DOTween-compatible spline editor and runtime path evaluator.
 * Supports local/world-space nodes, live gizmo previews, looping, speed/time-based paths, and dynamic evaluation.
 * Integrates with DOTween to render and follow animation paths during edit or runtime.
 *
 * ============= Usage =============
 * Attach to a GameObject and define nodes visually in the inspector.
 * Use DoTweenPathFollow component or GetWorldNodes() to animate along the defined path.
 */

namespace PiDev.Utilities
{
    [AddComponentMenu("Animation/DoTweenPath")]
    public class DoTweenPath : MonoBehaviour, IPointsProvider
    {
        public Color pathColor = Color.cyan;
        public bool loop;
        public LoopType loopType;
        public PathType pathType;
        public bool localSpacePoints = true;
        public bool moveToPath = true;

        public enum ValueType { Time, Speed }
        public ValueType valueType = ValueType.Time;
        public float value = 5;
        public Ease forwardsEase = Ease.Linear, backwardsEase = Ease.Linear;

        [HideInInspector]
        public List<Vector3> nodes = new List<Vector3>();
        [HideInInspector] public int nodeCount;


        public static Dictionary<string, DoTweenPath> paths = new Dictionary<string, DoTweenPath>();
        [NonSerialized] public bool initialized = false;
        [NonSerialized] public string initialName = "";
        [NonSerialized] public bool pathVisible = true;

        public List<Vector3> GetWorldNodes(bool reverse = false)
        {
            var ls = new List<Vector3>();
            Vector3 point;
            foreach (var p in nodes)
            {
                point = localSpacePoints ? transform.position + p : p;
                if (reverse)
                    ls.Insert(0, point);
                else ls.Add(point);
            }
            return ls;
        }
        public List<Vector3> GetPoints() => GetWorldNodes();

        static GameObject PathEvaluator = null;
        public List<Vector3> evaluatePathPoints()
        {
            // Make path & get render points
            if (PathEvaluator == null)
            {
                PathEvaluator = new GameObject("[DoTweenPath] PathEvaluator");
                PathEvaluator.hideFlags = HideFlags.HideAndDontSave;
            }
            var ls = GetWorldNodes();
            if (moveToPath == false) PathEvaluator.transform.position = nodes[0];
            else PathEvaluator.transform.position = transform.position;
            var path = PathEvaluator.transform.DOPath(ls.ToArray(), 0, pathType, PathMode.Ignore);
            path.ForceInit();
            ls = path.PathGetDrawPoints().ToList();
            DOTween.Kill(path.id, false);
            return ls;
        }

        public void SetWorldNodes(List<Vector3> worldPoints)
        {
            nodes.Clear();
            foreach (var p in worldPoints)
                nodes.Add(localSpacePoints ? p - transform.position : p);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (pathVisible)
            {
                if (nodes.Count > 0)
                {
                    EditorDraw(this);
                }
            }
        }

        public static void EditorDraw(DoTweenPath p)
        {
            if (Application.isPlaying) return;

            Gizmos.color = p.pathColor;
            Vector3 prev = Vector3.zero;
            EditorDrawNoColor(p);
        }
        public static void EditorDrawNoColor(DoTweenPath p)
        {
            if (Application.isPlaying) return;
            Vector3 prev = Vector3.zero;

            var points = p.evaluatePathPoints();

            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0) prev = points[i];
                else
                {
                    Gizmos.DrawLine(prev, points[i]);
                    Gizmos.DrawWireSphere(points[i], 0.05f);
                    prev = points[i];
                }
            }
        }
#endif

        /// <summary>
        /// Returns the visually edited path as a Vector3 array.
        /// </summary>
        /// <param name="requestedName">
        /// A <see cref="System.String"/> the requested name of a path.
        /// </param>
        /// <returns>
        /// A <see cref="Vector3[]"/>
        /// </returns>
        public static Vector3[] GetPath(string requestedName)
        {
            requestedName = requestedName.ToLower();
            if (paths.ContainsKey(requestedName))
            {
                return paths[requestedName].nodes.ToArray();
            }
            else
            {
                Debug.Log("No path with that name (" + requestedName + ") exists! Are you sure you wrote it correctly?");
                return null;
            }
        }

        /// <summary>
        /// Returns the reversed visually edited path as a Vector3 array.
        /// </summary>
        /// <param name="requestedName">
        /// A <see cref="System.String"/> the requested name of a path.
        /// </param>
        /// <returns>
        /// A <see cref="Vector3[]"/>
        /// </returns>
        public static Vector3[] GetPathReversed(string requestedName)
        {
            requestedName = requestedName.ToLower();
            if (paths.ContainsKey(requestedName))
            {
                List<Vector3> revNodes = paths[requestedName].nodes.GetRange(0, paths[requestedName].nodes.Count);
                revNodes.Reverse();
                return revNodes.ToArray();
            }
            else
            {
                Debug.Log("No path with that name (" + requestedName + ") exists! Are you sure you wrote it correctly?");
                return null;
            }
        }

        ///dummy
        private void Start()
        {

        }
    }
}