using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary;
using Newtonsoft.Json;

namespace Cyl18.QQ.CloudPlayerHelper
{
    [Configuration("CPHConfig")]
    public class Config : Configuration<Config>
    {
        public string ErrorMessageReceiverQQ { get; set; } = "775942303";
        public string AdminQQ { get; set; } = "775942303";
        public GroupListDictionary<ServerInfo> ServerInfos { get; set; } = new GroupListDictionary<ServerInfo>();

        public ServerInfo GetServerInfo(string group, string name)
        {
            return ServerInfos.GetMixed(@group).FirstOrDefault(info => info.ServerName == name);
        }
    }

    public abstract class MCInfo : IEquatable<MCInfo>
    {
        public string ServerName { get; set; }
        public string ServerUrl { get; set; }

        public bool Equals(MCInfo other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(ServerName, other.ServerName) && string.Equals(ServerUrl, other.ServerUrl);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((MCInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ServerName != null ? ServerName.GetHashCode() : 0) * 397) ^ (ServerUrl != null ? ServerUrl.GetHashCode() : 0);
            }
        }

        public static bool operator ==(MCInfo left, MCInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MCInfo left, MCInfo right)
        {
            return !Equals(left, right);
        }
    }

    public class ServerInfo : MCInfo
    {
        public bool Monitor { get; set; }
        public bool MonitorPlayer { get; set; }
        public string BoomMessage { get; set; } = "服务器爆炸了!";

        [JsonIgnore]
        public HashSet<string> LastPlayers { get; set; } = new HashSet<string>();
        [JsonIgnore]
        public bool LastTimeOnline { get; set; }
        [JsonIgnore]
        public bool Inited { get; set; }
    }
}
