using Newbe.Mahua.MahuaEvents;
using System;
using Newbe.Mahua;
using TextCommandCore;

namespace Cyl18.QQ.CloudPlayerHelper.MahuaEvents
{
    /// <summary>
    /// 私聊消息接收事件
    /// </summary>
    public class PrivateMessageReceivedMahuaEvent1
        : IPrivateMessageReceivedMahuaEvent
    {
        private readonly IMahuaApi _mahuaApi;

        public PrivateMessageReceivedMahuaEvent1(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        public void ProcessPrivateMessage(PrivateMessageReceivedContext context)
        {
            var qq = context.FromQq;
            var msg = context.Message;

            var handler = new PrivateMessageProcessor(qq);
            handler.ProcessCommandInput(qq, msg);
        }
    }

    public class PrivateMessageProcessor : ICommandHandlerCollection<PrivateMessageProcessor>, ISender
    {
        public Action<TargetID, Message> MessageSender { get; } = (id, message) => id.SendPrivate(message);

        public Action<Message> ErrorMessageSender { get; } =
            message => Config.Instance.ErrorMessageReceiverQQ.SendPrivate(message);

        public string Sender { get; }

        public PrivateMessageProcessor(string sender)
        {
            Sender = sender;
        }
    }
}

