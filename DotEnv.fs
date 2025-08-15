namespace Aqua

module DotEnv =
    open System
    open System.IO

    let private parseLine (line: string) =
        match line.Split('=') with
        | args ->
            Environment.SetEnvironmentVariable(
                args.[0],
                args.[1..]
                |> String.concat "="
                |> fun s ->
                    if s.StartsWith '"' && s.EndsWith '"' then
                        s.Substring(1, s.Length - 2)
                    else
                        s

            )

    let private load () =
        lazy
            (let dir = Directory.GetCurrentDirectory()
             let filePath = Path.Combine(dir, ".env")

             filePath
             |> File.Exists
             |> function
                 | false -> Console.WriteLine "No .env file found."
                 | true -> filePath |> File.ReadAllLines |> Seq.iter parseLine)

    let get key = Environment.GetEnvironmentVariable key

    let init = load().Value
