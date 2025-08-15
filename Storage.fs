namespace Aqua

module Storage =
    open System
    open System.IO
    open System.Text.Json

    let getCacheDir () =
        let baseDir =
            Environment.GetEnvironmentVariable("XDG_CACHE_HOME")
            |> Option.ofObj
            |> Option.defaultValue (
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache")
            )

        let appDir = Path.Combine(baseDir, "aqua")
        Directory.CreateDirectory(appDir) |> ignore
        appDir

    let getStorageDir () =
        let baseDir =
            Environment.GetEnvironmentVariable("XDG_DATA_HOME")
            |> Option.ofObj
            |> Option.defaultValue (
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share")
            )

        let appDir = Path.Combine(baseDir, "aqua")
        Directory.CreateDirectory(appDir) |> ignore
        appDir

    let saveTextData (filename: string) (content: string) =
        let filePath = Path.Combine(getStorageDir (), filename)
        File.WriteAllText(filePath, content)

    let loadTextData (filename: string) =
        Path.Combine(getStorageDir (), filename) |> File.ReadAllText

    let saveJsonData (filename: string) (data: 'a) =
        let filePath = Path.Combine(getStorageDir (), filename)
        let json = JsonSerializer.Serialize(data)
        File.WriteAllText(filePath, json)

    let loadJsonData<'a> (filename: string) =
        Path.Combine(getStorageDir (), filename)
        |> File.ReadAllText
        |> JsonSerializer.Deserialize<'a>
