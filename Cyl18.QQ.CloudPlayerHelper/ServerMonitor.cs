using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using GoodTimeStudio.ServerPinger;

namespace Cyl18.QQ.CloudPlayerHelper
{
    public static class ServerMonitor
    {


        public static void Tick()
        {
            var serverInfos = Config.Instance.ServerInfos.SelectMany(set => set.Value, (pair, info) => new { group = pair.Key, info })
                .Where(info => (info.info.MonitorPlayer || info.info.Monitor) && info.group != "AllGroup").ToArray();
            foreach (var serverInfo in serverInfos.Where(info => !info.info.Inited).Select(info => info.info))
                Init(serverInfo);

            foreach (var serverInfo in serverInfos)
            {
                var info = serverInfo.info;
                var lastPlayers = info.LastPlayers;
                var group = serverInfo.group;
                var stat = ServerPinger.GetStatus(info.ServerUrl).Result;
                var serverUp = stat != null;
                if (serverUp != info.LastTimeOnline && info.Monitor)
                {
                    group.SendGroup(serverUp ? $"{info.ServerName} 服务器上线啦!" : $"{info.ServerName} 服务器: {info.BoomMessage}");
                    info.LastTimeOnline = serverUp;
                }

                if (stat?.players?.sample != null && info.MonitorPlayer)
                {
                    var currentPlayers = new HashSet<string>(stat.players.sample.Select(p => p.name));
                    if (!currentPlayers.SetEquals(lastPlayers))
                    {
                        var newPlayers = currentPlayers.Except(lastPlayers).ToArray();
                        var guedPlayers = lastPlayers.Except(currentPlayers).ToArray();
                        if (newPlayers.Any())
                        {
                            group.SendGroup($"{info.ServerName}: {newPlayers.Connect()} 进入了服务器.");
                        }

                        if (guedPlayers.Any())
                        {
                            group.SendGroup($"{info.ServerName}: {guedPlayers.Connect()} 摸了.");
                        }
                    }

                    info.LastPlayers = currentPlayers;
                }
                
            }

        }

        private static void Init(ServerInfo serverInfo)
        {
            var stat = ServerPinger.GetStatus(serverInfo.ServerUrl).Result;
            serverInfo.LastTimeOnline = stat != null;
            serverInfo.LastPlayers = stat?.players?.sample == null ? new HashSet<string>() : new HashSet<string>(stat.players.sample.Select(info => info.name));
            serverInfo.Inited = true;
        }
    }
}
