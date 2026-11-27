using RutaiModManager.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using static RutaiModManager.Lang;

namespace RutaiModManager
{
    public static class Games
    {
        public const uint CodeLoaderMinCodeVersionStringId = 101;
        public const uint CodeLoaderMaxCodeVersionStringId = 102;
        public const string EmbeddedCPKREDIRVersion = "0.5.0.8";

        public static Game Unknown = new Game();
        
        public static Game SonicAllStarsRacingTransformed = new Game()
        {
            GameName = "SonicAllStarsRacingTransformed",
            SaveName = string.Empty,
            SupportsCPKREDIR = false,
            SupportsSaveRedirection = false,
            // Template folders created for new mods; mirrors the game's own Data directory
            Folders = ["Data"],
            AppID = "212480",
            GBProtocol = "rutaimmasrt",
            Is64Bit = false,
            ModLoader = ModLoaders.SumoModLoader,
            CodesURL = null,
            SupportsCodeCompilation = false,
            GamePaths = [
                Path.Combine("Sonic & All-Stars Racing Transformed", "ASN_App_PcDx9_Final.exe"),
                Path.Combine("Sonic & All Stars Racing Transformed", "ASN_App_PcDx9_Final.exe")
            ]
        };

        public static IEnumerable<Game> GetSupportedGames()
        {
            yield return SonicAllStarsRacingTransformed;
        }
    }

    internal static class EmbeddedLoaders
    {
        public static byte[] SumoModLoader;

        static EmbeddedLoaders()
        {
            using (var stream = new MemoryStream(Resources.DAT_LOADERS_ZIP))
            using (var zip = new ZipArchive(stream))
            {
                SumoModLoader = GetFile("SumoModLoader.dll");

                byte[] GetFile(string name)
                {
                    var entry = zip.GetEntry(name);
                    using (var file = entry.Open())
                    {
                        var buffer = new byte[entry.Length];
                        file.Read(buffer, 0, buffer.Length);
                        return buffer;
                    }
                }
            }
        }
    }

    public class Game
    {
        public string GameName = "NoGame";
        public string SaveName = string.Empty;
        public string ModsDirectoryName = "mods";
        public ModLoader ModLoader = null;
        public bool SupportsCPKREDIR = false;
        public bool SupportsSaveRedirection = false;
        public string[] Folders = { "raw" };
        public string AppID = "0";
        public string EGSID = null;
        public string GBProtocol;
        public bool Is64Bit = false;
        public string CodesURL;
        public string[] GamePaths = [];
        public uint[] Timestamps = null;
        public bool SupportsCodeCompilation = true;

        public override string ToString() => Localise("Game" + GameName, GameName);
    }

    public class GameInstall
    {
        public static GameInstall Unknown = new GameInstall(Games.Unknown, null, null, GameLauncher.None);

        public Game Game;
        public string GameDirectory;
        public string ExecutablePath;
        public GameLauncher Launcher;
        public bool ShowLauncher = false;
        public bool IsCustom { get; set; } = false;

        public string GameName => GetGameTitle();
        public Uri GameImage { get { return HedgeApp.GetResourceUri($"Resources/Graphics/Games/{Game?.GameName}.png"); } }

        public GameInstall(Game game, string directory, string executablePath, GameLauncher launcher, bool custom = false)
        {
            Game = game;
            if (string.IsNullOrEmpty(directory) && !string.IsNullOrEmpty(executablePath))
                GameDirectory = Path.GetDirectoryName(executablePath);
            else
                GameDirectory = directory;
            ExecutablePath = executablePath;
            Launcher = launcher;
            IsCustom = custom;
        }

