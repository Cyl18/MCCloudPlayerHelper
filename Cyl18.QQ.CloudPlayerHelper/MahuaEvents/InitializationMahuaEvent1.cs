using Newbe.Mahua.MahuaEvents;
using System;
using System.Timers;
using Newbe.Mahua;

namespace Cyl18.QQ.CloudPlayerHelper.MahuaEvents
{
    /// <summary>
    /// 插件初始化事件
    /// </summary>
    public class InitializationMahuaEvent1
        : IInitializationMahuaEvent
    {
        private readonly IMahuaApi _mahuaApi;
        private Timer timer;

        public InitializationMahuaEvent1(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
            timer = new Timer(15000);
        }

        public void Initialized(InitializedContext context)
        {
            timer.Elapsed += (sender, args) => { ServerMonitor.Tick(); };
            timer.Start();
        }
    }
}
