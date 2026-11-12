open Belin.Cli
open Microsoft.Extensions.DependencyInjection
open System
open System.Reflection

// Customize the console title bar.
Console.Title <-
  Container.Assembly.GetCustomAttribute<AssemblyProductAttribute>()
  |> nonNull
  |> _.Product

// Configure the dependency container.
let services =
  new ServiceCollection()
  |> Container.configureServices
  |> Container.configureCommands

// Start the application.
let args = Environment.GetCommandLineArgs() |> Array.skip 1
Environment.ExitCode <-
  services
    .BuildServiceProvider()
    .GetRequiredService<RootCommand>()
    .Parse(if args.Length > 0 then args else [| "--help" |])
    .Invoke()
