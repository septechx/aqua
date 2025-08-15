namespace Aqua

module AquaConfig =
    open System.IO

    type InstalledMod =
        { name: string
          version: string
          friendly_name: string
          enabled: bool
          source_path: string
          destination_path: string
          mod_id: int }

    type AquaConfig = { game_path: string; api_key: string }

    let getConfig () =
        Path.Combine(Storage.getStorageDir (), "config.json")
        |> Storage.loadJsonData<AquaConfig>

    let writeConfig (config: AquaConfig) =
        Storage.saveJsonData (Path.Combine(Storage.getStorageDir (), "config.json")) config
