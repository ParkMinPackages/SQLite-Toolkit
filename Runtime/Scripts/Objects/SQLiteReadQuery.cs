using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SQLite;

namespace ParkMinPackages.SQLiteToolkit
{
	public sealed class SQLiteReadQuery<TRecord>
	{
		// - Public Construct -
		public SQLiteReadQuery(TableQuery<TRecord> query, Action ensureNotDisposed) {
			_query = query ?? throw new ArgumentNullException(nameof(query));
			_ensureNotDisposed = ensureNotDisposed ?? throw new ArgumentNullException(nameof(ensureNotDisposed));
		}

		// - Public Methods -
		public SQLiteReadQuery<TResult> Clone<TResult>() {
			_ensureNotDisposed();
			return new SQLiteReadQuery<TResult>(_query.Clone<TResult>(), _ensureNotDisposed);
		}
		public SQLiteReadQuery<TRecord> Where(Expression<Func<TRecord, bool>> predicate) {
			_ensureNotDisposed();
			return new SQLiteReadQuery<TRecord>(_query.Where(predicate), _ensureNotDisposed);
		}
		public SQLiteReadQuery<TRecord> OrderBy<TOrder>(Expression<Func<TRecord, TOrder>> keySelector) {
			_ensureNotDisposed();
			return new SQLiteReadQuery<TRecord>(_query.OrderBy(keySelector), _ensureNotDisposed);
		}
		public SQLiteReadQuery<TRecord> OrderByDescending<TOrder>(Expression<Func<TRecord, TOrder>> keySelector) {
			_ensureNotDisposed();
			return new SQLiteReadQuery<TRecord>(_query.OrderByDescending(keySelector), _ensureNotDisposed);
		}
		public SQLiteReadQuery<TRecord> ThenBy<TOrder>(Expression<Func<TRecord, TOrder>> keySelector) {
			_ensureNotDisposed();
			return new SQLiteReadQuery<TRecord>(_query.ThenBy(keySelector), _ensureNotDisposed);
		}
		public SQLiteReadQuery<TRecord> ThenByDescending<TOrder>(Expression<Func<TRecord, TOrder>> keySelector) {
			_ensureNotDisposed();
			return new SQLiteReadQuery<TRecord>(_query.ThenByDescending(keySelector), _ensureNotDisposed);
		}
		public SQLiteReadQuery<TRecord> Skip(int count) {
			_ensureNotDisposed();
			return new SQLiteReadQuery<TRecord>(_query.Skip(count), _ensureNotDisposed);
		}
		public SQLiteReadQuery<TRecord> Take(int count) {
			_ensureNotDisposed();
			return new SQLiteReadQuery<TRecord>(_query.Take(count), _ensureNotDisposed);
		}
		public int Count() {
			_ensureNotDisposed();
			return _query.Count();
		}
		public int Count(Expression<Func<TRecord, bool>> predicate) {
			_ensureNotDisposed();
			return _query.Count(predicate);
		}
		public bool Any() {
			_ensureNotDisposed();
			return _query.Count() > 0;
		}
		public bool Any(Expression<Func<TRecord, bool>> predicate) {
			_ensureNotDisposed();
			return _query.Count(predicate) > 0;
		}
		public TRecord ElementAt(int index) {
			_ensureNotDisposed();
			return _query.ElementAt(index);
		}
		public TRecord First() {
			_ensureNotDisposed();
			return _query.First();
		}
		public TRecord First(Expression<Func<TRecord, bool>> predicate) {
			_ensureNotDisposed();
			return _query.First(predicate);
		}
		public TRecord FirstOrDefault() {
			_ensureNotDisposed();
			return _query.FirstOrDefault();
		}
		public TRecord FirstOrDefault(Expression<Func<TRecord, bool>> predicate) {
			_ensureNotDisposed();
			return _query.FirstOrDefault(predicate);
		}
		public TRecord[] ToArray() {
			_ensureNotDisposed();
			return _query.ToArray();
		}
		public List<TRecord> ToList() {
			_ensureNotDisposed();
			return _query.ToList();
		}

		// - Internals -
		readonly TableQuery<TRecord> _query;
		readonly Action _ensureNotDisposed;
	}
}
