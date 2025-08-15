namespace Aqua

module Init =
    open System
    open System.IO
    open System.Text.RegularExpressions
    open AquaConfig

    // Obtain the library path to no write a parser
    let extractSteamPaths (vdfContent: string) =
        let pattern = @"^\s*""path""\s+""([^""]+)"""

        Regex.Matches(vdfContent, pattern, RegexOptions.Multiline)
        |> Seq.cast<Match>
        |> Seq.map (fun m -> m.Groups.[1].Value)
        |> List.ofSeq


    let private getGamePath () =
        let libraries =
            File.ReadAllText(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".steam/root/steamapps/libraryfolders.vdf"
                )
            )
            |> extractSteamPaths

        // Brute force the subnautica directory
        let gamePath =
            libraries
            |> List.tryFind (fun libraryPath ->
                Path.Combine(libraryPath, "steamapps", "common", "Subnautica")
                |> Directory.Exists)

        if gamePath.IsNone then
            failwith "Could not find Subnautica installation directory"

        Path.Combine(gamePath.Value, "steamapps", "common", "Subnautica")

    let private createConfigFile (apikey: string) =
        { game_path = getGamePath ()
          api_key = apikey }
        |> AquaConfig.writeConfig


    let init (apikey: string) =
        if not (File.Exists(Path.Combine(Storage.getStorageDir (), "config.json"))) then
            createConfigFile (apikey)
