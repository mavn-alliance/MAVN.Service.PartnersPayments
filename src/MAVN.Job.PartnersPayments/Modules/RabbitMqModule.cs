using Autofac;
using JetBrains.Annotations;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.SettingsReader;
using MAVN.Job.PartnersPayments.Settings;
using MAVN.Job.PartnersPayments.Settings.JobSettings;
using MAVN.Service.PartnersPayments.Contract;

namespace MAVN.Job.PartnersPayments.Modules
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
