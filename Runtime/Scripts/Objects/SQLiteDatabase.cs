using System;
using System.IO;
using SQLite;
using UnityEngine;

namespace ParkMinPackages.SQLiteToolkit
{
	public abstract class SQLiteDatabase : IDisposable
	{
		// - Statics -
		static readonly object ProviderLock = new object();
		static bool _isProviderInitialized;

		// - Construct -
		protected SQLiteDatabase(
			string relativeDatabasePath,
			SQLiteEditorBasePath editorBasePath,
			SQLiteStandaloneBasePath standaloneBasePath,
			SQLiteAndroidBasePath androidBasePath,
			SQLiteWebGLBasePath webGLBasePath
		) {
#if UNITY_EDITOR
			string basePath = GetBasePath(editorBasePath);
#elif UNITY_STANDALONE
			string basePath = GetBasePath(standaloneBasePath);
#elif UNITY_ANDROID
			string basePath = GetBasePath(androidBasePath);
#elif UNITY_WEBGL
			string basePath = GetBasePath(webGLBasePath);
#else
			throw new PlatformNotSupportedException("Current platform is not supported.");
#endif
			DatabasePath = GetDatabasePath(basePath, relativeDatabasePath);
			string databaseDirectoryPath = Path.GetDirectoryName(DatabasePath);
			if (string.IsNullOrWhiteSpace(databaseDirectoryPath)) {
				throw new InvalidOperationException("Database directory path could not be resolved.");
			}
			Directory.CreateDirectory(databaseDirectoryPath);

			InitializeProvider();
			_connection = new SQLiteConnection(DatabasePath);
		}

		// - Public Methods -
		public void Dispose() {
			if (_isDisposed) {
				return;
			}

			_isDisposed = true;
			try {
				DisposeTables();
			}
			finally {
				_connection.Close();
			}
		}

		// - Public Properties-
		public string DatabasePath { get; }

		// - Internals -
		readonly SQLiteConnection _connection;
		bool _isDisposed;

		protected SQLiteConnection GetConnection() {
			EnsureNotDisposed();
			return _connection;
		}
		protected abstract void DisposeTables();
		static string GetBasePath(SQLiteEditorBasePath basePath) {
			return basePath switch
			{
				SQLiteEditorBasePath.PersistentDataPath => Application.persistentDataPath,
				SQLiteEditorBasePath.Assets => Application.dataPath,
				_ => throw new ArgumentOutOfRangeException(nameof(basePath), basePath, null)
			};
		}
		static string GetBasePath(SQLiteStandaloneBasePath basePath) {
			switch (basePath) {
				case SQLiteStandaloneBasePath.PersistentDataPath:
					return Application.persistentDataPath;
				case SQLiteStandaloneBasePath.ExecutableDirectory:
					string executableDirectoryPath = Path.GetDirectoryName(Application.dataPath);
					if (string.IsNullOrWhiteSpace(executableDirectoryPath)) {
						throw new InvalidOperationException("Executable directory path could not be resolved.");
					}
					return executableDirectoryPath;
				case SQLiteStandaloneBasePath.DataDirectory:
					return Application.dataPath;
				default:
					throw new ArgumentOutOfRangeException(nameof(basePath), basePath, null);
			}
		}
		static string GetBasePath(SQLiteAndroidBasePath basePath) {
			return basePath switch
			{
				SQLiteAndroidBasePath.PersistentDataPath => Application.persistentDataPath,
				_ => throw new ArgumentOutOfRangeException(nameof(basePath), basePath, null)
			};
		}
		static string GetBasePath(SQLiteWebGLBasePath basePath) {
			return basePath switch
			{
				SQLiteWebGLBasePath.PersistentDataPath => Application.persistentDataPath,
				_ => throw new ArgumentOutOfRangeException(nameof(basePath), basePath, null)
			};
		}
		static string GetDatabasePath(string basePath, string relativeDatabasePath) {
			if (string.IsNullOrWhiteSpace(basePath)) {
				throw new ArgumentException("Base path cannot be empty.", nameof(basePath));
			}
			if (string.IsNullOrWhiteSpace(relativeDatabasePath)) {
				throw new ArgumentException("Relative database path cannot be empty.", nameof(relativeDatabasePath));
			}

			string normalizedRelativePath = relativeDatabasePath.Trim();
			if (normalizedRelativePath.StartsWith("//", StringComparison.Ordinal) ||
			    normalizedRelativePath.StartsWith("\\\\", StringComparison.Ordinal) ||
			    normalizedRelativePath.Length >= 2 &&
			    char.IsLetter(normalizedRelativePath[0]) &&
			    normalizedRelativePath[1] == ':') {
				throw new ArgumentException("Absolute database paths are not allowed.", nameof(relativeDatabasePath));
			}

			normalizedRelativePath = normalizedRelativePath.TrimStart('/', '\\');
			normalizedRelativePath = normalizedRelativePath
			                        .Replace('/', Path.DirectorySeparatorChar)
			                        .Replace('\\', Path.DirectorySeparatorChar);
			if (!normalizedRelativePath.EndsWith(".db", StringComparison.OrdinalIgnoreCase)) {
				normalizedRelativePath += ".db";
			}

			string databaseFileName = Path.GetFileNameWithoutExtension(normalizedRelativePath);
			if (string.IsNullOrWhiteSpace(databaseFileName)) {
				throw new ArgumentException("Database file name cannot be empty.", nameof(relativeDatabasePath));
			}

			string normalizedBasePath = Path.GetFullPath(basePath);
			string databasePath = Path.GetFullPath(Path.Combine(normalizedBasePath, normalizedRelativePath));
			string basePathPrefix = normalizedBasePath.TrimEnd(
				Path.DirectorySeparatorChar,
				Path.AltDirectorySeparatorChar
			) + Path.DirectorySeparatorChar;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			StringComparison pathComparison = StringComparison.OrdinalIgnoreCase;
#else
			StringComparison pathComparison = StringComparison.Ordinal;
#endif
			if (!databasePath.StartsWith(basePathPrefix, pathComparison)) {
				throw new ArgumentException("Database path must remain inside the selected base path.", nameof(relativeDatabasePath));
			}
			return databasePath;
		}
		void EnsureNotDisposed() {
			if (_isDisposed) {
				throw new ObjectDisposedException(nameof(SQLiteDatabase));
			}
		}
		void InitializeProvider() {
			lock (ProviderLock) {
				if (_isProviderInitialized) {
					return;
				}

				SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
				_isProviderInitialized = true;
			}
		}
	}
}
