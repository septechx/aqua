namespace Aqua

module ToggleMod =
    open System.IO


    let enable (modName: string) =
        let modsFile = "installed-mods.json"
        let modsPath = Path.Combine(Storage.getStorageDir (), modsFile)

        if File.Exists modsPath then
            Storage.loadJsonData<InstallMod.InstalledMod list> modsPath
            |> List.map (fun m ->
                if m.friendly_name = modName then
                    File.CreateSymbolicLink(m.destination_path, m.source_path) |> ignore
                    { m with enabled = true }
                else
                    m)
            |> Storage.saveJsonData modsPath

            printfn "Enabled %s" modName
        else
            failwith "No installed mods found"

    let disable (modName: string) =
        let modsFile = "installed-mods.json"
        let modsPath = Path.Combine(Storage.getStorageDir (), modsFile)

        if File.Exists modsPath then
            Storage.loadJsonData<InstallMod.InstalledMod list> modsPath
            |> List.map (fun m ->
                if m.friendly_name = modName then
                    File.Delete(m.destination_path) |> ignore
                    { m with enabled = false }
                else
                    m)
            |> Storage.saveJsonData modsPath

            printfn "Disabled %s" modName
        else
            failwith "No installed mods found"
