using System;
using System.Collections.Generic;
using R3;
using SQLite;

namespace ParkMinPackages.SQLiteToolkit
{
	public sealed class ReactiveSingleRecordSQLiteTable<TKey, TRecord> : IDisposable
		where TRecord : class, ISQLiteRecord<TKey>, new()
	{
		// - Public Construct -
		public ReactiveSingleRecordSQLiteTable(
			SQLiteConnection connection,
			Func<TRecord> createDefaultRecord
		) {
			_connection = connection ?? throw new ArgumentNullException(nameof(connection));
			if (createDefaultRecord == null) {
				throw new ArgumentNullException(nameof(createDefaultRecord));
			}
			_connection.CreateTable<TRecord>();
			List<TRecord> records = _connection.Table<TRecord>().ToList();
			if (records.Count == 0) {
				TRecord defaultRecord = createDefaultRecord();
				if (defaultRecord == null) {
					throw new InvalidOperationException("Default record cannot be null.");
				}
				int changedRows = _connection.Insert(defaultRecord);
				if (changedRows != 1) {
					throw new InvalidOperationException($"Default record insert failed. Count={changedRows}");
				}
				return;
			}
			if (records.Count != 1) {
				throw new InvalidOperationException($"Record count must be exactly one. Count={records.Count}");
			}
		}

		// - Public Methods -
		public TRecord Read() {
			EnsureNotDisposed();
			return _connection.Table<TRecord>().First();
		}
		public void Update(Action<TRecord> updateAction) {
			EnsureNotDisposed();
			if (updateAction == null) {
				throw new ArgumentNullException(nameof(updateAction));
			}

			TRecord record = Read();
			updateAction(record);

			int changedRows = _connection.Update(record);
			if (changedRows != 1) {
				throw new InvalidOperationException($"Record was not found. Id={record.Id}");
			}

			_updated.OnNext(record);
		}
		public void Dispose() {
			if (_isDisposed) {
				return;
			}
			_isDisposed = true;
			_updated.Dispose();
		}

		// - Public Properties-
		public Observable<TRecord> Updated
		{
			get
			{
				EnsureNotDisposed();
				return _updated;
			}
		}

		// - Internals -
		readonly SQLiteConnection _connection;
		readonly Subject<TRecord> _updated = new Subject<TRecord>();
		bool _isDisposed;

		void EnsureNotDisposed() {
			if (_isDisposed) {
				throw new ObjectDisposedException(nameof(ReactiveSingleRecordSQLiteTable<TKey, TRecord>));
			}
		}
	}
}
