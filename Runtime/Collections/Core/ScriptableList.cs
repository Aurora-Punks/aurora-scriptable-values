using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AuroraPunks.ScriptableValues.Helpers;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AuroraPunks.ScriptableValues
{
	public abstract class ScriptableList<T> : RuntimeScriptableObject, IList<T>, IReadOnlyList<T>, IList
	{
#if UNITY_EDITOR
		internal const int ORDER = ScriptableEvent.ORDER + 50;
#endif
		
		[SerializeField] 
		[Tooltip("If read only, the list cannot be changed at runtime and won't be cleared on start.")]
		private bool isReadOnly = false;
		[SerializeField] 
		[Tooltip("If true, an equality check will be run before setting an item through the indexer to make sure the new object is not the same as the old one.")]
		private bool setEqualityCheck = true;
		[SerializeField] 
		[Tooltip("If true, the list will be cleared on play mode start/game boot.")]
		private bool clearOnStart = true;
		[SerializeField]
		internal List<T> list = new List<T>();

		public T this[int index] { get { return list[index]; } set { SetValue(index, value); } }

		object IList.this[int index]
		{
			get { return list[index]; }
			set
			{
				if (IsValidType(value, out T newValue))
				{
					SetValue(index, newValue);
				}
			}
		}

		// Is this List synchronized (thread-safe)?
		bool ICollection.IsSynchronized { get { return false; } }
		// Synchronization root for this object.
		object ICollection.SyncRoot { get { return this; } }

		bool IList.IsFixedSize { get { return isReadOnly; } }
		public bool IsReadOnly { get { return isReadOnly; } set { isReadOnly = value; } }
		public int Count { get { return list.Count; } }
		
		public int Capacity { get { return list.Capacity; } }

		/// <summary>
		///		If true, an equality check will be run before setting an item through the indexer to make sure the new object is not the same as the old one.
		/// </summary>
		public bool SetEqualityCheck { get { return setEqualityCheck; } set { setEqualityCheck = value; } }
		/// <summary>
		///		If true, the list will be cleared on play mode start/game boot.
		/// </summary>
		public bool ClearOnStart { get { return clearOnStart; } set { clearOnStart = value; } }

		public event Action<T> OnAdded;
		public event Action<int, T> OnInserted;
		public event Action<int, T> OnAddedOrInserted;
		public event Action<int, T, T> OnSet;
		public event Action<int, T> OnRemoved;
		public event Action OnCleared;

		private void SetValue(int index, T value)
		{
			if (Application.isPlaying && isReadOnly)
			{
				Debug.LogError($"{this} is marked as read only and cannot be changed at runtime.");
				return;
			}
			
			if (setEqualityCheck && EqualityHelper.Equals(list[index], value))
			{
				return;
			}
			
			T oldValue = list[index];
			list[index] = value;
			OnSet?.Invoke(index, oldValue, value);
			
			AddStackTrace();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsValidType(object value, out T newValue)
		{
			if (value is T newValueT)
			{
				newValue = newValueT;
				return true;
			}

#if DEBUG
			Debug.LogError($"{typeof(T)} is not assignable from {value.GetType()}.");
#endif
			newValue = default;
			return false;
		}

		int IList.Add(object value)
		{
			if (IsValidType(value, out T newValue))
			{
				Add(newValue);
				return Count - 1;
			}

			return -1;
		}

		bool IList.Contains(object value)
		{
			return IsValidType(value, out T newValue) && Contains(newValue);
		}

		int IList.IndexOf(object value)
		{
			if (IsValidType(value, out T newValue))
			{
				return IndexOf(newValue);
			}

			return -1;
		}

		void IList.Insert(int index, object value)
		{
			if (IsValidType(value, out T newValue))
			{
				Insert(index, newValue);
			}
		}

		void IList.Remove(object value)
		{
			if (IsValidType(value, out T newValue))
			{
				Remove(newValue);
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection) list).CopyTo(array, index);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return ((IEnumerable<T>) list).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable) list).GetEnumerator();
		}

		public void Add(T item)
		{
			if (Application.isPlaying && isReadOnly)
			{
				Debug.LogError($"{this} is marked as read only and cannot be added to at runtime.");
				return;
			}
			
			int index = Count;
			list.Add(item);
			OnAdded?.Invoke(item);
			OnAddedOrInserted?.Invoke(index, item);
			
			AddStackTrace();
		}

		public void Insert(int index, T item)
		{
			if (Application.isPlaying && isReadOnly)
			{
				Debug.LogError($"{this} is marked as read only and cannot be inserted to at runtime.");
				return;
			}
			
			list.Insert(index, item);
			OnInserted?.Invoke(index, item);
			OnAddedOrInserted?.Invoke(index, item);
			
			AddStackTrace();
		}

		public bool Remove(T item)
		{
			if (Application.isPlaying && isReadOnly)
			{
				Debug.LogError($"{this} is marked as read only and cannot be removed from at runtime.");
				return false;
			}
			
			int index = list.IndexOf(item);
			if (index == -1)
			{
				return false;
			}

			list.RemoveAt(index);
			OnRemoved?.Invoke(index, item);
			AddStackTrace();

			return true;
		}

		public void RemoveAt(int index)
		{
			if (Application.isPlaying && isReadOnly)
			{
				Debug.LogError($"{this} is marked as read only and cannot be removed from at runtime.");
				return;
			}

			T item = list[index];
			list.RemoveAt(index);
			OnRemoved?.Invoke(index, item);
			
			AddStackTrace();
		}
		
		public int RemoveAll(Predicate<T> match)
		{
#if DEBUG
			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}
