namespace Aqua

module InstallMod =
    open NexusApi
    open Semver
    open System.IO

    type InstalledMod = { name: string; version: string }

    type AquaConfig = { game_path: string; api_key: string }

    let private getLatestFile (files: NexusModFile list) : NexusModFile option =
        let tryParseSemVer (version: string) =
            match SemVersion.TryParse(version) with
            | (true, semver) -> Some semver
            | _ -> None

        files
        |> List.choose (fun file -> tryParseSemVer file.version |> Option.map (fun semver -> (semver, file)))
        |> List.fold
            (fun acc (version, file) ->
                match acc with
                | None -> Some(version, file)
                | Some(maxVersion, _) when version.CompareSortOrderTo(maxVersion) > 0 -> Some(version, file)
                | current -> current)
            None
        |> Option.map snd

    let install (mxm: string) =
        let config =
            Path.Combine(Storage.getStorageDir (), "config.json")
            |> Storage.loadJsonData<AquaConfig>

        let parsedMxm = MxmParser.parseNxmUri mxm

        let file = getFile (config.api_key, parsedMxm.ModId.Value, parsedMxm.FileId.Value)

        let modsFile = "installed-mods.json"

        if Path.Combine(Storage.getStorageDir (), modsFile) |> File.Exists then
            Storage.loadJsonData<InstalledMod list> modsFile
            |> List.filter (fun m -> m.name <> file.name)
        else
            []
        @ [ { name = file.name
              version = file.version } ]
        |> Storage.saveJsonData modsFile

        let resultPath =
            NexusApi.getDownloadLink (
                config.api_key,
                parsedMxm.ModId.Value,
                file.file_id,
                parsedMxm.Params.["expires"],
                parsedMxm.Params.["key"]
            )
            |> fun downloadLink ->
                NexusApi.downloadZipAsync (downloadLink, sprintf "%s-%s.zip" file.name file.version)
                |> Async.RunSynchronously

        let modPath = Path.Combine(Storage.getStorageDir (), file.name)

        Unzip.moveDirFromZip (resultPath, file.name, modPath, true)

        let aquaConfig =
            Path.Combine(Storage.getStorageDir (), "config.json")
            |> Storage.loadJsonData<AquaConfig>

        File.CreateSymbolicLink(Path.Combine(aquaConfig.game_path, "BepInEx", "plugins", file.name), modPath)
        |> ignore

        ()
