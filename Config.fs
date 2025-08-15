namespace Aqua

module Config =
    open System.IO
    open Microsoft.Extensions.Configuration

    let basePath =
        System.Reflection.Assembly.GetExecutingAssembly().Location
        |> Path.GetDirectoryName

    let config =
        ConfigurationBuilder()
        |> fun builder -> builder.SetBasePath(basePath).AddJsonFile("appsettings.json", optional = true).Build()
