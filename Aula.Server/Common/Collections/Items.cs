using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Primitives;

namespace Aula.Server.Common.Collections;

internal struct Items<T> : IReadOnlyList<T>, IEquatable<Items<T>> where T : class?
{
	private Object? _items;

	internal Items(T item)
	{
		_items = item is not null ? item : NullItem.Instance;
	}

	internal Items(List<T> items)
	{
		_items = items;
	}

	internal readonly Int32 Count
	{
		get
		{
			if (_items is List<T> list)
			{
				return list.Count;
			}

			if (_items is not null)
			{
				return 1;
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
			else if (_items is NullItem)
			{
				return null!;
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
				(_items as T)!,
				value,
			};
		}
		else
		{
			_items = value is not null ? value : NullItem.Instance;
		}
	}

	public readonly Enumerator GetEnumerator()
	{
		return new Enumerator(_items);
	}

	public readonly Boolean Equals(Items<T> other)
	{
		var thisIsNull = _items is null;
		var otherIsNull = other._items is null;
		if (thisIsNull || otherIsNull)
		{
			return thisIsNull == otherIsNull;
		}

		var thisIsNullItem = _items is NullItem;
		var otherIsNullItem = other._items is NullItem;
		if (thisIsNullItem || otherIsNullItem)
		{
			return thisIsNullItem == otherIsNullItem;
		}

		if (_items is T thisItem)
		{
			if (other._items is T otherItem)
			{
				return thisItem == otherItem;
			}

			var otherItems = Unsafe.As<List<T>>(other._items!);
			return otherItems.Count == 1 && thisItem == otherItems[0];
		}
		else
		{
			var thisItems = Unsafe.As<List<T>>(_items!);
			if (other._items is List<T> otherItems)
			{
				return thisItems.SequenceEqual(otherItems);
			}

			var otherItem = Unsafe.As<T>(other._items);
			return thisItems.Count == 1 && thisItems[0] == otherItem;
		}
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

	public static implicit operator Items<T?>(T value) => new(value);

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
		private Object? _current;
		private Int32 _index;

		internal Enumerator(Object? item)
		{
			if (item is List<T> items)
			{
				_current = null;
				_items = items;
			}
			else if (item is not null)
			{
				_current = item;
				_items = null;
			}
			else
			{
				_index = -1;
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
			return true;
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}

		public readonly T Current => (_current as T)!;

		readonly Object? IEnumerator.Current => Current;

		public readonly void Dispose()
		{
		}
	}

	internal sealed class NullItem
	{
		internal static NullItem Instance { get; } = new();

		private NullItem()
		{
		}
	}
}
