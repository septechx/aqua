namespace Aqua

module NexusApi =
    open System.Net.Http
    open System.Text.Json
    open System.IO

    let baseUrl = "https://api.nexusmods.com"

    type NexusModInfo = { name: string; version: string }

    type NexusModFile =
        { file_id: int
          name: string
          version: string
          file_name: string
          mod_version: string }

    type NexusModFiles = { files: NexusModFile list }

    type NexusDownloadLink = { URI: string }

    let private makeRequest (apiKey: string) (path: string) =
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
            failwithf
                "HTTP Error: %d:\n%s"
                (int response.StatusCode)
                (response.Content.ReadAsStringAsync()
                 |> Async.AwaitTask
                 |> Async.RunSynchronously)

        response

    let getModInfo (apiKey: string) (modId: int) =
        let response =
            sprintf "/v1/games/subnautica/mods/%d.json" modId |> makeRequest apiKey

        let jsonString =
            response.Content.ReadAsStringAsync()
            |> Async.AwaitTask
            |> Async.RunSynchronously

        let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
        JsonSerializer.Deserialize<NexusModInfo>(jsonString, options)

    let getFiles (apiKey: string) (modId: int) =
        let response =
            sprintf "/v1/games/subnautica/mods/%d/files.json" modId |> makeRequest apiKey

        let jsonString =
            response.Content.ReadAsStringAsync()
            |> Async.AwaitTask
            |> Async.RunSynchronously

        let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
        JsonSerializer.Deserialize<NexusModFiles>(jsonString, options).files

    let getFile (apiKey: string) (modId: int) (fileId: int) =
        let response =
            sprintf "/v1/games/subnautica/mods/%d/files/%d.json" modId fileId
            |> makeRequest apiKey

        let jsonString =
            response.Content.ReadAsStringAsync()
            |> Async.AwaitTask
            |> Async.RunSynchronously

        let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
        JsonSerializer.Deserialize<NexusModFile>(jsonString, options)

    let getDownloadLink (apiKey: string) (modId: int) (fileId: int) (expires: string) (key: string) =
        let response =
            sprintf
                "/v1/games/subnautica/mods/%d/files/%d/download_link.json?key=%s&expires=%s"
                modId
                fileId
                key
                expires
            |> makeRequest apiKey

        let jsonString =
            response.Content.ReadAsStringAsync()
            |> Async.AwaitTask
            |> Async.RunSynchronously

        let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
        JsonSerializer.Deserialize<NexusDownloadLink list>(jsonString, options).Head.URI

    let downloadZipAsync (url: string) (fileName: string) =
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
