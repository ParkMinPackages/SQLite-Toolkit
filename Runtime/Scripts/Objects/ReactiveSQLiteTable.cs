using System;
using System.Collections.Generic;
using R3;
using SQLite;

namespace ParkMinPackages.SQLiteToolkit
{
	public class ReactiveSQLiteTable<TKey, TRecord> : IDisposable
		where TRecord : class, ISQLiteRecord<TKey>, new()
	{
		// - Construct -
		public ReactiveSQLiteTable(SQLiteConnection connection) {
			_connection = connection ?? throw new ArgumentNullException(nameof(connection));

			_connection.CreateTable<TRecord>();
			_sqLiteReadQuery = new SQLiteReadQuery<TRecord>(_connection.Table<TRecord>(), EnsureNotDisposed);
		}

		// - Public Methods -
		public TRecord Create(TRecord record) {
			EnsureNotDisposed();
			if (record == null) {
				throw new ArgumentNullException(nameof(record));
			}
			int changedRows = _connection.Insert(record);
			if (changedRows != 1) {
				throw new InvalidOperationException($"Record insert failed. Count={changedRows}");
			}
			_created.OnNext(record);
			return record;
		}

		public SQLiteReadQuery<TRecord> Query() {
			EnsureNotDisposed();
			return _sqLiteReadQuery;
		}
		public TRecord Read(TKey id) {
			EnsureNotDisposed();
			return _connection.Find<TRecord>(id);
		}
		public List<TRecord> ReadAll() {
			EnsureNotDisposed();
			return _connection.Table<TRecord>().ToList();
		}
		public int Count() {
			EnsureNotDisposed();
			return _connection.Table<TRecord>().Count();
		}
		public bool Contains(TKey id) {
			EnsureNotDisposed();
			TRecord record = Read(id);
			return record != null;
		}

		public void Update(TRecord record) {
			EnsureNotDisposed();
			if (record == null) {
				throw new ArgumentNullException(nameof(record));
			}
			int changedRows = _connection.Update(record);
			if (changedRows != 1) {
				throw new InvalidOperationException($"Record was not found. Id={record.Id}");
			}
			_updated.OnNext(record);
		}

		public virtual void Delete(TKey id) {
			EnsureNotDisposed();
			TRecord record = _connection.Find<TRecord>(id);
			if (record == null) {
				throw new InvalidOperationException($"Record was not found. Id={id}");
			}
			int changedRows = _connection.Delete(record);
			if (changedRows != 1) {
				throw new InvalidOperationException($"Record was not found. Id={id}");
			}
			_deleted.OnNext(record);
		}

		public void Dispose() {
			if (_isDisposed) {
				return;
			}
			_isDisposed = true;
			_created.Dispose();
			_updated.Dispose();
			_deleted.Dispose();
		}

		// - Public Properties-
		public Observable<TRecord> Created
		{
			get
			{
				EnsureNotDisposed();
				return _created;
			}
		}
		public Observable<TRecord> Updated
		{
			get
			{
				EnsureNotDisposed();
				return _updated;
			}
		}
		public Observable<TRecord> Deleted
		{
			get
			{
				EnsureNotDisposed();
				return _deleted;
			}
		}

		// - Internals -
		protected readonly SQLiteConnection _connection;
		protected readonly SQLiteReadQuery<TRecord> _sqLiteReadQuery;
		readonly Subject<TRecord> _created = new Subject<TRecord>();
		readonly Subject<TRecord> _updated = new Subject<TRecord>();
		readonly Subject<TRecord> _deleted = new Subject<TRecord>();
		bool _isDisposed;

		void EnsureNotDisposed() {
			if (_isDisposed) {
				throw new ObjectDisposedException(nameof(ReactiveSQLiteTable<TKey, TRecord>));
			}
		}
	}
}
