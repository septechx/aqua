namespace Aqua

module Unzip =
    open System
    open System.IO
    open System.IO.Compression

    let rec private copyDirRecursive (src: string) (dst: string) =
        Directory.CreateDirectory(dst) |> ignore

        for f in Directory.EnumerateFiles(src) do
            let destFile = Path.Combine(dst, Path.GetFileName(f))
            File.Copy(f, destFile, true)

        for d in Directory.EnumerateDirectories(src) do
            let nextDst = Path.Combine(dst, Path.GetFileName(d))
            copyDirRecursive d nextDst

    let moveDirFromZip (zipPath: string) (dirName: string) (destination: string) (overwrite: bool) =
        if not (File.Exists zipPath) then
            invalidArg "zipPath" (sprintf "Zip file not found: %s" zipPath)

        let temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(temp) |> ignore

        try
            ZipFile.ExtractToDirectory(zipPath, temp)

            // TODO: Replace this with something better
            let matches =
                Directory.EnumerateDirectories(temp, "*", SearchOption.AllDirectories)
                |> Seq.filter (fun dir ->
                    Path
                        .GetFileName(dir)
                        .Replace(" ", "")
                        .StartsWith(dirName.Substring(0, 5).Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
                |> Seq.toList

            match matches with
            | [] -> failwithf "No directory starting with '%s' found inside zip" dirName

            | [ single ] ->
                let ensureDestination () =
                    if Directory.Exists destination then
                        if overwrite then
                            Directory.Delete(destination, true)
                        else
                            failwithf "Destination already exists: %s" destination

                ensureDestination ()

                try
                    Directory.Move(single, destination)
                with :? IOException ->
                    copyDirRecursive single destination
                    Directory.Delete(single, true)

            | many ->
                let depth (p: string) =
                    p
                        .Substring(temp.Length)
                        .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        .Split(
                            [| Path.DirectorySeparatorChar; Path.AltDirectorySeparatorChar |],
                            StringSplitOptions.RemoveEmptyEntries
                        )
                        .Length

                let best = List.minBy depth many

                let ensureDestination () =
                    if Directory.Exists destination then
                        if overwrite then
                            Directory.Delete(destination, true)
                        else
                            failwithf "Destination already exists: %s" destination

                ensureDestination ()

                try
                    Directory.Move(best, destination)
                with :? IOException ->
                    copyDirRecursive best destination
                    Directory.Delete(best, true)
        finally
            if Directory.Exists temp then
                try
                    Directory.Delete(temp, true)
                with _ ->
                    ()
