namespace Belin.Cli

open System
open System.Management.Automation
open System.Net.Http

/// Contains operations for HTTP requests.
module Http =

  /// The assembly version.
  let private version = nonNull (typeof<Architecture>.Assembly.GetName().Version)

  /// Creates a new HTTP client.
  let newClient(): HttpClient =
    let httpClient = new HttpClient(Timeout = TimeSpan.FromMinutes 1L)
    httpClient.DefaultRequestHeaders.Add("User-Agent", $".NET/{PSVersionInfo.PSVersion} | Belin.Cli/{version.ToString 3}")
    httpClient
