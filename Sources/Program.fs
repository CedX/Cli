open Belin.Cli
open Microsoft.Extensions.DependencyInjection
open System
open System.Reflection

// Configure the dependency container.
let services =
  new ServiceCollection()
  |> Container.configureServices
  |> Container.configureCommands

// Start the application.
Console.Title <-
  (nonNull (Container.Assembly.GetCustomAttribute<AssemblyProductAttribute>())).Product

let args = Environment.GetCommandLineArgs()
Environment.ExitCode <-
  services
    .BuildServiceProvider()
    .GetRequiredService<RootCommand>()
    .Parse(if args.Length > 0 then args else [| "--help" |])
    .Invoke()
