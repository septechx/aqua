namespace Aqua

module Cli =
    open System.Collections.Generic
    open CommandLine

    let application_version = "0.1.0"
    let application_name = "Aqua"

    type options =
        { [<Option('h', "help", Default = false, HelpText = "Display this help.")>]
          help: bool
          [<Option('v', "version", Default = false, HelpText = "Display version information.")>]
          version: bool }

    let private printHelp () =
        printfn "Usage: aqua [options]"
        printfn ""
        printfn "Options:"
        printfn "  -h, --help      Display this help."
        printfn "  -v, --version   Display version information."
        printfn ""
        printfn "Examples:"
        printfn "  aqua -h"
        printfn "  aqua -v"

    let private printVersion () = printfn "aqua v%s" application_version

    let run (o: options) =
        if o.help then printHelp ()
        elif o.version then printVersion ()
        else printHelp ()

        0

    let fail (e: IEnumerable<Error>) =
        for error in e do
            printfn "%A" error

        1
