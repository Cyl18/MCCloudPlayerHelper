using Newbe.Mahua.MahuaEvents;
using System;
using System.Diagnostics;
using System.Linq;
using GammaLibrary.Extensions;
using GoodTimeStudio.ServerPinger;
using Newbe.Mahua;
using TextCommandCore;

namespace Cyl18.QQ.CloudPlayerHelper.MahuaEvents
{
    /// <summary>
    /// 群消息接收事件
    /// </summary>
    public class GroupMessageReceivedMahuaEvent1
        : IGroupMessageReceivedMahuaEvent
    {
        private readonly IMahuaApi _mahuaApi;

        public GroupMessageReceivedMahuaEvent1(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        public void ProcessGroupMessage(GroupMessageReceivedContext context)
        {
            var qq = context.FromQq;
            var group = context.FromGroup;
            var msg = context.Message;

            var handler = new GroupMessageProcessor(qq, @group);
            handler.ProcessCommandInput(group, msg);
        }
    }

    public class GroupMessageProcessor : ICommandHandlerCollection<GroupMessageProcessor>, ISender
    {
        public Action<TargetID, Message> MessageSender { get; } = (id, message) => id.SendGroup(message);

        public Action<Message> ErrorMessageSender { get; } =
            message => Config.Instance.ErrorMessageReceiverQQ.SendPrivate(message);

        public string Sender { get; }
        public string Group { get; }

        public GroupMessageProcessor(string sender, string group)
        {
            Sender = sender;
            Group = @group;
        }
        [Matchers("删除服务器配置", "Delete-ServerInfo")]
        [RequireServerExists, SaveConfig]
        string DeleteAllGroupConfig(string name)
        {
            Config.Instance.ServerInfos.GetGroup(Group).RemoveWhere(info => info.ServerName == name);
            return "好嘞.";
        }
        [Matchers("添加服务器配置", "Add-ServerInfo"), SaveConfig]
        string AddAllGroupConfig(string name, string url)
        {
            if (ServerPinger.GetStatus(url) == null) return "无法在这个时候访问这个服务器. 请检查你的参数是否正确, 如果你非要添加这个服务器不可, 请使用命令 [强行添加服务器配置].";
            return AddAllGroupConfigForce(name, url);
        }

        [Matchers("强行添加服务器配置", "Add-ServerInfo-Force"), SaveConfig]
        string AddAllGroupConfigForce(string name, string url)
        {
            var info = new ServerInfo { ServerName = name, ServerUrl = url };
            var list = Config.Instance.ServerInfos.GetGroup(Group);
            if (list.Contains(info))
            {
                return "你要添加的服务器配置似乎已经存在了呢.";
            }
            else
            {
                Config.Instance.ServerInfos.GetGroup(Group).Add(info);
                return "搞定.";
            }
        }

        [Matchers("开启监视服务器", "监视服务器", "Enable-MonitorServer")]
        [RequireServerExists, SaveConfig]
        string EnableMonitorServer(string name)
        {
            Config.Instance.GetServerInfo(Group, name).Monitor = true;
            return "好咯.";
        }

        [Matchers("开启监视服务器玩家", "监视服务器玩家", "Enable-MonitorServerPlayer")]
        [RequireServerExists, SaveConfig]
        string EnableMonitorServerPlayer(string name)
        {
            Config.Instance.GetServerInfo(Group, name).MonitorPlayer = true;
            return "好滴.";
        }

        [Matchers("关闭监视服务器", "Disable-MonitorServer")]
        [RequireServerExists, SaveConfig]
        string DisableMonitorServer(string name)
        {
            Config.Instance.GetServerInfo(Group, name).Monitor = false;
            return "好咯.";
        }

        [Matchers("关闭监视服务器玩家", "Disable-MonitorServerPlayer")]
        [RequireServerExists, SaveConfig]
        string DisableMonitorServerPlayer(string name)
        {
            Config.Instance.GetServerInfo(Group, name).MonitorPlayer = false;
            return "好滴.";
        }

        [Matchers("加载设置", "Load-Config")]
        string LoadConfig()
        {
            Config.Update();
            return "应该是完事了.";
        }

        [Matchers("服务器信息", "Get-ServerStatus", "serverstat", "serverinfo", "info")]
        string ServerStatus(string name = null)
        {
            if (name == null)
                return $"请指定服务器的名称. 当前存在的服务器配置有 [{Config.Instance.ServerInfos.GetMixed(Group).Select(i => i.ServerName).Connect()}] 你可以使用 [添加服务器配置 名字 IP].";

            var info = Config.Instance.GetServerInfo(Group, name);
            if (info == null)
                return $"没有找到这样的服务器. 当前存在的服务器配置有 [{Config.Instance.ServerInfos.GetMixed(Group).Select(i => i.ServerName).Connect()}] 你可以使用 [添加服务器配置 名字 IP].";

            try
            {
                var stat = ServerPinger.GetStatus(info.ServerUrl).Result;
                if (stat == null) return "服务器不在线.";

                return $"这个服务器在线! \r\n" +
                       $"服务器版本: {stat.version.name}\r\n" +
                       $"玩家: {stat.players.online}/{stat.players.max}" +
                       (stat.modinfo == null ? "" : $"\r\n共有 {stat.modinfo.modlist.Count} 个 Models.");
            }
            catch (Exception e)
            {
                ErrorMessageSender(e.ToString());
                return "在获取服务器信息时出了差错. 这好像是因为服务器不在线, 要不然就是服务器地址敲错了.";
            }
        }

        [Matchers("服务器玩家", "Get-ServerPlayers", "playerinfo")]
        string ServerPlayers(string name = null)
        {
            if (name == null)
                return $"请指定服务器的名称. 当前存在的服务器配置有 [{Config.Instance.ServerInfos.GetMixed(Group).Select(i => i.ServerName).Connect()}] 你可以使用 [添加服务器配置 名字 IP].";

            var info = Config.Instance.GetServerInfo(Group, name);
            if (info == null)
                return $"没有找到这样的服务器. 当前存在的服务器配置有 [{Config.Instance.ServerInfos.GetMixed(Group).Select(i => i.ServerName).Connect()}] 你可以使用 [添加服务器配置 名字 IP].";

            try
            {
                var stat = ServerPinger.GetStatus(info.ServerUrl).Result;
                if (stat == null) return "在获取服务器信息时出了差错. 这好像是因为服务器不在线, 要不然就是服务器地址敲错了.";

                return $"这个服务器在线! \r\n" +
                       $"服务器版本: {stat.version.name}\r\n" +
                       $"玩家{stat.players.online}/{stat.players.max}: {stat.players.sample?.Select(p => p.name).Connect()}" +
                       (stat.modinfo == null ? "" : $"\r\n共有 {stat.modinfo.modlist.Count} 个 Models.");
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return "在获取服务器信息时出了差错. 这好像是因为服务器不在线, 要不然就是服务器地址敲错了.";
            }
        }

        [Matchers("全部服务器信息", "allstat", "infos", "servers", "ping", "信息")]
        string AllServerStatus()
        {
            return
                Config.Instance.ServerInfos.GetMixed(Group).AsParallel()
                    .Select(info => $"{info.ServerName}: {ServerPlayers(info.ServerName)}").ToArray().Connect("\r\n\r\n");
        }

        [Matchers("设置服务器爆炸信息")]
        [RequireServerExists, SaveConfig]
        string SetServerBoomMessage(string serverName, string message)
        {
            var info = Config.Instance.GetServerInfo(Group, serverName);
            info.BoomMessage = message;
            return "完了.";
        }

    }
}
