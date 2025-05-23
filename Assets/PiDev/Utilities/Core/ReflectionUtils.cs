using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
 * Reflection helpers for accessing and modifying private or public fields at runtime.
 * Includes utilities to get/set field values and copy all fields from one instance to another.
 * Useful for internal debugging, data transfer, or tooling without exposing members publicly.
 *
 * ============= Usage =============
 * ReflectionUtils.SetFieldValue(target, "fieldName", newValue);
 * var value = ReflectionUtils.GetFieldValue(target, "fieldName");
 * ReflectionUtils.CopyPublicMembers(sourceComponent, targetComponent);
 */

public static class ReflectionUtils
{
    public static void SetFieldValue(object target, string fieldName, object value, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        var type = target.GetType();
        FieldInfo field = GetFieldInfo(type, fieldName, flags);
        if (field == null)
            throw new MissingFieldException(type.FullName, fieldName);
        field.SetValue(target, value);
    }

    public static object GetFieldValue(object target, string fieldName, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        var type = target.GetType();
        FieldInfo field = GetFieldInfo(type, fieldName, flags);
        if (field == null)
            throw new MissingFieldException(type.FullName, fieldName);
        return field.GetValue(target);
    }

    private static FieldInfo GetFieldInfo(Type type, string fieldName, BindingFlags flags)
    {
        while (type != null)
        {
            FieldInfo field = type.GetField(fieldName, flags);
            if (field != null)
                return field;
            type = type.BaseType;
        }
        return null;
    }

    public static void CopyPublicMembers<T>(T sourceComp, T targetComp)
    {
        FieldInfo[] sourceFields = sourceComp.GetType().GetFields(BindingFlags.Public |
                                                         BindingFlags.NonPublic |
                                                         BindingFlags.Instance);
        int i = 0;
        for (i = 0; i < sourceFields.Length; i++)
        {
            var value = sourceFields[i].GetValue(sourceComp);
            sourceFields[i].SetValue(targetComp, value);
        }
    }


    public class MultipleFields<T>
    {
        public bool hasDifferentValues = false;
        public bool hasNoValue = true;
        public T value = default(T);
        public List<T> values = new List<T>();
    }


    public static MultipleFields<PropertyType> GetFields<PropertyType, ObjectType>(List<ObjectType> objects, string field)
    {
        MultipleFields<PropertyType> res = new MultipleFields<PropertyType>();
        foreach (var o in objects)
        {
            try
            {
                var type = o.GetType();
                var fld = type.GetField(field);
                var prop = type.GetProperty(field);
                if (fld != null)
                    res.values.Add((PropertyType)fld.GetValue(o));
                else if (prop != null)
                    res.values.Add((PropertyType)prop.GetValue(o, null));
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        var distinct = res.values.Distinct().ToList();
        if (distinct.Count > 1) { res.hasDifferentValues = true; res.hasNoValue = false; }
        else if (distinct.Count == 1) { res.value = res.values[0]; res.hasNoValue = false; }
        return res;
    }

    public static void SetFields<PropertyType, ObjectType>(List<ObjectType> objects, string field, PropertyType value)
    {
        foreach (var o in objects)
        {
            try
            {
                var type = o.GetType();
                var fld = type.GetField(field);
                var prop = type.GetProperty(field);
                if (fld != null)
                    fld.SetValue(o, value);
                if (prop != null)
                    prop.SetValue(o, value, null);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to set value: " + e.Message + "\n" + e.StackTrace.ToString());
            }
        }
    }
}
