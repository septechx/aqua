namespace Aqua

module InstallMod =
    open NexusApi
    open Semver
    open System.IO
    open AquaConfig

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
        let config = AquaConfig.getConfig ()

        let parsedMxm = MxmParser.parseNxmUri mxm

        let modInfo = getModInfo config.api_key parsedMxm.ModId.Value

        let file = getFile config.api_key parsedMxm.ModId.Value parsedMxm.FileId.Value

        let resultPath =
            sprintf "%s-%s.zip" file.name file.version
            |> NexusApi.downloadZipAsync (
                NexusApi.getDownloadLink
                    config.api_key
                    parsedMxm.ModId.Value
                    parsedMxm.FileId.Value
                    parsedMxm.Params.["expires"]
                    parsedMxm.Params.["key"]
            )
            |> Async.RunSynchronously

        let modPath = Path.Combine(Storage.getStorageDir (), file.name)

        Unzip.moveDirFromZip resultPath file.name modPath true

        let destPath = Path.Combine(config.game_path, "BepInEx", "plugins", file.name)

        File.CreateSymbolicLink(destPath, modPath) |> ignore

        let modsFile = "installed-mods.json"

        if Path.Combine(Storage.getStorageDir (), modsFile) |> File.Exists then
            Storage.loadJsonData<InstalledMod list> modsFile
            |> List.filter (fun m -> m.name <> file.name)
        else
            []
        @ [ { name = file.name
              version = file.version
              friendly_name = modInfo.name
              enabled = true
              source_path = modPath
              destination_path = destPath
              mod_id = parsedMxm.ModId.Value } ]
        |> Storage.saveJsonData modsFile
