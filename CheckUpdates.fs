namespace Aqua

module CheckUpdates =
    open AquaConfig
    open System.IO

    let check () =
        let config = AquaConfig.getConfig ()
        let modsPath = Path.Combine(Storage.getStorageDir (), "installed-mods.json")

        if not (modsPath |> File.Exists) then
            failwith "No installed mods found"

        let mods = Storage.loadJsonData<InstalledMod list> modsPath

        for modInfo in mods do
            printfn "Checking for updates for %s" modInfo.friendly_name

            let remoteModInfo = NexusApi.getModInfo config.api_key modInfo.mod_id

            if remoteModInfo.version = modInfo.version then
                printfn "%s  No updates available%s" Colors.green Colors.reset
            else
                printfn "%s  Update available%s" Colors.red Colors.reset

            printfn "  Current version: %s" modInfo.version
            printfn "  Latest version: %s" remoteModInfo.version
