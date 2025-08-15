namespace Aqua

module NexusApi =
    open System.Net.Http
    open System.Text.Json
    open System.IO

    let baseUrl = "https://api.nexusmods.com"

    type NexusModFile =
        { file_id: int
          name: string
          version: string
          category_id: int
          category_name: string
          is_primary: bool
          size: int
          file_name: string
          uploaded_timestamp: int
          uploaded_time: string
          mod_version: string
          external_virus_scan_url: string
          description: string
          size_kb: int
          size_in_bytes: int
          changelog_html: string
          content_preview_link: string }

    type NexusModFiles = { files: NexusModFile list }

    type NexusDownloadLink =
        { name: string
          short_name: string
          URI: string }

    let private makeRequest (path: string, apiKey: string) =
        let client = new HttpClient()
        let url = sprintf "%s%s" baseUrl path
        let request = new HttpRequestMessage(HttpMethod.Get, url)
        request.Headers.Add("apikey", apiKey)
        request.Headers.Add("Application-Name", Config.config.["Application:Name"])
        request.Headers.Add("Application-Version", Config.config.["Application:Version"])

        request.Headers.Add(
            "User-Agent",
            sprintf "%s/%s" (Config.config.["Application:Name"]) (Config.config.["Application:Version"])
        )

        let response =
            client.SendAsync(request) |> Async.AwaitTask |> Async.RunSynchronously

        if not response.IsSuccessStatusCode then
            failwithf "HTTP Error: %d" (int response.StatusCode)

        response

    let validate () =
        let apiKey = DotEnv.get "NEXUS_API_KEY"

        let response = makeRequest ("/v1/users/validate.json", apiKey)

        response.Content.ReadAsStringAsync()
        |> Async.AwaitTask
        |> Async.RunSynchronously

    let getFiles (apiKey: string, modId: int) =
        let response =
            makeRequest (sprintf "/v1/games/subnautica/mods/%d/files.json" modId, apiKey)

        let jsonString =
            response.Content.ReadAsStringAsync()
            |> Async.AwaitTask
            |> Async.RunSynchronously

        let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
        JsonSerializer.Deserialize<NexusModFiles>(jsonString, options).files

    let getDownloadLink (apiKey: string, modId: int, fileId: int, expires: string, key: string) =
        let response =
            makeRequest (
                sprintf
                    "/v1/games/subnautica/mods/%d/files/%d/download_link.json?key=%s&expires=%s"
                    modId
                    fileId
                    key
                    expires,
                apiKey
            )

        let jsonString =
            response.Content.ReadAsStringAsync()
            |> Async.AwaitTask
            |> Async.RunSynchronously

        let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
        JsonSerializer.Deserialize<NexusDownloadLink list>(jsonString, options).Head.URI

    let downloadZipAsync (url: string, fileName: string) =
        async {
            let cacheDir = Storage.getCacheDir ()
            Directory.CreateDirectory(cacheDir) |> ignore
            let destPath = Path.Combine(cacheDir, fileName)

            use http = new HttpClient()

            use request = new HttpRequestMessage(HttpMethod.Get, url)
            request.Headers.Add("Application-Name", Config.config.["Application:Name"])
            request.Headers.Add("Application-Version", Config.config.["Application:Version"])

            request.Headers.Add(
                "User-Agent",
                sprintf "%s/%s" (Config.config.["Application:Name"]) (Config.config.["Application:Version"])
            )

            use! response =
                http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                |> Async.AwaitTask

            response.EnsureSuccessStatusCode() |> ignore

            use! sourceStream = response.Content.ReadAsStreamAsync() |> Async.AwaitTask

            use destStream =
                new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, useAsync = true)

            do! sourceStream.CopyToAsync(destStream) |> Async.AwaitTask |> Async.Ignore

            return destPath
        }
