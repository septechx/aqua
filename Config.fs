namespace Aqua

module Config =
    open System.IO
    open Microsoft.Extensions.Configuration

    let config =
        ConfigurationBuilder()
        |> fun builder ->
            builder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional = true)
                .Build()
