namespace Aqua

module Program =
    [<EntryPoint>]
    let main argv =
        DotEnv.init

        Cli.run (argv)
