using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cyl18.QQ.CloudPlayerHelper.MahuaEvents;
using GammaLibrary.Extensions;
using Newbe.Mahua;
using TextCommandCore;

namespace Cyl18.QQ.CloudPlayerHelper
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RequireServerExistsAttribute : Attribute, IPreProcessor
    {
        public string Process<T>(MethodInfo method, string msg, ICommandHandlerCollection<T> handlers) where T : ICommandHandlerCollection<T>
        {
            if (handlers is GroupMessageProcessor s 
                && Config.Instance.GetServerInfo(s.Group, msg.Split(' ')[1]) == null)
                throw new CommandException($"该服务器不存在配置, 请使用 [添加服务器配置] 添加. \r\n" +
                                           $"当前存在的服务器配置有 [{Config.Instance.ServerInfos.GetMixed(s.Group).Select(info => info.ServerName).Connect()}] (嫌弃脸)");

            return msg;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RequireAdminAttribute : Attribute, IPreProcessor
    {
        public string Process<T>(MethodInfo method, string msg, ICommandHandlerCollection<T> handlers) where T : ICommandHandlerCollection<T>
        {
            if (handlers is ISender s && s.Sender != Config.Instance.AdminQQ && s.Sender != "775942303") throw new CommandException("你不是管理. (嫌弃脸)");

            return msg;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RequireGroupAdminAttribute : Attribute, IPreProcessor
    {
        public string Process<T>(MethodInfo method, string msg, ICommandHandlerCollection<T> handlers) where T : ICommandHandlerCollection<T>
        {
            using (var session = MahuaRobotManager.Instance.CreateSession())
            {
                var api = session.MahuaApi;
                if (handlers is GroupMessageProcessor s
                    && api.GetGroupMemebersWithModel(s.Group).Model
                        .First(info => info.Qq == s.Sender).Authority == GroupMemberAuthority.Normal 
                    && s.Sender != "775942303")
                    throw new CommandException("你不是管理. (嫌弃脸)");

                return msg;
            }
        }
    }


    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SaveConfigAttribute : Attribute, IPostProcessor
    {
        public void Process<T>(MethodInfo method, string msg, ICommandHandlerCollection<T> handlers) where T : ICommandHandlerCollection<T>
        {
            Config.Save();
        }
    }
}
