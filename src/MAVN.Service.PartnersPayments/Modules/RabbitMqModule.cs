using Autofac;
using JetBrains.Annotations;
using Lykke.Job.QuorumTransactionWatcher.Contract;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using MAVN.Service.PartnersPayments.Contract;
using MAVN.Service.PartnersPayments.DomainServices.RabbitMq.Subscribers;
using MAVN.Service.PartnersPayments.Settings;
using Lykke.Service.PrivateBlockchainFacade.Contract.Events;
using Lykke.SettingsReader;

namespace MAVN.Service.PartnersPayments.Modules
{
    [UsedImplicitly]
    public class RabbitMqModule : Module
    {
        private const string TransactionFailedExchange = "lykke.wallet.transactionfailed";
        private const string PartnerPaymentRequestCreatedExchangeName =
            "lykke.wallet.partnerpaymentrequestcreated";
        private const string PartnersPaymentTokensReservedExchangeName =
            "lykke.wallet.partnerspaymenttokensreserved";
        private const string PartnerPaymentProcessedExchangeName =
            "lykke.wallet.partnerspaymentprocessed";
        private const string PartnersPaymentStatusUpdatedExchangeName =
            "lykke.wallet.partnerspaymentstatusupdated";
        private const string DefaultQueueName = "partnerspayments";

        private readonly RabbitMqSettings _settings;

        public RabbitMqModule(IReloadingManager<AppSettings> appSettings)
        {
            _settings = appSettings.CurrentValue.PartnersPaymentsService.RabbitMq;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var rabbitMqConnString = _settings.RabbitMqConnectionString;

            builder.RegisterJsonRabbitPublisher<PartnerPaymentRequestCreatedEvent>(
                rabbitMqConnString,
                PartnerPaymentRequestCreatedExchangeName);

            builder.RegisterJsonRabbitPublisher<PartnersPaymentTokensReservedEvent>(
                rabbitMqConnString,
                PartnersPaymentTokensReservedExchangeName);

            builder.RegisterJsonRabbitPublisher<PartnersPaymentProcessedEvent>(
                rabbitMqConnString,
                PartnerPaymentProcessedExchangeName);

            builder.RegisterJsonRabbitPublisher<PartnersPaymentStatusUpdatedEvent>(
                rabbitMqConnString,
                PartnersPaymentStatusUpdatedExchangeName);

            builder.RegisterJsonRabbitSubscriber<UndecodedSubscriber, UndecodedEvent>(
                rabbitMqConnString,
                Context.GetEndpointName<UndecodedEvent>(),
                DefaultQueueName);

            builder.RegisterJsonRabbitSubscriber<TransactionFailedSubscriber, TransactionFailedEvent>(
                rabbitMqConnString,
                TransactionFailedExchange,
                DefaultQueueName);
        }
    }
}
