using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 * Licensed under MIT
 *
 * ============= Description =============
 * The underlying data structure for the InspectorTable.
 * 
 * ============= Usage =============
 * public TableList<YourType> yourTable;
 */
namespace PiDev.TableView
{
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