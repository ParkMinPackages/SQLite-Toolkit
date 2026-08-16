using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;

namespace ParkMinPackages.SQLiteToolkit
{
	public sealed class ReactiveSQLiteTableSynchronizedObservableList<TKey, TRecord, TItem> : IReadOnlyObservableList<TItem>, IDisposable
		where TRecord : class, ISQLiteRecord<TKey>, new()
		where TItem : class
	{
		// - Public Construct -
		public ReactiveSQLiteTableSynchronizedObservableList(
			ReactiveSQLiteTable<TKey, TRecord> table,
			Func<TItem, TKey> itemKeySelector,
			Func<TRecord, TItem> itemCreateAction,
			Action<TItem, TRecord> itemUpdateAction,
			Comparison<TItem> comparer = null
		) {
			_table = table ?? throw new ArgumentNullException(nameof(table));
			_itemKeySelector = itemKeySelector ?? throw new ArgumentNullException(nameof(itemKeySelector));
			_itemCreateAction = itemCreateAction ?? throw new ArgumentNullException(nameof(itemCreateAction));
			_itemUpdateAction = itemUpdateAction ?? throw new ArgumentNullException(nameof(itemUpdateAction));
			_comparer = comparer == null ? null : Comparer<TItem>.Create(comparer);
		}

		// - Public Methods -
		public void Initialize() {
			if (_isInitialized) {
				throw new InvalidOperationException($"{nameof(ReactiveSQLiteTableSynchronizedObservableList<TKey, TRecord, TItem>)} is already initialized.");
			}
			List<TRecord> records = _table.ReadAll();
			List<TItem> items = records.Select(_itemCreateAction).ToList();
			if (_comparer != null) {
				items.Sort(_comparer);
			}
			_items.Clear();
			foreach (TItem item in items) {
				_items.Add(item);
			}

			_table.Created.Subscribe(record =>
			{
				_items.Add(_itemCreateAction(record));
				if (_comparer != null) {
					_items.Sort(_comparer);
				}
			}).AddTo(_subscriptions);
			_table.Updated.Subscribe(record =>
			{
				TItem item = _items.Single(item => EqualityComparer<TKey>.Default.Equals(_itemKeySelector(item), record.Id));
				_itemUpdateAction(item, record);
				if (_comparer != null) {
					_items.Sort(_comparer);
				}
			}).AddTo(_subscriptions);
			_table.Deleted.Subscribe(record =>
			{
				TItem item = _items.Single(item => EqualityComparer<TKey>.Default.Equals(_itemKeySelector(item), record.Id));
				_items.Remove(item);
			}).AddTo(_subscriptions);
			_isInitialized = true;
		}
		public void Dispose() {
			_subscriptions.Dispose();
		}
		public IEnumerator<TItem> GetEnumerator() {
			return _items.GetEnumerator();
		}
		public ISynchronizedView<TItem, TView> CreateView<TView>(Func<TItem, TView> transform) {
			return _items.CreateView(transform);
		}

		// - Public Properties-
		public int Count
		{
			get { return _items.Count; }
		}
		public TItem this[int index]
		{
			get { return _items[index]; }
		}
		public object SyncRoot
		{
			get { return _items.SyncRoot; }
		}

		// - Public Events-
		public event NotifyCollectionChangedEventHandler<TItem> CollectionChanged
		{
			add { _items.CollectionChanged += value; }
			remove { _items.CollectionChanged -= value; }
		}

		// - Internals -
		readonly ReactiveSQLiteTable<TKey, TRecord> _table;
		readonly Func<TItem, TKey> _itemKeySelector;
		readonly Func<TRecord, TItem> _itemCreateAction;
		readonly Action<TItem, TRecord> _itemUpdateAction;
		readonly IComparer<TItem> _comparer;
		readonly ObservableList<TItem> _items = new ObservableList<TItem>();
		readonly CompositeDisposable _subscriptions = new CompositeDisposable();
		bool _isInitialized;

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
