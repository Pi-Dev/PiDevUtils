// Copyright (c) 2010 Bob Berkebile
//
// Modified by ~PeterSvP
//  REWORKED to work with DOTween
//  Made initial points at the transform position and close to it, 
//  added a +Add button - adds a point next the last one,
//  added Drop field where you can drop scene object to add its transform as a point,
//  And DELETE buttons for each point to be able to delete it
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

namespace PiDev.Utilities
{
    [CustomEditor(typeof(DoTweenPath))]
    public class DoTweenPathEditor : UnityEditor.Editor
    {
        DoTweenPath _target;
        GUIStyle style = new GUIStyle();
        public static int count = 0;
        static GameObject drop;
        static string dropName = "\n\nDrop object to add its pos\n\n";    // the (Transform) is out of view and it's centered :D

        void OnEnable()
        {
            //i like bold handle labels since I'm getting old:
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;
            _target = (DoTweenPath)target;

            //lock in a default path name:
            if (!_target.initialized)
            {
                _target.initialized = true;
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            Undo.RecordObject(_target, "Edit Path");

            // Space before the node count
            EditorGUILayout.Space();

            //exploration segment count control:
            EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.PrefixLabel("Node Count");
            _target.nodeCount = Mathf.Max(2, EditorGUILayout.IntField("Node Count", _target.nodeCount));
            //_target.nodeCount =  Mathf.Clamp(EditorGUILayout.IntSlider(_target.nodeCount, 0, 10), 2,100);
            EditorGUILayout.EndHorizontal();

            //add node?
            if (_target.nodeCount > _target.nodes.Count)
            {
                // if noone, add small tween from current to +1 forward instead of (0,0,0)
                if (_target.nodes.Count == 0)
                {
                    _target.nodes.Add(_target.localSpacePoints ? Vector3.zero : _target.transform.position);
                    _target.nodes.Add(_target.localSpacePoints ? Vector3.up : _target.transform.position + Vector3.forward);
                }
                // Else add close to last
                else for (int i = 0; i < _target.nodeCount - _target.nodes.Count; i++)
                    {
                        int c = _target.nodes.Count;
                        _target.nodes.Add(Vector3.LerpUnclamped(_target.nodes[c - 2], _target.nodes[c - 1], 1.5f));
                    }
            }

            //remove node?
            if (_target.nodeCount < _target.nodes.Count)
            {
                if (EditorUtility.DisplayDialog("Remove path node?", "Shortening the node list will permantently destory parts of your path. This operation cannot be undone.", "OK", "Cancel"))
                {
                    int removeCount = _target.nodes.Count - _target.nodeCount;
                    _target.nodes.RemoveRange(_target.nodes.Count - removeCount, removeCount);
                }
                else
                {
                    _target.nodeCount = _target.nodes.Count;
                }
            }

            //node display:
            bool disableDelete = _target.nodeCount <= 2;
            for (int i = 0; i < _target.nodes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label((i + 1) + ":", GUILayout.MaxWidth(20));
                _target.nodes[i] = EditorGUILayout.Vector3Field("", _target.nodes[i]);

                GUI.enabled = i < _target.nodes.Count - 1;
                if (GUILayout.Button("M", GUILayout.MaxWidth(20)))
                {
                    Vector3 mid = (_target.nodes[i] + _target.nodes[i + 1]) / 2;
                    _target.nodes.Insert(i + 1, mid); _target.nodeCount++; return;
                }
                GUI.enabled = !disableDelete;
                if (GUILayout.Button("X", GUILayout.MaxWidth(20))) { _target.nodes.RemoveAt(i); _target.nodeCount--; return; }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }

            // Utility buttons feature - add new close to last or add from scene object
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add")) _target.nodeCount++;

            // Hack to dislay custom text into the ObjectField
            if (drop == null)
            {
                drop = new GameObject();
                drop.hideFlags = HideFlags.HideAndDontSave;
                drop.name = dropName;
            }

            // If something was selected, add its position
            Transform changed = EditorGUILayout.ObjectField(drop, typeof(Transform), true) as Transform;
            if (changed != null)
            {
                _target.nodeCount++;
                _target.nodes.Add(changed.position);
            }
            EditorGUILayout.EndHorizontal();


            // Whole path offsetting feature
            EditorGUILayout.BeginHorizontal();
            var diff = EditorGUILayout.Vector3Field("Offset path (Moves all points): ", Vector3.zero);
            if (diff != Vector3.zero)
                for (int i = 0; i < _target.nodes.Count; i++)
                    _target.nodes[i] += diff;

            EditorGUILayout.EndHorizontal();
            EditorUtility.SetDirty(_target);
        }

        void OnSceneGUI()
        {
            Undo.RecordObject(_target, "DoTweenPath");
            if (_target.pathVisible && _target.nodes.Count > 0)
            {
                var points = _target.GetWorldNodes();

                //node handle display:
                for (int i = 0; i < points.Count; i++)
                {
                    var op = points[i];
                    points[i] = Handles.PositionHandle(points[i], Quaternion.identity);
                    if (op != points[i]) EditorUtility.SetDirty(_target);
                }
                for (int i = 1; i < points.Count; i++)
                {
                    var op = points[i];
                    var bp = points[i - 1];
                    var pos = (op + bp) / 2;
                    Handles.color = _target.pathColor;
                    if (Handles.Button(pos, Quaternion.identity, 0.2f, .25f, Handles.SphereHandleCap))
                    {
                        points.Insert(i, pos);
                        _target.nodeCount++;
                        _target.SetWorldNodes(points);
                        EditorUtility.SetDirty(_target);
                        break;
                    }
                    Handles.color = Color.white;
                }
                _target.SetWorldNodes(points);
            }
        }
    }
}