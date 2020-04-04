using Autofac;
using JetBrains.Annotations;
using Lykke.Common.MsSql;
using MAVN.Service.PartnersPayments.Domain.Repositories;
using MAVN.Service.PartnersPayments.MsSqlRepositories;
using MAVN.Service.PartnersPayments.MsSqlRepositories.Repositories;
using MAVN.Service.PartnersPayments.Settings;
using Lykke.SettingsReader;

namespace MAVN.Service.PartnersPayments.Modules
{
    [UsedImplicitly]
    public class DataLayerModule : Module
    {
        private readonly DbSettings _settings;

        public DataLayerModule(IReloadingManager<AppSettings> appSettings)
        {
            _settings = appSettings.CurrentValue.PartnersPaymentsService.Db;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMsSql(
                _settings.DataConnString,
                connString => new PartnersPaymentsContext(connString, false),
                dbConn => new PartnersPaymentsContext(dbConn));

            builder.RegisterType<PaymentsRepository>()
                .As<IPaymentsRepository>()
                .SingleInstance();

            builder.RegisterType<PaymentRequestBlockchainRepository>()
                .As<IPaymentRequestBlockchainRepository>()
                .SingleInstance();
        }
    }
}
