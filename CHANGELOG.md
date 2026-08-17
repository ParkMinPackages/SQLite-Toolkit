# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2026-08-17

### Changed
- Changed synchronized observable list initialization to read records through `Query().ToList()`.

### Removed
- Removed the redundant `ReactiveSQLiteTable.ReadAll()` API. Use `Query().ToList()` instead.

## [1.0.1] - 2026-08-16

### Added
- Added PackageManager dependency metadata for `sqlite-net-pcl` and `ObservableCollections.R3`.

### Changed
- Restricted `package.json` dependencies to Unity Registry packages.

## [1.0.0] - 2026-08-16

### Added
- Added SQLite database, query, reactive table, synchronized observable list, record interface, and platform base-path implementations.
- Documented direct and transitive NuGet dependencies.

### Changed
- Changed the runtime namespace from `ParkMin.SQLiteToolkit` to `ParkMinPackages.SQLiteToolkit`.

## [0.1.0] - 2026-08-16

### Added
- Created the initial Unity package structure with Runtime and Editor assembly definitions.