#endif
			
			if (Application.isPlaying && isReadOnly)
			{
				Debug.LogError($"{this} is marked as read only and cannot be removed from at runtime.");
				return 0;
			}

			int result = 0;

			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (match(list[i]))
				{
					result++;
					RemoveAt(i);
				}
			}

			return result;
		}

		public void Clear()
		{
			if (Application.isPlaying && isReadOnly)
			{
				Debug.LogError($"{this} is marked as read only and cannot be cleared at runtime.");
				return;
			}

			list.Clear();
			OnCleared?.Invoke();
			
			AddStackTrace();
		}

		public int IndexOf(T item)
		{
			return list.IndexOf(item);
		}

		public bool Contains(T item)
		{
			return list.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			list.CopyTo(array, arrayIndex);
		}

		public void Reverse()
		{
			if (Application.isPlaying && isReadOnly)
			{
				Debug.LogError($"{this} is marked as read only and cannot be reversed at runtime.");
				return;
			}

			list.Reverse();
			
			AddStackTrace();
		}

		public void Reverse(int index, int count)
		{
			if (Application.isPlaying && isReadOnly)
			{
				Debug.LogError($"{this} is marked as read only and cannot be reversed at runtime.");
				return;
			}
			
			list.Reverse(index, count);
			
			AddStackTrace();
		}

		public void Sort()
		{
			if (Application.isPlaying && isReadOnly)
			{
				Debug.LogError($"{this} is marked as read only and cannot be sorted at runtime.");
				return;
			}
			
			list.Sort();
			
			AddStackTrace();
		}

		public void Sort(IComparer<T> comparer)
		{
			if (Application.isPlaying && isReadOnly)
			{
				Debug.LogError($"{this} is marked as read only and cannot be sorted at runtime.");
				return;
			}

			list.Sort(0, Count, comparer);
			
			AddStackTrace();
		}
		
		public void Sort(int index, int count, IComparer<T> comparer)
		{
			if (Application.isPlaying && isReadOnly)
			{
				Debug.LogError($"{this} is marked as read only and cannot be sorted at runtime.");
				return;
			}
			
			list.Sort(index, count, comparer);
			
			AddStackTrace();
		}

		public void Sort(Comparison<T> comparison)
		{
			if (Application.isPlaying && isReadOnly)
			{
				Debug.LogError($"{this} is marked as read only and cannot be sorted at runtime.");
				return;
			}
			
			list.Sort(comparison);
			
			AddStackTrace();
		}

		public T[] ToArray()
		{
			return list.ToArray();
		}

		public void TrimExcess()
		{
			if (Application.isPlaying && isReadOnly)
			{
				Debug.LogError($"{this} is marked as read only and cannot be trimmed at runtime.");
				return;
			}
			
			list.TrimExcess();
			
			AddStackTrace();
		}

		public bool TrueForAll(Predicate<T> match)
		{
			return list.TrueForAll(match);
		}
		
		public bool TryFind(Predicate<T> match, out T result)
		{
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				if(match(list[i]))
				{
					result = list[i];
					return true;
				}
			}
			
			result = default;
			return false;
		}

		public override void ResetValues()
		{
			ResetStackTraces();

			OnAdded = null;
			OnInserted = null;
			OnAddedOrInserted = null;
			OnSet = null;
			OnRemoved = null;
			OnCleared = null;

			if (clearOnStart && !isReadOnly)
			{
				list.Clear();
			}
		}
		
#if UNITY_EDITOR
		protected override void OnExitPlayMode()
		{
			if (!isReadOnly && clearOnStart && list.Count > 0)
			{
				Debug.LogWarning($"There are left over objects in the scriptable list {name}. You should clear the list before leaving play mode.");
			}
			
			EventHelper.WarnIfLeftOverSubscribers(OnAdded, nameof(OnAdded), this);
			EventHelper.WarnIfLeftOverSubscribers(OnInserted, nameof(OnInserted), this);
			EventHelper.WarnIfLeftOverSubscribers(OnAddedOrInserted, nameof(OnAddedOrInserted), this);
			EventHelper.WarnIfLeftOverSubscribers(OnSet, nameof(OnSet), this);
			EventHelper.WarnIfLeftOverSubscribers(OnRemoved, nameof(OnRemoved), this);
			EventHelper.WarnIfLeftOverSubscribers(OnCleared, nameof(OnCleared), this);
			
			list.TrimExcess();
		}
#endif
	}
}