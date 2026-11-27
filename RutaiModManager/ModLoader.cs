using RutaiModManager.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RutaiModManager
{
    public static class ModLoaders
    {
        // Each loader lives in its own repository and ships as a GitHub release, so it can be
        // updated without a new manager build. Loaders.ini (URL_LOADERS_INI) comes from the same
        // release; the copy in Resources/ModLoader/EmbeddedLoaders.zip is the offline fallback.
        public const string SumoModLoaderRepo = "https://github.com/thezenoverse/SumoModLoader";
        public const string SumoModLoaderDownloadURL = SumoModLoaderRepo + "/releases/latest/download/SumoModLoader.dll";

        /// <summary>
        /// File-redirecting mod loader for Sonic &amp; All-Stars Racing Transformed (32-bit, D3D9).
        /// Installed as a dinput8.dll proxy, which the game loads on start-up.
        /// Source: https://github.com/thezenoverse/SumoModLoader
        /// </summary>
        public static ModLoader SumoModLoader = new ModLoader()
        {
            ModLoaderDownloadURL = SumoModLoaderDownloadURL,
            ModLoaderData = EmbeddedLoaders.SumoModLoader,
            ModLoaderName = "Sumo Engine Mod Loader",
            ModLoaderID = "SumoModLoader",
            ModLoaderFileName = "dinput8.dll",
            DirectXVersion = 9
        };
    }

    public class ModLoader
    {
        public string ModLoaderDownloadURL = string.Empty;
        public byte[] ModLoaderData = null;
        public string ModLoaderName = "None";
        public string ModLoaderID   = null;
        public string ModLoaderFileName = string.Empty;
        public uint DirectXVersion  = uint.MaxValue;
        public string[] IncompatibleFiles = Array.Empty<string>();
        public Func<string, bool> IncompatibleFileCallback = null;

        public static Func<string, bool> IncompatibleByOriginalName(params string[] originalNames)
        {
            return fileName =>
            {
                var info = FileVersionInfo.GetVersionInfo(fileName);
                if (originalNames.Contains(info.OriginalFilename))
                {
                    try
                    {
                        File.Delete(fileName);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                return true;
            };
        }

        public static Func<string, bool> IncompatibleByMD5(params ValueTuple<string, string>[] types)
        {
            return fileName =>
            {
                var name = Path.GetFileName(fileName);
                var data = types.FirstOrDefault(x => x.Item1.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (data != default)
                {
                    var hash = HedgeApp.ComputeMD5Hash(fileName);
                    if (hash.Equals(data.Item2, StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            File.Delete(fileName);
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }

                return true;
            };
        }

        public bool MakeCompatible(string root)
        {
            foreach (string file in IncompatibleFiles)
            {
                var fullPath = Path.Combine(root, file);
                if (!File.Exists(fullPath))
                {
                    continue;
                }

                if (IncompatibleFileCallback != null)
                {
                    if (!IncompatibleFileCallback(fullPath))
                    {
                        return false;
                    }
                }
                else
                {
                    File.Delete(fullPath);
                }
            }
            return true;
        }
    }
}
