using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Finalspace.Onigiri.MVVM;

/// <summary>
/// A thread-safe, immutable, observable collection that implements
/// INotifyCollectionChanged and INotifyPropertyChanged.
/// Safe to use as ItemsSource in WPF or MAUI.
/// Backed by ImmutableArray for O(1) indexing and contiguous memory layout.
/// </summary>
public class ImmutableObservableCollection<T> :
    INotifyCollectionChanged,
    INotifyPropertyChanged,
    IList<T>,
    IList,
    IReadOnlyList<T>,
    ICollection<T>
{
    private ImmutableArray<T> _items = ImmutableArray<T>.Empty;
    private readonly object _writeLock = new();
    private volatile bool _isBatchUpdate = false;

    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    public int Count => _items.Length;

    public bool IsReadOnly => false;

    public T this[int index] { get => _items[index]; set => SetItem(index, value); }

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_items).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private readonly IDispatcher _dispatcherStorage;
    private IDispatcher Dispatcher => _dispatcherStorage ?? DispatcherFactory.Instance.GetDefault();

    public ImmutableObservableCollection()
    {
        _dispatcherStorage = null;
    }

    public ImmutableObservableCollection(IDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        _dispatcherStorage = dispatcher;
    }

    public int IndexOf(T item) => _items.IndexOf(item);

    public bool Contains(T item) => _items.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);


    public void Add(T item)
    {
        int newIndex;
        lock (_writeLock)
        {
            newIndex = _items.Length;
            _items = _items.Add(item);
        }

        RaiseCollectionChanged(
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, newIndex));
    }

    public void AddRange(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        IList<T> itemsList = items as IList<T> ?? items.ToList();
        if (itemsList.Count == 0)
            return;

        lock (_writeLock)
        {
            _items = _items.AddRange(itemsList);
        }

        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Insert(int index, T item)
    {
        lock (_writeLock)
        {
            _items = _items.Insert(index, item);
        }

        RaiseCollectionChanged(
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    public bool Remove(T item)
    {
        int oldIndex;
        lock (_writeLock)
        {
            oldIndex = _items.IndexOf(item);
            if (oldIndex < 0) return false;
            _items = _items.Remove(item);
        }

        RaiseCollectionChanged(
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, oldIndex));
        return true;
    }

    public void RemoveAt(int index)
    {
        T removed;
        lock (_writeLock)
        {
            if (index < 0 || index >= _items.Length) throw new ArgumentOutOfRangeException(nameof(index));
            removed = _items[index];
            _items = _items.RemoveAt(index);
        }

        RaiseCollectionChanged(
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed, index));
    }

    public void Clear()
    {
        lock (_writeLock)
        {
            if (_items.IsEmpty) return;
            _items = ImmutableArray<T>.Empty;
        }

        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void SetItem(int index, T newItem)
    {
        T oldItem;
        lock (_writeLock)
        {
            if (index < 0 || index >= _items.Length) throw new ArgumentOutOfRangeException(nameof(index));
            oldItem = _items[index];
            _items = _items.SetItem(index, newItem);
        }

        RaiseCollectionChanged(
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
    }

    /// <summary>
    /// Replace entire contents (one atomic swap). Raises a single Reset event.
    /// </summary>
    public void ReplaceAll(IEnumerable<T> newItems)
    {
        ArgumentNullException.ThrowIfNull(newItems);
        lock (_writeLock)
        {
            _items = ImmutableArray.CreateRange(newItems);
        }
        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Suppresses all change notifications until <see cref="EndUpdate"/> is called.
    /// Use during deserialization when no UI is bound.
    /// </summary>
    public void BeginUpdate() => _isBatchUpdate = true;

    /// <summary>
    /// Ends bulk loading and raises a single Reset notification.
    /// </summary>
    public void EndUpdate()
    {
        _isBatchUpdate = false;
        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    #region Notification helpers

    private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
    {
        if (_isBatchUpdate)
            return;

        void Notify()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            CollectionChanged?.Invoke(this, args);
        }
        if (Dispatcher is not null)
            Dispatcher.Invoke(() => Notify());
        else
            Notify();
    }

    #endregion

    #region Explicit IList (non-generic) — anonymous

    bool IList.IsFixedSize => false;
    bool IList.IsReadOnly => false;
    int IList.Add(object value)
    {
        if (value is T t)
        {
            Add(t);
            return _items.IndexOf(t);
        }
        throw new ArgumentException($"Invalid type {value?.GetType()}", nameof(value));
    }

    bool IList.Contains(object value) => value is T t && Contains(t);

    int IList.IndexOf(object value) => value is T t ? IndexOf(t) : -1;

    void IList.Insert(int index, object value)
    {
        if (value is T t)
            Insert(index, t);
        else
            throw new ArgumentException($"Invalid type {value?.GetType()}", nameof(value));
    }

    void IList.Remove(object value)
    {
        if (value is T t)
            Remove(t);
    }

    void IList.RemoveAt(int index) => RemoveAt(index);

    object IList.this[int index]
    {
        get => this[index];
        set
        {
            if (value is T t)
                SetItem(index, t);
            else
                throw new ArgumentException($"Invalid type {value?.GetType()}", nameof(value));
        }
    }

    void ICollection.CopyTo(Array array, int index)
    {
        if (array is T[] tArray)
            CopyTo(tArray, index);
        else
            throw new ArgumentException("Invalid array type", nameof(array));
    }

    int ICollection.Count => Count;
    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => this;

    #endregion
}
