namespace Belin.Cli.Nssm

open System.CommandLine

/// Registers a Web application as a Windows service.
type NssmModule() as this =

  inherit Command("mysql", "Register a Web application as a Windows service.")
  do ()
