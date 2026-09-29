namespace Belin.Cli

open System
open System.Management.Automation
open System.Net.Http

/// Contains operations for HTTP requests.
module Http =

  /// The assembly version.
  let private Version = typeof<Architecture>.Assembly.GetName().Version |> nonNull

  /// Creates a new HTTP client.
  let newClient (): HttpClient =
    let client = new HttpClient(Timeout = TimeSpan.FromMinutes 1L)
    client.DefaultRequestHeaders.Add("User-Agent", $"PowerShell/{PSVersionInfo.PSVersion} | Belin.Cli/{Version.ToString 3}")
    client
