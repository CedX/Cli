# TODO Database Management

## Backup and restore
Create backups of [MySQL](https://www.mysql.com) or [MariaDB](https://mariadb.com) databases:

```powershell
# Backup all databases, excluding the system ones (i.e. "information_schema", "mysql", "performance_schema" and "sys").
Backup-MySqlTable -Uri "mysql://user:pass@localhost/db" -Path /path/to/backups

# Backup all tables in a specific database.
Backup-MySqlTable -Uri "mysql://user:pass@localhost/db" -Path /path/to/backups -Schema MyDB

# Backup specific tables.
Backup-MySqlTable -Uri "mysql://user:pass@localhost/db" -Path /path/to/backups -Schema MyDB -Table Users, Orders
```

Restore MySQL or MariaDB databases from backup files:

```powershell
# Restore a database from the specified SQL file.
Restore-MySqlTable -Uri "mysql://user:pass@localhost/db" -Path /path/to/backups/MyDB.sql
```

## Miscellaneous operations
Defragment [MySQL](https://www.mysql.com) or [MariaDB](https://mariadb.com) tables for better performance:

```powershell
# Optimize all tables in all non-system databases.
Optimize-MySqlTable -Uri "mysql://user:pass@localhost/db"

# Optimize specific tables.
Optimize-MySqlTable -Uri "mysql://user:pass@localhost/db" -Schema MyDB -Table Users, Orders
```

Modify the character set of the tables:

```powershell
# Optimize all tables in all non-system databases.
Set-MySqlCharset -Uri "mysql://user:pass@localhost/db" -Schema MyDB -Table Users -Collation utf8mb4_unicode_ci
```

Modify the storage engine of the tables:

```powershell
# Optimize all tables in all non-system databases.
Set-MySqlEngine -Uri "mysql://user:pass@localhost/db" -Schema MyDB -Table Users -Engine InnoDB
```
