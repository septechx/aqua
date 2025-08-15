namespace Aqua

module Refresh =
    open System.IO
    open AquaConfig

    let refresh () =
        let config = AquaConfig.getConfig ()
        let modsPath = Path.Combine(Storage.getStorageDir (), "installed-mods.json")

        if not (modsPath |> File.Exists) then
            failwith "No installed mods found"

        let mods = Storage.loadJsonData<InstalledMod list> modsPath

        for modInfo in mods do
            let destPath = Path.Combine(config.game_path, "BepInEx", "plugins", modInfo.name)

            if Directory.Exists destPath then
                Directory.Delete(destPath, true)

            File.CreateSymbolicLink(destPath, modInfo.source_path) |> ignore
