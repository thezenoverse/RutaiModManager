using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RutaiModManager.Misc;
using Newtonsoft.Json;

namespace RutaiModManager
{
    public class NetworkConfig
    {
        public DateTime LastUpdated { get; set; }

        public List<string> URLBlockList { get; set; } = new List<string>();

        public bool IsServerBlocked(string url)
        {
            if (URLBlockList == null || URLBlockList.Count == 0)
                return false;

            return URLBlockList.Any(u => url.ToLowerInvariant().Contains(u));
        }

        public static async Task<NetworkConfig> LoadConfig(string updateURL, bool force = false)
        {
            try
            {
                NetworkConfig config = null;
                string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "RutaiModManager/RMMNetworkConfig.json");
                bool update = force || !File.Exists(configPath);
                if (!update && File.Exists(configPath))
                {
                    config = JsonConvert.DeserializeObject<NetworkConfig>(File.ReadAllText(configPath));
                    if (config == null || DateTime.Now > config.LastUpdated.AddDays(7))
                        update = true;
                }

                if (update)
                {
                    config = await Singleton.GetInstance<HttpClient>().GetAsJsonAsync<NetworkConfig>(updateURL);
                    if (config == null)
                        return new NetworkConfig();

                    config.LastUpdated = DateTime.Now;
                    string dir = Path.GetDirectoryName(configPath) ?? "RutaiModManager";
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    File.WriteAllText(configPath, JsonConvert.SerializeObject(config));
                }

                return config ?? new NetworkConfig();
            }
            catch
            {
                return new NetworkConfig();
            }
        }
    }
}
