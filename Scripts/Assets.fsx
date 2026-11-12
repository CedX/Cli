#load "Tools.fsx"
open System
open System.IO
open System.Net.Http
open Tools

writeOutput "Deploying the assets..."
let httpClient = new HttpClient()
httpClient.BaseAddress <- Uri "https://raw.githubusercontent.com/sindresorhus/"
httpClient.DefaultRequestHeaders.Add("User-Agent", $".NET/{Environment.Version} | Belin.Cli/{Version}")

for name in ["Binary"; "Text"] do
  let file = $"{name.ToLowerInvariant()}-extensions"
  let json = httpClient.GetStringAsync($"{file}/main/{file}.json").GetAwaiter().GetResult()
  File.WriteAllText($"Resources/Text/{name}Extensions.json", json)
