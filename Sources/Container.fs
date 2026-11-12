namespace Belin.Cli

open Microsoft.Extensions.DependencyInjection
open System
open System.CommandLine
open System.Net.Http

/// Contains operations for configuring the dependency container.
module Container =

  /// The application assembly.
  let Assembly = typeof<RootCommand>.Assembly

  /// Registers the application commands.
  let configureCommands (services: IServiceCollection): IServiceCollection =
    let commands = query {
      for definedType in Assembly.DefinedTypes do
      where (not definedType.IsAbstract && definedType.IsSubclassOf typeof<Command>)
      select (definedType.AsType())
    }

    for command in commands do services.AddTransient command |> ignore<IServiceCollection>
    services

  /// Registers the application services.
  let configureServices (services: IServiceCollection): IServiceCollection =
    services
      .AddTransient<HttpClient>(fun _ ->
        let httpClient = new HttpClient()
        httpClient.DefaultRequestHeaders.Add("User-Agent", $".NET/{Environment.Version} | Belin.Cli/{Assembly.GetName().Version}")
        httpClient
      )
