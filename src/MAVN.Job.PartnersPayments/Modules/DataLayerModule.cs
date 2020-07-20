using Autofac;
using JetBrains.Annotations;
using Lykke.SettingsReader;
using MAVN.Job.PartnersPayments.Settings;
using MAVN.Job.PartnersPayments.Settings.JobSettings;
using MAVN.Persistence.PostgreSQL.Legacy;
using MAVN.Service.PartnersPayments.Domain.Repositories;
using MAVN.Service.PartnersPayments.MsSqlRepositories;
using MAVN.Service.PartnersPayments.MsSqlRepositories.Repositories;

namespace MAVN.Job.PartnersPayments.Modules
{
    [UsedImplicitly]
    public class DataLayerModule : Module
    {
        private readonly DbSettings _settings;

        public DataLayerModule(IReloadingManager<AppSettings> appSettings)
        {
            _settings = appSettings.CurrentValue.PartnersPaymentsJob.Db;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterPostgreSQL(
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
