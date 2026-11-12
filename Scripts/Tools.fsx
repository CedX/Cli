open System
open System.Diagnostics
open System.IO
open System.Xml.Linq

/// Contains operations for building a solution.
[<AutoOpen>]
module Tools =

  /// Value indicating whether the requested configuration is "Release".
  let IsRelease: bool =
    fsi.CommandLineArgs |> Array.tail |> Array.exists (fun arg -> arg.Equals("--release", StringComparison.OrdinalIgnoreCase))

  /// The configuration to use for project generation.
  let Configuration: string =
    if IsRelease then "Release" else "Debug"

  /// The path to the solution file.
  let Solution: FileInfo =
    Directory.EnumerateFiles(Environment.CurrentDirectory, "*.slnx") |> Seq.exactlyOne |> FileInfo

  /// The project version number.
  let Version: Version =
    XDocument.Load(Path.Join(Environment.CurrentDirectory, "Package.xml")).Descendants "Version"
    |> Seq.exactlyOne
    |> fun element -> Version.Parse element.Value

  /// Deletes the file system entry located at the specified path.
  let removeItem (path: string) =
    if Directory.Exists path then Directory.Delete(path, recursive = true)
    else File.Delete path

  /// Cleans the directory located at the specified path.
  let clearItem (path: string) =
    Directory.EnumerateFileSystemEntries path
    |> Seq.filter (fun entry -> Path.GetFileName entry <> ".gitkeep")
    |> Seq.iter removeItem

  /// Prints the specified message to the standard error and terminates the process.
  let writeError (message: objnull) =
    eprintfn "%s" (Convert.ToString message)
    Environment.Exit 1

  /// Prints the specified message to the standard output.
  let writeOutput (message: objnull) =
    printn message

  /// Invokes the specified command.
  let startProcess (command: string) (arguments: string) =
    try
      use startedProcess = Process.Start(command, arguments)
      startedProcess.WaitForExit()
      if startedProcess.ExitCode <> 0 then failwith ($"Command failed: {command} {arguments}".TrimEnd())
    with
      ex -> writeError ex.Message
