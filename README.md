<div align="center">
    <img src="assets/aqua_large.png" alt="Aqua" width="66%"  />
</div>
<br />

Aqua is a simple CLI tool for managing subnautica 2.0+ mods.

**Warning: Aqua is still in development and may not work as expected. Minor updates might break your configuration.**

## Features

- [x] Install mods from Nexus with a nxm link
- [x] List mods
- [x] Check for updates
- [x] Disable mods
- [x] Enable mods
- [ ] Automatically update mods
- [ ] Automatically install BepInEx

## Installation

First, download the latest release from the [releases page](https://github.com/septechx/aqua/releases).

Then run the following command to install Aqua:

```bash
dotnet tool install -g --add-source "<directory with the downloaded release>" "Aqua"
```

## Usage

Run `aqua --help` to see the available commands.

First, initialize aqua by running `aqua init <api key>`. <br>
You can get your nexus api key in your nexusmods profile settings.

### Installing a mods

Run `aqua install <nxm link>` to install a mod. <br>
You can get the nxm link by clicking the "Vortex" button in the mod's page and then copying the orange link after pressing download and selecting one of the options.

Running `aqua list` will show the installed mods and their versions.

You can enable or disable mods with `aqua enable <mod name>` and `aqua disable <mod name>`.

You can check for updates with `aqua update`.

### Updating BepInEx

After updating BepInEx, you will have to run `aqua refresh` to recreate the symlinks if they were deleted.
