using Autofac;
using JetBrains.Annotations;
using Lykke.Job.PartnersPayments.Settings;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.PartnersPayments.Contract;

using Lykke.SettingsReader;

namespace Lykke.Job.PartnersPayments.Modules
{
    [UsedImplicitly]
    public class RabbitMqModule : Module
    {
        private const string PartnerPaymentRequestCreatedExchangeName =
            "lykke.wallet.partnerpaymentrequestcreated";

        private const string PartnersPaymentStatusUpdatedExchangeName =
            "lykke.wallet.partnerspaymentstatusupdated";

        private readonly RabbitMqSettings _settings;

        public RabbitMqModule(IReloadingManager<AppSettings> appSettings)
        {
            _settings = appSettings.CurrentValue.PartnersPaymentsJob.RabbitMq;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var rabbitMqConnString = _settings.RabbitMqConnectionString;

            builder.RegisterJsonRabbitPublisher<PartnerPaymentRequestCreatedEvent>(
                rabbitMqConnString,
                PartnerPaymentRequestCreatedExchangeName);

            builder.RegisterJsonRabbitPublisher<PartnersPaymentStatusUpdatedEvent>(
                rabbitMqConnString,
                PartnersPaymentStatusUpdatedExchangeName);
        }
    }
}
