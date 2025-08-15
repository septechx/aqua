namespace Aqua

module MxmParser =
    open System

    type ParsedUri =
        { Game: string option
          ModId: int option
          FileId: int option
          Params: Map<string, string> }

    let private tryParseInt (s: string) =
        match Int32.TryParse(s) with
        | true, v -> Some v
        | _ -> None

    let private parseQuery (q: string) : Map<string, string> =
        if String.IsNullOrEmpty(q) then
            Map.empty
        else
            q.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries)
            |> Array.choose (fun kv ->
                let i = kv.IndexOf('=')

                if i < 0 then
                    Some(Uri.UnescapeDataString(kv), "")
                else
                    let k = kv.Substring(0, i)
                    let v = kv.Substring(i + 1)
                    Some(Uri.UnescapeDataString(k), Uri.UnescapeDataString(v)))
            |> Map.ofArray

    let parseNxmUri (s: string) : ParsedUri =
        let uri = Uri(s)

        let segments =
            uri.AbsolutePath.Trim('/')
            |> fun t -> if String.IsNullOrWhiteSpace(t) then [||] else t.Split('/')

        let game = if segments.Length > 0 then Some segments.[0] else None

        let indexOfSegment name =
            segments
            |> Array.tryFindIndex (fun seg -> String.Equals(seg, name, StringComparison.OrdinalIgnoreCase))

        let modId =
            match indexOfSegment "mods" with
            | Some i when i + 1 < segments.Length -> tryParseInt segments.[i + 1]
            | _ -> None

        let fileId =
            match indexOfSegment "files" with
            | Some i when i + 1 < segments.Length -> tryParseInt segments.[i + 1]
            | _ -> None

        { Game = game
          ModId = modId
          FileId = fileId
          Params = parseQuery uri.Query }
