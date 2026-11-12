namespace Belin.Cli.Commands

open Belin.Cli
open System
open System.Collections.Generic
open System.IO
open System.Management.Automation
open System.Net.Http
open System.Net.Http.Json
open System.ServiceProcess
open System.Threading.Tasks

/// Downloads and installs the latest Gitea release.
/// Returns the output from the `gitea --version` command.
[<Cmdlet(VerbsLifecycle.Install, "Gitea"); OutputType(typeof<string>); Sealed>]
type InstallGitea() =

  /// Performs execution of this command.
  override this.ProcessRecord() =
    let destinationPath = this.GetUnresolvedProviderPathFromPSPath this.DestinationPath
    if not (Security.canWriteTo destinationPath) then
      let ex = UnauthorizedAccessException "You must run this command in an elevated prompt."
      ErrorRecord(ex, "UnauthorizedAccess", ErrorCategory.PermissionDenied, destinationPath)

    let version = this.FetchLatestVersion().GetAwaiter().GetResult()
    let binary = this.DownloadBinary(version).GetAwaiter().GetResult()
    let executable = Path.Join(destinationPath, if OperatingSystem.IsWindows() then "gitea.exe" else "gitea")

    use serviceController = new ServiceController "Gitea"
    if Environment.IsPrivilegedProcess then ServiceProcess.stopService serviceController

    Console.WriteLine $@"Moving the binary into the directory ""{destinationPath}""...")
    Directory.CreateDirectory destinationPath |> ignore<DirectoryInfo>
    File.Copy(binary.FullName, executable, overwrite = true)
    if not OperatingSystem.IsWindows() then File.makeExecutable executable

    if Environment.IsPrivilegedProcess then ServiceProcess.startService serviceController
    executable |> Diagnostics.getExecutableVersion |> this.WriteObject
