using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Random = UnityEngine.Random;

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
 * Random utility functions for selecting items from arrays, lists, and weighted sequences.
 * Includes helpers for ranged values using Vector2 and basic random picking via Choose().
 * Useful for loot drops, randomized behaviors, and probability-based logic.
 *
 * ============= Usage =============
 * var item = Utils.Choose("A", "B", "C");
 * var fromList = myList.GetRandomElement();
 * var weighted = items.GetRandomElementByWeight(i => i.weight);
 * float value = rangeVector2.RandomRange(); // rangeVector2 = new Vector2(min, max)
 */

namespace PiDev
{
    public static partial class Utils
    {
        // Randoms
        public static T Choose<T>(params T[] x) => x[Random.Range(0, x.Length)];
        public static T Choose<T>(IList<T> x) => x[Random.Range(0, x.Count)];
        public static int Choose(params int[] x) => x[Random.Range(0, x.Length)];
        public static float RandomRange(this Vector2 v) => Random.Range(v.x, v.y);

        public static T GetRandomElement<T>(this IList<T> collection)
        {
            return collection[Random.Range(0, collection.Count)];
        }

        public static T GetRandomElementByWeight<T>(this IEnumerable<T> sequence, Func<T, float> weightSelector)
        {
            float totalWeight = sequence.Sum(weightSelector);
            float itemWeightIndex = Random.value * totalWeight;
            float currentWeightIndex = 0;
            foreach (var item in from weightedItem in sequence select new { Value = weightedItem, Weight = weightSelector(weightedItem) })
            {
                currentWeightIndex += item.Weight;
                if (currentWeightIndex >= itemWeightIndex)
                    return item.Value;
            }
            return default(T);
        }
    }
}