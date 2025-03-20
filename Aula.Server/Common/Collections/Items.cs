using System.Collections;
using System.Runtime.CompilerServices;

namespace Aula.Server.Common.Collections;

internal struct Items<T> : IReadOnlyList<T>, IEquatable<Items<T>> where T : class
{
	private Object? _items;

	internal Items(T item)
	{
		_items = item;
	}

	internal Items(List<T> items)
	{
		_items = items;
	}

	internal readonly Int32 Count
	{
		get
		{
			if (_items is T)
			{
				return 1;
			}

			if (_items is not null)
			{
				return Unsafe.As<List<T>>(_items).Count;
			}

			return 0;
		}
	}

	internal readonly T? UnderlyingItem => _items as T;

	internal readonly List<T>? UnderlyingList => _items as List<T>;

	internal readonly T this[Int32 index]
	{
		get
		{
			if (_items is T value)
			{
				if (index == 0)
				{
					return value;
				}
			}
			else if (_items is not null)
			{
				return Unsafe.As<List<T>>(_items)[index];
			}

			return Array.Empty<T>()[0];
		}
		set => throw new NotSupportedException();
	}

	internal void Add(T value)
	{
		if (_items is List<T> list)
		{
			list.Add(value);
		}
		else if (_items is not null)
		{
			_items = new List<T>
			{
				Unsafe.As<T>(_items),
				value,
			};
		}
		else
		{
			_items = value;
		}
	}

	public readonly Enumerator GetEnumerator()
	{
		return new Enumerator(_items);
	}

	public readonly Boolean Equals(Items<T> other)
	{
		if (_items is null &&
		    other._items is null)
		{
			return true;
		}

		if (_items is T item1)
		{
			if (other._items is T item2)
			{
				return item1 == item2;
			}

			if (other._items is not null)
			{
				var items2 = Unsafe.As<List<T>>(other._items);
				return items2.Count == 1 && item1 == items2[0];
			}

			return false;
		}

		if (_items is List<T> items1)
		{
			if (other._items is List<T> items2)
			{
				return items1.SequenceEqual(items2);
			}

			if (other._items is not null)
			{
				return items1.Count == 1 && items1[0] == Unsafe.As<T>(other._items);
			}

			return false;
		}

		return false;
	}

	public override readonly Boolean Equals(Object? obj)
	{
		return obj is Items<T> other && Equals(other);
	}

	public override readonly Int32 GetHashCode()
	{
		return _items != null ? _items.GetHashCode() : 0;
	}

	public static Boolean operator ==(Items<T> left, Items<T> right) => left.Equals(right);

	public static Boolean operator !=(Items<T> left, Items<T> right) => !left.Equals(right);

	public static implicit operator Items<T>(T value) => new(value);

	public static implicit operator Items<T>(List<T> values) => new(values);

	readonly T IReadOnlyList<T>.this[Int32 index] => this[index];

	readonly Int32 IReadOnlyCollection<T>.Count => Count;

	readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	readonly IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal struct Enumerator : IEnumerator<T>
	{
		private readonly List<T>? _items;
		private T? _current;
		private Int32 _index;

		internal Enumerator(Object? items)
		{
			if (items is T item)
			{
				_current = item;
				_items = null;
			}
			else if (items is not null)
			{
				_current = null;
				_items = Unsafe.As<List<T>>(items);
			}
		}

		public Boolean MoveNext()
		{
			if (_index < 0)
			{
				return false;
			}

			if (_items is not null)
			{
				if (_index < _items.Count)
				{
					_current = _items[_index];
					_index++;
					return true;
				}

				_current = null;
				_index = -1;
				return false;
			}

			_index = -1;
			return _current is not null;
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}

		public readonly T Current => _current ?? throw new InvalidOperationException($"{nameof(Current)} is null");

		readonly Object? IEnumerator.Current => Current;

		public readonly void Dispose()
		{
		}
	}
}
