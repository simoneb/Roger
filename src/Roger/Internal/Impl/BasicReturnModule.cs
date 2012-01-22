using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Roger.Internal.Impl
{
    internal class BasicReturnModule : IPublishModule
    {
        private DefaultBasicReturnHandler basicReturnHandler;

        public void BeforePublishEnabled(IModel publishModel)
        {
            publishModel.BasicReturn += PublishModelOnBasicReturn;
        }

        public void Initialize(IPublishingProcess publishingProcess)
        {
            basicReturnHandler = new DefaultBasicReturnHandler();
        }

        private void PublishModelOnBasicReturn(IModel model, BasicReturnEventArgs args)
        {
            // beware, this is called on the RabbitMQ client connection thread, we should not block
            //log.DebugFormat("Model issued a basic return for message {{we can do better here}} with reply {0} - {1}", args.ReplyCode, args.ReplyText);
            basicReturnHandler.Handle(new BasicReturn(new RogerGuid(args.BasicProperties.MessageId), args.ReplyCode, args.ReplyText));
        }

        public void BeforePublish(IDelivery delivery, IModel publishModel, IBasicProperties properties, Action<BasicReturn> basicReturnCallback)
        {
            // todo: handle this, we don't want to subscribe multiple times in case of republish
            if (basicReturnCallback != null)
                basicReturnHandler.Subscribe(new RogerGuid(properties.MessageId), basicReturnCallback);
        }

        public void AfterPublishDisabled(IModel publishModel)
        {
        }

        public void Dispose()
        {
        }
    }
}