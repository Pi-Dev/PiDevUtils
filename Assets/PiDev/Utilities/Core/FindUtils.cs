using System.Collections.Generic;
using System.Linq;
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
 * Utility extensions for finding transforms, components, and objects based on proximity.
 * Includes support for tags, component types, and interface implementations.
 * Also provides tools to fetch grandchild transforms and all objects of a type.
 *
 * ============= Usage =============
 * transform.FindGrandChild("ChildName");
 * Utils.GetClosestPoint(origin, listOfPoints);
 * Utils.GetClosestNumber(originValue, floatValues);
 * Utils.GetClosestObject(origin, objects); // GameObjects or Transforms
 * Utils.GetClosestObjectWithTag(origin, "Enemy");
 * Utils.GetAllComponents<UnityEngine.UI.Image>();
 * Utils.GetClosestObjectWithComponent<MyComponent>(origin);
 * Utils.GetClosestComponent<MyComponent>(origin);
 * Utils.GetClosestComponent(origin, allowedList);
 * Utils.GetClosestObjectImplementingInterface<IMyInterface>(origin, 10f, excludeList);
 */

namespace PiDev
{
    public static partial class Utils
    {
        static public Transform FindGrandChild(this Transform fromGameObject, string withName, bool includeInactive = true)
        {
            Transform[] ts = fromGameObject.GetComponentsInChildren<Transform>(includeInactive);
            foreach (Transform t in ts) if (t.gameObject.name == withName) return t;
            return null;
        }

        public static Vector3 GetClosestPoint(Vector3 origin, IEnumerable<Vector3> points)
        {
            Vector3 tMin = Vector3.negativeInfinity;
            float minDist = float.PositiveInfinity;
            foreach (var p in points)
            {
                float dist = Vector3.Distance(p, origin);
                if (dist < minDist)
                {
                    tMin = p;
                    minDist = dist;
                }
            }
            return tMin;
        }

        public static float GetClosestNumber(float origin, IEnumerable<float> points)
        {
            float tMin = float.NegativeInfinity;
            float minDist = float.PositiveInfinity;
            foreach (var p in points)
            {
                float dist = Mathf.Abs(p - origin);
                if (dist < minDist)
                {
                    tMin = p;
                    minDist = dist;
                }
            }
            return tMin;
        }

        public static Transform GetClosestObject(Vector3 origin, IEnumerable<GameObject> objects, float maxDist = float.PositiveInfinity)
        {
            Transform tMin = null;
            float minDist = float.PositiveInfinity;
            Vector3 currentPos = origin;
            foreach (GameObject t in objects)
            {
                float dist = Vector3.Distance(t.transform.position, currentPos);
                if (dist > maxDist) continue;
                if (dist < minDist)
                {
                    tMin = t.transform;
                    minDist = dist;
                }
            }
            return tMin;
        }

        public static Transform GetClosestObject(Vector3 origin, IEnumerable<Transform> objects)
        {
            Transform tMin = null;
            float minDist = float.PositiveInfinity;
            Vector3 currentPos = origin;
            foreach (Transform t in objects)
            {
                float dist = Vector3.Distance(t.position, currentPos);
                if (dist < minDist)
                {
                    tMin = t;
                    minDist = dist;
                }
            }
            return tMin;
        }

        public static Transform GetClosestObjectWithTag(Vector3 origin, string tag)
        {
            return GetClosestObject(origin, GameObject.FindGameObjectsWithTag(tag));
        }

        public static T[] GetAllComponents<T>() where T : UnityEngine.Object
        {
            return Resources.FindObjectsOfTypeAll<T>();
        }

        public static Transform GetClosestObjectWithComponent<T>(Vector3 origin) where T : Component
        {
            var objs = UnityEngine.Object.FindObjectsOfType<T>();
            List<GameObject> gameobjects = new List<GameObject>();
            foreach (var c in objs)
            {
                gameobjects.Add(c.gameObject);
            }
            return GetClosestObject(origin, gameobjects.ToArray());
        }

        public static T GetClosestComponent<T>(Vector3 origin) where T : Component
        {
            var objs = UnityEngine.Object.FindObjectsOfType<T>();
            List<GameObject> gameobjects = new List<GameObject>();
            foreach (var c in objs)
            {
                gameobjects.Add(c.gameObject);
            }
            var co = GetClosestObject(origin, gameobjects.ToArray());
            if (co != null) return co.GetComponent<T>();
            else return null;
        }

        public static T GetClosestComponent<T>(Vector3 origin, IEnumerable<T> allowedComponents) where T : Component
        {
            T closestComponent = null;
            float closestDistance = float.MaxValue;
            foreach (var component in allowedComponents)
            {
                if (component == null) continue;
                float distance = Vector3.Distance(origin, component.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestComponent = component;
                }
            }
            return closestComponent;
        }

        public static T GetClosestObjectImplementingInterface<T>(Vector3 origin, float maxDist, List<GameObject> exclude = null) where T : class
        {
            var ss = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().OfType<T>().ToList();
            var objects = new List<GameObject>();
            foreach (var c in ss)
            {
                GameObject go = (c as MonoBehaviour).gameObject;
                if (exclude == null) objects.Add(go);
                else if (!exclude.Contains(go)) objects.Add(go);
            }
            return GetClosestObject(origin, objects, maxDist)?.GetComponent<T>();
        }
    }
}