namespace Aqua

module ListMods =
    open System.IO

    let list () =
        let modsFile = "installed-mods.json"

        if Path.Combine(Storage.getStorageDir (), modsFile) |> File.Exists then
            Storage.loadJsonData<InstallMod.InstalledMod list> modsFile
            |> List.map (fun m ->
                sprintf "%s%s %s%s" (if m.enabled then "\x1b[32m" else "\x1b[31m") m.friendly_name m.version "\x1b[0m")
            |> List.sort
            |> String.concat "\n"
            |> printfn "%s"
        else
            failwith "No installed mods found"
