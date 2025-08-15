namespace Aqua

module Cli =
    module Install =
        open System
        open Argu

        type InstallArgs =
            | [<Mandatory; MainCommand>] Mods of mxm_link: string list

            interface IArgParserTemplate with
                member this.Usage =
                    match this with
                    | Mods _ -> "mods to install"

        let run (args: ParseResults<InstallArgs>) =
            match args.TryGetResult <@ Mods @> with
            | None -> raise (ArgumentException(message = "No mods specified", paramName = "mods"))
            | Some mods ->
                for mxm in mods do
                    InstallMod.install (mxm)


    open System
    open Argu

    let application_version = Config.config.["Application:Version"]

    type CmdArgs =
        | [<AltCommandLine("-v")>] Version
        | [<AltCommandLine("i"); CliPrefix(CliPrefix.None)>] Install of ParseResults<Install.InstallArgs>
        | Validate

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Version -> "display version information."
                | Validate -> "validate api key"
                | Install _ -> "install mods"


    let private printVersion () = printfn "aqua v%s" application_version

    let run (argv: string array) =
        let errorHandler =
            ProcessExiter(
                colorizer =
                    function
                    | ErrorCode.HelpText -> None
                    | _ -> Some ConsoleColor.Red
            )

        let parser =
            ArgumentParser.Create<CmdArgs>(programName = "aqua", errorHandler = errorHandler)

        match parser.ParseCommandLine argv with
        | p when p.Contains(Version) ->
            printVersion ()
            0
        | p when p.Contains(Install) ->
            Install.run (p.GetResult(Install))
            0
        | p when p.Contains(Validate) ->
            printfn "Validating API key..."
            let response = NexusApi.validate ()
            printfn "%s" response
            0
        | _ ->
            printfn "%s" (parser.PrintUsage())
            1
