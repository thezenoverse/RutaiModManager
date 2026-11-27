# Rutai Mod Manager

A mod manager for **Sonic & All-Stars Racing Transformed** (PC), forked from
[Hedge Mod Manager](https://github.com/thesupersonic16/HedgeModManager) 7.x by thesupersonic16.
Hedge Mod Manager keeps managing the Hedgehog Engine games; Rutai Mod Manager is a separate
program that can be installed next to it (own settings, own URL handler, own loader).

### Supported games
- Sonic & All-Stars Racing Transformed (Steam, app 212480)

More Sumo Digital titles can be added later in `RutaiModManager/Game.cs`.

## How it works

| Part | What it does |
| --- | --- |
| `RutaiModManager/` | The WPF manager (fork of HMM). Detects the game through Steam, manages the `mods` folder, profiles, mod configs, updates. Writes `cpkredir.ini` (game folder) and `mods/ModsDB.ini`, same formats as HMM. |
| [Sumo Engine Mod Loader](https://github.com/thezenoverse/SumoModLoader) (own repo) | `dinput8.dll` proxy the game loads on start-up. Reads the two INI files and redirects the game's file reads to enabled mods. The manager installs and updates it from that repo's latest GitHub release; `Resources/ModLoader/EmbeddedLoaders.zip` is the offline fallback. |

Press **Install Loader** in the manager's settings to drop the loader into the game folder as `dinput8.dll`.

## Making a mod

A mod is a folder in `<game>\mods\` with a `mod.ini` and a `Data` folder. The mod's `Data` folder
mirrors the game's own `Data` folder (the same idea as `disk\wars_patch` for Sonic Forces in HMM), so a
replacement for `<game>\Data\Foo.toc` lives at `<mod>\Data\Foo.toc`. Only `Data` is redirected; anything
else in the mod folder (previews, DLLs, configs) is left alone. "New Mod" in the manager creates the `Data` folder for you.

```
mods/
  My Mod/
    mod.ini
    Data/
      ...replacement files...
```

```ini
[Desc]
Title="My Mod"
Version="1.0"
Author="Me"

[Main]
IncludeDirCount=1
IncludeDir0="."
DLLFile=""
```

- Mods at the top of the manager's list win over mods below them.
- Each `IncludeDirN` is a folder that contains its own `Data` folder. Within one mod, `IncludeDir1` overrides `IncludeDir0`, and so on.
- Game data ships as `.toc` + `.m00` packs in `Data`. Repacked packs (e.g. made with
  [SumoTTP](https://github.com/Tyaap/ASRT_SumoTTP)) can be replaced as whole files. Loose files only
  take effect if the game reads loose files at that path.
- `DLLFile` takes a comma-separated list of DLLs (relative to the mod folder). See
  the [loader's README](https://github.com/thezenoverse/SumoModLoader#readme) for the optional `Init`/`PostInit` exports.

Codes (`.hmm`) are not supported for this game yet.

## Building

- Manager: Visual Studio 2022 (or `msbuild`) with the .NET Framework 4.7.2 targeting pack. Open `RutaiModManager.sln`.
- Loader: see the [SumoModLoader](https://github.com/thezenoverse/SumoModLoader) repo. With it checked out next to
  this one, `make install` there refreshes `RutaiModManager/Resources/ModLoader/EmbeddedLoaders.zip`.

## Credits
- [Hedge Mod Manager](https://github.com/thesupersonic16/HedgeModManager) by thesupersonic16 and contributors (MIT)
- [Tyaap](https://github.com/Tyaap) for the S&ASRT data tools and research
