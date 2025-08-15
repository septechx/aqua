set positional-arguments

@build version:
    dotnet pack ./aqua.fsproj -p Version=$1

install:
    dotnet tool install -g --add-source "./bin/Release/" "Aqua"
