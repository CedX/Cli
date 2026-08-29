# Changelog

## Version [6.0.1](https://github.com/CedX/Cli/compare/v6.0.0...v6.0.1)
- Optimized the packaging.

## Version [6.0.0](https://github.com/CedX/Cli/compare/v5.4.0...v6.0.0)
- Reverted the solution to a plain [PowerShell](https://learn.microsoft.com/en-us/powershell) project.

## Version [5.4.0](https://github.com/CedX/Cli/compare/v5.3.0...v5.4.0)
- Ported some [PowerShell](https://learn.microsoft.com/en-us/powershell) types to [C#](https://learn.microsoft.com/en-us/dotnet/csharp) to improve startup performance.

## Version [5.3.0](https://github.com/CedX/Cli/compare/v5.2.0...v5.3.0)
- Updated the package dependencies.

## Version [5.2.0](https://github.com/CedX/Cli/compare/v5.1.1...v5.2.0)
- Updated the list of source folders scanned by the `New-NssmService` and `Remove-NssmService` cmdlets.

## Version [5.1.1](https://github.com/CedX/Cli/compare/v5.1.0...v5.1.1)
- Fixed the entry point of [PowerShell](https://learn.microsoft.com/en-us/powershell) applications installed with the `New-NssmService` cmdlet.

## Version [5.1.0](https://github.com/CedX/Cli/compare/v5.0.1...v5.1.0)
- Merged the nested modules into a single main module.
- Updated the package dependencies.

## Version [5.0.1](https://github.com/CedX/Cli/compare/v5.0.0...v5.0.1)
- Fixed the `ConvertTo-Encoding` cmdlet, which used to stop after processing the first text file.

## Version [5.0.0](https://github.com/CedX/Cli/compare/v4.4.0...v5.0.0)
- Breaking change: restored the [PowerShell](https://learn.microsoft.com/en-us/powershell) source code.
- Breaking change: renamed the `-Path` parameter of the `Install` cmdlets to `-DestinationPath`.
- Breaking change: masked the transitive cmdlets from the public API.
- Updated the package dependencies.

## Version [4.4.0](https://github.com/CedX/Cli/compare/v4.3.0...v4.4.0)
- Added the `-Credential` parameter to the `New-NssmService` cmdlet.

## Version [4.3.0](https://github.com/CedX/Cli/compare/v4.2.1...v4.3.0)
- Added support for 32-bit applications in the `New-NssmService` cmdlet.
- Embedded the [NSSM](https://nssm.cc) executables into the module instead of relying on a pre-installed executable.

## Version [4.2.1](https://github.com/CedX/Cli/compare/v4.2.0...v4.2.1)
- Fixed the XML deserialization of [C#](https://learn.microsoft.com/en-us/dotnet/csharp) projects in the `New-NssmService` and `Remove-NssmService` cmdlets.

## Version [4.2.0](https://github.com/CedX/Cli/compare/v4.1.0...v4.2.0)
- Dropped the support of [JSON](https://www.json.org) format in the `Backup-MySqlTable` cmdlet.
- Updated the package dependencies.

## Version [4.1.0](https://github.com/CedX/Cli/compare/v4.0.0...v4.1.0)
- Updated the package dependencies.

## Version [4.0.0](https://github.com/CedX/Cli/compare/v3.1.0...v4.0.0)
- Added the `-Filter` parameter to `ConvertTo-Encoding` and `Restore-MySqlTable` cmdlets.
- Added the `-Config` parameter to `Install-Node` cmdlet.
- Dropped the support of [PowerShell](https://learn.microsoft.com/en-us/powershell) applications in the `New-NssmService` cmdlet.
- Fixed the base URL of [PHP](https://www.php.net) releases in the `Install-Php` cmdlet.
- Ported the cmdlets of `Compression`, `MySql` and `Validation` modules to [C#](https://learn.microsoft.com/en-us/dotnet/csharp).
- Restored the handling of [NSSM](https://nssm.cc) services in the `Install-Node` cmdlet.

## Version [3.1.0](https://github.com/CedX/Cli/compare/v3.0.0...v3.1.0)
- Added the `ConvertTo-Encoding` cmdlet.
- Added the `-Recurse` parameter to the `Restore-MySqlTable` cmdlet.
- Updated the package dependencies.

## Version [3.0.0](https://github.com/CedX/Cli/compare/v2.7.0...v3.0.0)
- Breaking change: ported the source code to [PowerShell](https://learn.microsoft.com/en-us/powershell).
- Breaking change: removed the installer.

## Version [2.7.0](https://github.com/CedX/Cli/compare/v2.6.0...v2.7.0)
- Added support for XML configuration files to the `nssm install` command.

## Version [2.6.0](https://github.com/CedX/Cli/compare/v2.5.0...v2.6.0)
- Updated the installer to allow the program to be installed in non administrative mode.
- Updated the installer to add the program to the `PATH` environment variable.

## Version [2.5.0](https://github.com/CedX/Cli/compare/v2.4.0...v2.5.0)
- Updated the `iconv` command to exclude specific folders (`.git`, `node_modules` and `vendor`).

## Version [2.4.0](https://github.com/CedX/Cli/compare/v2.3.0...v2.4.0)
- Removed the generic host.
- Updated the package dependencies.

## Version [2.3.0](https://github.com/CedX/Cli/compare/v2.2.0...v2.3.0)
- Use a generic host and dependency injection to handle commands and services.

## Version [2.2.0](https://github.com/CedX/Cli/compare/v2.1.0...v2.2.0)
- Ported the [NSSM](https://nssm.cc) configuration file to [XML](https://www.w3.org/XML) format.
- Renamed the `Belin.Cli.Nssm.Application` class to `WebApplication`.
- Use [Dapper](https://github.com/DapperLib/Dapper) to handle object mapping.

## Version [2.1.0](https://github.com/CedX/Cli/compare/v2.0.0...v2.1.0)
- Added support for [C#](https://learn.microsoft.com/en-us/dotnet/csharp) applications to the `nssm install` command.

## Version [2.0.0](https://github.com/CedX/Cli/compare/v1.2.3...v2.0.0)
- Breaking change: dropped support for **Linux** and **macOS** platforms.
- Breaking change: ported the source code to [C#](https://learn.microsoft.com/en-us/dotnet/csharp).
- Ported the build system to [Cake](https://cakebuild.net).

## Version [1.2.3](https://github.com/CedX/Cli/compare/v1.2.2...v1.2.3)
- Fixed the `node` command: the [NSSM](https://nssm.cc) configuration file is now in [JSON](https://www.json.org) format.
- Ported the build system to [Cake](https://coffeescript.org/#cake).

## Version [1.2.2](https://github.com/CedX/Cli/compare/v1.2.1...v1.2.2)
- Fixed the `php` command.

## Version [1.2.1](https://github.com/CedX/Cli/compare/v1.2.0...v1.2.1)
- Fixed the the `jdk` and `node` commands.

## Version [1.2.0](https://github.com/CedX/Cli/compare/v1.1.0...v1.2.0)
- Provide the `db-backup` and `db-restore` commands.

## Version [1.1.0](https://github.com/CedX/Cli/compare/v1.0.1...v1.1.0)
- Provide the `db-charset`, `db-engine`, `db-optimize` and `iconv` commands.
- Fixed the log file path for `node` command.

## Version [1.0.1](https://github.com/CedX/Cli/compare/v1.0.0...v1.0.1)
- Fixed the packaging.

## Version 1.0.0
- Provide the `jdk`, `node`, `nssm` and `php` commands.
