using UnityEngine;
using System.Collections.Generic;
using System;

/* Public Domain - 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * ============= Description =============
 * Generic object pooling system that manages reuse of instances through customizable generation, recycling, and destruction functions.
 * Reduces allocation and improves performance for repeated instantiations of frequently used objects.
 *
 * ============= Usage =============
 * var pool = new ObjectPooler<MyType>();
 * pool.funcGenerate = () => new MyType();
 * var item = pool.Buy();
 * pool.Recycle(item);
 */

namespace PiDev.Utilities
{
    public class ObjectPooler<T>
    {

        // Generator function that will generate new object
        public delegate T ObjectGenerateFunc();
        public delegate void ObjectReinitFunc(T obj);
        public delegate void ObjectRecycleFunc(T obj);
        public delegate void ObjectDestroyFunc(T obj);

        public ObjectGenerateFunc funcGenerate;
        public ObjectReinitFunc funcReinit;
        public ObjectRecycleFunc funcRecycle;
        public ObjectReinitFunc funcDestroy;

        // The Queue pool of objects
        public Queue<T> objects = new Queue<T>();

        // Recycle object, so it's ready for reusing
        public void Recycle(T obj)
        {
            if (funcRecycle != null) funcRecycle(obj);
            objects.Enqueue(obj);
        }

        // Get a new object from the pooler
        public T Buy()
        {
            if (objects.Count == 0)
            {
                if (funcGenerate != null) return funcGenerate();
                else return default(T);
            }

            T obj = objects.Dequeue();
            if (funcReinit != null) funcReinit(obj);
            return obj;
        }

        // Fill the pooler with objects
        public void Stock(int amount)
        {
            for (int i = 0; i < amount; ++i)
            {
                T obj;
                if (funcGenerate != null) obj = funcGenerate();
                else obj = default(T);
                objects.Enqueue(obj);
            }
        }

        // Reset the pooler
        public void Clear(bool destroy = true)
        {
            if (destroy && funcDestroy != null)
                foreach (var o in objects)
                    funcDestroy(o);
            objects.Clear();
        }
    }
}