        public void StartGame(bool useLauncher = true, string startDirectory = null)
        {
            if (string.IsNullOrEmpty(startDirectory))
                startDirectory = GameDirectory;

            if (useLauncher)
            {
                switch (Launcher)
                {
                    case GameLauncher.Steam:
                        HedgeApp.StartURL($"steam://run/{Game.AppID}", true);
                        break;
                    case GameLauncher.Epic:
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = $"com.epicgames.launcher://apps/{Game.EGSID}?action=launch&silent=true",
                            UseShellExecute = true
                        });
                        break;
                    case GameLauncher.Heroic:
                        HedgeApp.StartURL($"heroic://launch/{Game.EGSID}", true);
                        break;
                    default:
                        Process.Start(new ProcessStartInfo(ExecutablePath)
                        {
                            WorkingDirectory = startDirectory
                        });
                        break;
                }
            }
            else
            {
                Process.Start(new ProcessStartInfo(ExecutablePath)
                {
                    WorkingDirectory = startDirectory
                });
            }
        }

        public static List<GameInstall> SearchForGames(string preference = null)
        {
            var steamGames = Steam.SearchForGames();
            var epicGames = Epic.SearchForGames();
            var games = new List<GameInstall>();

            if (steamGames != null)
                games.AddRange(steamGames);

            if (epicGames != null)
                games.AddRange(epicGames);

            // Search for registry searchable games
            var hives = new Dictionary<string, RegistryKey>()
            {
                { "HKEY_CURRENT_USER", Registry.CurrentUser }
            };
            foreach (var game in Games.GetSupportedGames())
            {
                foreach (string path in game.GamePaths
                    .Where(x => x.StartsWith(":") && x.Contains("\\")))
                {
                    string hiveName = path.Substring(1).Split('\\')[0];
                    string keyPath = path.Substring(hiveName.Length + 2);
                    var hive = hives[hiveName];
                    var key = hive.OpenSubKey(keyPath);
                    if (key == null)
                        continue;

                    string exePath = key.GetValue("ExecutableFilePath") as string;
                    string dirPath = key.GetValue("RootDirectoryPath") as string;
                    if (File.Exists(exePath))
                        games.Add(new GameInstall(game, dirPath, exePath, GameLauncher.None));
                }
            }

            // Extra directories
            if (!string.IsNullOrEmpty(RegistryConfig.CustomGames))
            {
                foreach (string entry in RegistryConfig.CustomGames.Split(';'))
                {
                    string[] split = entry.Split('|');
                    // Should follow the format below, can be a directory at this point for compatibility
                    // e.g. "A:\Games\Sonic Generations\SonicGenerations.exe|SonicGenerations|Steam"
                    string path = split[0];
                    var game = Games.Unknown;
                    var launcher = GameLauncher.None;

                    if (split.Length > 1)
                        game = Games.GetSupportedGames().FirstOrDefault(x => x.GameName == split[1]);

                    if (split.Length > 2)
                        Enum.TryParse(split[2], out launcher);

                    // Guess game for compatibility
                    if (game == Games.Unknown && Directory.Exists(path))
                    {
                        game = Games.GetSupportedGames().FirstOrDefault(x =>
                        {
                            foreach (string gamePath in x.GamePaths)
                            {
                                if (File.Exists(Path.Combine(path, gamePath)))
                                    return true;
                            }
                            return false;
                        });
                        path = Path.Combine(path, game.GamePaths[0]);
                    }

                    if (File.Exists(path))
                        games.Add(new GameInstall(game, null, path, launcher, true));
                }
            }

            HandleGameInstallDuplicates(games);

            return !string.IsNullOrEmpty(preference)
                ? games.OrderBy(x => x.Game.GameName != preference).ToList()
                : games;
        }

        public static void HandleGameInstallDuplicates(List<GameInstall> games)
        {
            // Reset launcher visibility
            games.ForEach(game => game.ShowLauncher = false);

            // Show launcher for duplicates
            foreach (var game in games.GroupBy(t => t.Game).Where(t => t.Count() > 1).Select(t => t.Key))
            {
                foreach (var install in games.Where(t => t.Game == game))
                {
                    install.ShowLauncher = true;
                }
            }

            // Remove duplicates with same path
            foreach (var game in games.GroupBy(t => t.ExecutablePath).Where(t => t.Count() > 1).Select(t => t.Key))
            {
                var installs = games.Where(t => t.ExecutablePath == game).ToList();
                for (int i = installs.Count - 1; i > 0; i--)
                    games.Remove(installs[i]);
            }
        }

        public string GetGameTitle()
        {
            string title = Localise("Game" + Game.GameName, Game.GameName);

            if (ShowLauncher)
            {
                if (Launcher == GameLauncher.None)
                    title += $" ({Path.GetFileNameWithoutExtension(ExecutablePath)})";
                else
                    title += $" ({Localise("Launcher" + Launcher)})";
            }

            return title;
        }

        public override string ToString() => Localise("Game" + Game.GameName, Game.GameName);
    }

    public enum GameLauncher
    {
        None,
        Steam,
        Epic,
        Heroic
    }

    public class CodeLoaderInfo
    {
        public Version LoaderVersion { get; set; }
        public Version MinCodeVersion { get; set; }
        public Version MaxCodeVersion { get; set; }

        public CodeLoaderInfo(Version loader)
        {
            LoaderVersion = loader;
        }

        public CodeLoaderInfo(Version loader, Version minCode) : this(loader)
        {
            MinCodeVersion = minCode;
        }

        public CodeLoaderInfo(Version loader, Version minCode, Version maxCode) : this(loader, minCode)
        {
            MaxCodeVersion = maxCode;
        }
    }
}
