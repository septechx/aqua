namespace Aqua

module ListMods =
    open System.IO
    open AquaConfig

    let list () =
        let modsFile = "installed-mods.json"

        if Path.Combine(Storage.getStorageDir (), modsFile) |> File.Exists then
            Storage.loadJsonData<InstalledMod list> modsFile
            |> List.map (fun m ->
                sprintf
                    "%s[v%s] %s%s"
                    (if m.enabled then Colors.green else Colors.red)
                    m.version
                    m.friendly_name
                    Colors.reset)
            |> List.sort
            |> String.concat "\n"
            |> printfn "%s"
        else
            failwith "No installed mods found"
