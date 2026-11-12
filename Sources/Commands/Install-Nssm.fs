namespace Belin.Cli.Cmdlets

open Belin.Cli
open System
open System.IO
open System.Management.Automation

/// Installs the latest NSSM release.
/// Returns the output from the `nssm --version` command.
[<Cmdlet(VerbsLifecycle.Install, "Nssm"); OutputType(typeof<string>)>]
type InstallNssm() =
  inherit PSCmdlet()

  /// The path to the output directory.
  [<Parameter(Position = 1); ValidatePath("The specified output path is invalid.")>]
  member val DestinationPath = if Platform.IsWindows then @"C:\Windows\System32" else "/usr/local/bin" with get, set

  /// Performs initialization of command execution.
  override this.BeginProcessing() =
    if not Platform.IsWindows then
      let ex = PlatformNotSupportedException "This command only supports the Windows platform."
      ErrorRecord(ex, "PlatformNotSupported", ErrorCategory.InvalidOperation, Environment.OSVersion) |> this.ThrowTerminatingError

  /// Performs execution of this command.
  override this.ProcessRecord() =
    let destinationPath = this.GetUnresolvedProviderPathFromPSPath this.DestinationPath
    if not (Security.canWriteTo destinationPath) then
      let ex = UnauthorizedAccessException "You must run this command in an elevated prompt."
      ErrorRecord(ex, "UnauthorizedAccess", ErrorCategory.PermissionDenied, destinationPath) |> this.ThrowTerminatingError

    InformationRecord($@"Copying the binary into the directory ""{destinationPath}""...", nameof this.ProcessRecord) |> this.WriteInformation
    let executable = Path.Join(destinationPath, if Platform.IsWindows then "nssm.exe" else "nssm")
    Directory.CreateDirectory destinationPath |> ignore<DirectoryInfo>
    File.Copy(Nssm.getPath Architecture.Current, executable, overwrite = true)

    if not Platform.IsWindows then File.makeExecutable executable
    executable |> Diagnostics.getExecutableVersion |> this.WriteObject
