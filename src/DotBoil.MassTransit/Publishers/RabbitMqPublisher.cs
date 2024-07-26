using DotBoil.MassTransit.Attributes;
using MassTransit;

namespace DotBoil.MassTransit.Publishers
{
    internal class RabbitMqPublisher : IBusPublisher
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public RabbitMqPublisher(ISendEndpointProvider sendEndpointProvider)
        {
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task Publish<T>(T message) where T : MessageBroker.IEvent
        {
            var messageType = message.GetType();
            var queueAttributes = messageType.GetCustomAttributes(typeof(QueueAttribute), true) as QueueAttribute[];

            if (!queueAttributes.Any())
                return;

            foreach (var queueAttribute in queueAttributes)
            {
                var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{queueAttribute.Name}"));

                await endpoint.Send(message, sendContext =>
                {
                    sendContext.MessageId = message.Id;
                });
            }
        }
    }
}
