namespace Aqua

module Program =
    open CommandLine
    open Cli

    [<EntryPoint>]
    let main argv =
        DotEnv.init

        let result = Parser.Default.ParseArguments<options>(argv)

        match result with
        | :? Parsed<options> as parsed -> run parsed.Value
        | :? NotParsed<options> as notParsed -> fail notParsed.Errors
        | _ -> 1
