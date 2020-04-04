using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using MAVN.Service.PartnersPayments.Domain.RabbitMq.Handlers;
using Lykke.Service.PrivateBlockchainFacade.Contract.Events;

namespace MAVN.Service.PartnersPayments.DomainServices.RabbitMq.Subscribers
{
    public class TransactionFailedSubscriber : JsonRabbitSubscriber<TransactionFailedEvent>
    {
        private readonly ITransactionFailedEventHandler _handler;
        private readonly ILog _log;

        public TransactionFailedSubscriber(
            ITransactionFailedEventHandler handler,
            string connectionString,
            string exchangeName,
            string queueName,
            ILogFactory logFactory)
            : base(connectionString, exchangeName, queueName, logFactory)
        {
            _handler = handler;
            _log = logFactory.CreateLog(this);
        }


        protected override async Task ProcessMessageAsync(TransactionFailedEvent message)
        {
            await _handler.HandleAsync(message.OperationId);

            _log.Info("Handled transaction failed event", message);
        }
    }
}
