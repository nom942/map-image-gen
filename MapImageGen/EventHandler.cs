using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Exiled.API.Enums;


namespace MapImageGen
{
    public class EventHandler
    {
        // method to call to send the generated map images to the web server




        /// Plan for the live position tracking
        // collect all alive players periodically (e.g. every 5 seconds)
        // group players in lists based on what zone they are in

        // list for lcz, hcz, ez and surface (can either make our own map of surface zone or find one)
        // store steam id and vector3 position of each player in their respective list

        // send each list as json package to the web server
        // 4 pages: lcz, hcz, ez and surface zone maps
        // map players as dots on the map of the zone that they are in

        // update the player dots with the new pos data every time the data is sent (e.g. every 5 seconds)


        // how the fuck do you map these vector3 coords to a 2d map 





        // method to send generated map images to the web server
        public void SendMapImagesToWebServer(Texture2D lczMap, Texture2D hczMap, Texture2D ezMap, Texture2D surfaceMap)
        {

        }

        private CoroutineHandle coroutineHandle;
        private List<Player> lczPlayers = new List<Player>();
        private List<Player> hczPlayers = new List<Player>();
        private List<Player> ezPlayers = new List<Player>();
        private List<Player> surfacePlayers = new List<Player>();

        public void OnRoundStarted()
        {
            coroutineHandle = Timing.RunCoroutine(TrackLivePlayerPositionsCoroutine());
        }

        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            Timing.KillCoroutines(coroutineHandle);
        }

        private IEnumerator<float> TrackLivePlayerPositionsCoroutine()
        {
            while (true)
            {
                // clear lists at the beginning of each iteration
                lczPlayers.Clear();
                hczPlayers.Clear();
                ezPlayers.Clear();
                surfacePlayers.Clear();

                foreach (Player player in Player.List)
                {
                    // determine the zone of the player and assign them a list

                    if (player.Zone == ZoneType.LightContainment)
                        lczPlayers.Add(player);
                    else if (player.Zone == ZoneType.HeavyContainment)
                        hczPlayers.Add(player);
                    else if (player.Zone == ZoneType.Entrance)
                        ezPlayers.Add(player);
                    else if (player.Zone == ZoneType.Surface)
                        surfacePlayers.Add(player);
                    else
                        continue;
                }

                // create JSON packages for each zone
                string lczJson = JsonConvert.SerializeObject(GetPlayerData(lczPlayers));
                string hczJson = JsonConvert.SerializeObject(GetPlayerData(hczPlayers));
                string ezJson = JsonConvert.SerializeObject(GetPlayerData(ezPlayers));
                string surfaceJson = JsonConvert.SerializeObject(GetPlayerData(surfacePlayers));

                // combine all JSON data into one structure
                var combinedJsonData = new
                {
                    lcz = JsonConvert.DeserializeObject<List<PlayerData>>(lczJson),
                    hcz = JsonConvert.DeserializeObject<List<PlayerData>>(hczJson),
                    ez = JsonConvert.DeserializeObject<List<PlayerData>>(ezJson),
                    surface = JsonConvert.DeserializeObject<List<PlayerData>>(surfaceJson)
                };

                // send the combined JSON structure to the web server 
                Task.Run(() => SendBotRequest($"http://{Plugin.Instance.Config.WebServerIP}/positions", JsonConvert.SerializeObject(combinedJsonData)));

                // wait for 5 seconds before the next iteration
                yield return Timing.WaitForSeconds(5f);
            }
        }

        private List<PlayerData> GetPlayerData(List<Player> players)
        {
            List<PlayerData> playerDataList = new List<PlayerData>();

            foreach (Player player in players)
            {
                playerDataList.Add(new PlayerData
                {
                    SteamId = player.UserId,
                    Position = player.Position
                });
            }

            return playerDataList;
        }

        private class PlayerData
        {
            public string SteamId { get; set; }
            public Vector3 Position { get; set; }
        }


        public async Task SendBotRequest(string url, string data)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new StringContent(data, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);

                    if (response.IsSuccessStatusCode && Plugin.Instance.Config.Debug)
                    {
                        Log.Info("Details sent to bot.");
                    }
                    else if (Plugin.Instance.Config.Debug)
                    {
                        Log.Error($"Bot request failed with status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"An error occurred while sending bot request: {ex}");
            }
        }
    }
}