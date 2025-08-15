namespace Aqua

module NexusApi =
    open System.Net.Http
    open System.Text.Json
    open System.IO

    let baseUrl = "https://api.nexusmods.com"

    type NexusModInfo =
        { name: string
          summary: string
          description: string
          picture_url: string
          mod_downloads: int
          mod_unique_downloads: int
          uid: int
          mod_id: int
          game_id: int
          allow_rating: bool
          domain_name: string
          category_id: int
          version: string
          endorsement_count: int
          created_timestamp: int
          created_time: string
          updated_timestamp: int
          updated_time: string
          author: string
          uploaded_by: string
          uploaded_users_profile_url: string
          contains_adult_content: bool
          status: string
          available: bool }

    type NexusModFile =
        { file_id: int
          name: string
          version: string
          file_name: string
          mod_version: string }

    type NexusModFiles = { files: NexusModFile list }

    type NexusDownloadLink = { URI: string }

    let private makeRequest (path: string) (apiKey: string) =
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
            modId |> sprintf "/v1/games/subnautica/mods/%d/files.json" |> makeRequest apiKey

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
