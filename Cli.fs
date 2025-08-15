namespace Aqua

module Cli =
    module Install =
        open System
        open System.IO
        open Argu
        open Storage

        type InstallArgs =
            | [<Mandatory; MainCommand>] Mods of mxm_link: string list

            interface IArgParserTemplate with
                member this.Usage =
                    match this with
                    | Mods _ -> "mods to install"

        let run (args: ParseResults<InstallArgs>) =
            if not (File.Exists(Path.Combine(getStorageDir (), "config.json"))) then
                failwith "No config file found. Please run `aqua init` first."

            let aquaConfig =
                Path.Combine(Storage.getStorageDir (), "config.json")
                |> Storage.loadJsonData<InstallMod.AquaConfig>

            if not (Directory.Exists(Path.Combine(aquaConfig.game_path, "BepInEx"))) then
                failwith "BepInEx is not installed. Please install BepInEx first."

            match args.TryGetResult <@ Mods @> with
            | None -> raise (ArgumentException(message = "No mods specified", paramName = "mods"))
            | Some mods ->
                for mxm in mods do
                    InstallMod.install (mxm)

    module Init =
        open System
        open Argu

        type InitArgs =
            | [<Mandatory; MainCommand>] ApiKey of api_key: string

            interface IArgParserTemplate with
                member this.Usage =
                    match this with
                    | ApiKey _ -> "api key for nexus"

        let run (args: ParseResults<InitArgs>) =
            match args.TryGetResult <@ ApiKey @> with
            | None -> raise (ArgumentException(message = "No api key specified", paramName = "api_key"))
            | Some key -> Init.init (key)

    open System
    open Argu

    let application_version = Config.config.["Application:Version"]

    type CmdArgs =
        | [<AltCommandLine("-v")>] Version
        | [<CliPrefix(CliPrefix.None); AltCommandLine("i")>] Install of ParseResults<Install.InstallArgs>
        | [<CliPrefix(CliPrefix.None)>] Init of ParseResults<Init.InitArgs>

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Version -> "display version information."
                | Install _ -> "install mods"
                | Init _ -> "initialize aqua"

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
        | p when p.Contains(Init) ->
            Init.run (p.GetResult(Init))
            0
        | _ ->
            printfn "%s" (parser.PrintUsage())
            1
