using module Belin.Cli

# Backup all databases, excluding the system ones (i.e. "information_schema", "mysql", "performance_schema" and "sys").
Backup-MySqlTable -Uri "mysql://user:pass@localhost/db" -Path /path/to/backups

# Backup all tables in a specific database.
Backup-MySqlTable -Uri "mysql://user:pass@localhost/db" -Path /path/to/backups -Schema MyDB

# Backup specific tables.
Backup-MySqlTable -Uri "mysql://user:pass@localhost/db" -Path /path/to/backups -Schema MyDB -Table Users, Orders

# Restore a database from the specified SQL file.
Restore-MySqlTable -Uri "mysql://user:pass@localhost/db" -Path /path/to/backups/MyDB.sql

# Optimize all tables in all non-system databases.
Optimize-MySqlTable -Uri "mysql://user:pass@localhost/db"

# Optimize specific tables.
Optimize-MySqlTable -Uri "mysql://user:pass@localhost/db" -Schema MyDB -Table Users, Orders
