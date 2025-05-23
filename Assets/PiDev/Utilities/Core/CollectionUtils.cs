using System;
using System.Collections;
using System.Collections.Generic;
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
 * Extension methods for safe element access in arrays, lists, and dictionaries.
 * Includes utilities for default value returns and a simple weighted random selection.
 * Useful for robust indexing and probability-based item picking in collections.
 *
 * ============= Usage =============
 * array.GetOrDefault(index, defaultValue);
 * list.GetOrDefault(index, defaultValue);
 * dictionary.GetOrDefault(key, fallbackValue);
 * dictionary.GetOrNull(key);
 * weightedList.GetByWeight(); // ICollection<Utils.Weighted<T>>
 */

namespace PiDev
{
    public static partial class Utils
    {
        public static T GetOrDefault<T>(this System.Array arr, int index, T def)
        {
            if (index < 0) return def;
            if (index >= arr.Length) return def;
            return (T)arr.GetValue(index);
        }

        public static T GetOrDefault<T>(this List<T> arr, int index, T def)
        {
            if (index < 0) return def;
            if (index >= arr.Count) return def;
            return (T)arr.ToArray().GetValue(index);
        }

        public static V GetOrDefault<K, V>(this Dictionary<K, V> dict, K key, V def)
        {
            if (dict == null) return def;
            V val;
            if (dict.TryGetValue(key, out val)) return val;
            return def;
        }

        public static V GetOrNull<K, V>(this Dictionary<K, V> dict, K key)
        {
            if (dict == null) return default(V);
            V val;
            if (dict.TryGetValue(key, out val)) return val;
            return default(V);
        }


        // Weighted Random
        [Serializable]
        public class Weighted<T>
        {
            public T item;
            public float weight;
        }
        public static T GetByWeight<T>(this ICollection<Weighted<T>> collection)
        {
            return collection.GetRandomElementByWeight(x => x.weight).item;
        }

        // Serializable Dictionary
        [Serializable]
        public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
        {
            [Serializable] public struct Entry { public TKey Key; public TValue Value; }
            [SerializeField] private List<Entry> entries = new();

            public Dictionary<TKey, TValue> dictionary = new();

            public void OnBeforeSerialize()
            {
                entries.Clear();
                foreach (var kv in dictionary)
                    entries.Add(new Entry { Key = kv.Key, Value = kv.Value });
            }

            public void OnAfterDeserialize()
            {
                dictionary = new Dictionary<TKey, TValue>();
                foreach (var e in entries)
                {
                    if (!dictionary.ContainsKey(e.Key))
                        dictionary[e.Key] = e.Value;
                }
            }
        }

        // Serializable list that is displayed as a table in Inspector
        [Serializable]
        public class TableList<T> : IList<T>
        {
            [SerializeField] List<T> items;
            public T this[int index] { get => ((IList<T>)items)[index]; set => ((IList<T>)items)[index] = value; }
            public int Count => ((ICollection<T>)items).Count;
            public bool IsReadOnly => ((ICollection<T>)items).IsReadOnly;
            public void Add(T item) => ((ICollection<T>)items).Add(item);
            public void Clear() => ((ICollection<T>)items).Clear();
            public bool Contains(T item) => ((ICollection<T>)items).Contains(item);
            public void CopyTo(T[] array, int arrayIndex) => ((ICollection<T>)items).CopyTo(array, arrayIndex);
            public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)items).GetEnumerator();
            public int IndexOf(T item) => ((IList<T>)items).IndexOf(item);
            public void Insert(int index, T item) => ((IList<T>)items).Insert(index, item);
            public bool Remove(T item) => ((ICollection<T>)items).Remove(item);
            public void RemoveAt(int index) => ((IList<T>)items).RemoveAt(index);
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)items).GetEnumerator();
        }

    }
}