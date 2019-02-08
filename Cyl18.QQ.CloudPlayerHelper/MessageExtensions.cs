using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newbe.Mahua;
using TextCommandCore;

namespace Cyl18.QQ.CloudPlayerHelper
{
    public static class MessageExtensions
    {
        public static void SendPrivate(this string qq, string message)
        {
            using (var session = MahuaRobotManager.Instance.CreateSession())
            {
                var api = session.MahuaApi;
                api.SendPrivateMessage(qq, message);
            }
        }

        public static void SendPrivate(this TargetID qq, string message)
        {
            using (var session = MahuaRobotManager.Instance.CreateSession())
            {
                var api = session.MahuaApi;
                api.SendPrivateMessage(qq, message);
            }
        }

        public static void SendGroup(this string qq, string message)
        {
            using (var session = MahuaRobotManager.Instance.CreateSession())
            {
                var api = session.MahuaApi;
                api.SendGroupMessage(qq, message);
            }
        }

        public static void SendGroup(this TargetID qq, string message)
        {
            using (var session = MahuaRobotManager.Instance.CreateSession())
            {
                var api = session.MahuaApi;
                api.SendGroupMessage(qq, message);
            }
        }
    }
}
