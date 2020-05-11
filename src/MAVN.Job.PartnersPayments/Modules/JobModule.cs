using Autofac;
using Common;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Sdk.Health;
using Lykke.SettingsReader;
using MAVN.Job.PartnersPayments.Services;
using MAVN.Job.PartnersPayments.Settings;
using MAVN.Service.CustomerProfile.Client;
using MAVN.Service.EligibilityEngine.Client;
using MAVN.Service.PartnerManagement.Client;
using MAVN.Service.PartnersPayments.Domain.Common;
using MAVN.Service.PartnersPayments.Domain.RabbitMq.Handlers;
using MAVN.Service.PartnersPayments.Domain.Services;
using MAVN.Service.PartnersPayments.DomainServices;
using MAVN.Service.PartnersPayments.DomainServices.Common;
using MAVN.Service.PartnersPayments.DomainServices.RabbitMq.Handlers;
using MAVN.Service.PrivateBlockchainFacade.Client;
using MAVN.Service.WalletManagement.Client;

namespace MAVN.Job.PartnersPayments.Modules
{
    [UsedImplicitly]
    public class JobModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public JobModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterWalletManagementClient(_appSettings.CurrentValue.WalletManagementService, null);
            builder.RegisterCustomerProfileClient(_appSettings.CurrentValue.CustomerProfileService, null);
            builder.RegisterPrivateBlockchainFacadeClient(_appSettings.CurrentValue.PrivateBlockchainFacadeService, null);
            builder.RegisterPartnerManagementClient(_appSettings.CurrentValue.PartnerManagementServiceClient, null);
            builder.RegisterEligibilityEngineClient(_appSettings.CurrentValue.EligibilityEngineServiceClient, null);

            builder.RegisterType<PaymentsService>()
                .As<IPaymentsService>()
                .WithParameter("tokenSymbol",
                    _appSettings.CurrentValue.PartnersPaymentsJob.Constants.TokenSymbol)
                .SingleInstance();

            builder.RegisterType<TransactionScopeHandler>()
                .As<ITransactionScopeHandler>()
                .InstancePerLifetimeScope();

            builder.RegisterType<PaymentsStatusUpdater>()
                .As<IPaymentsStatusUpdater>()
                .WithParameter("tokenSymbol",
                    _appSettings.CurrentValue.PartnersPaymentsJob.Constants.TokenSymbol)
                .SingleInstance();

            builder.RegisterType<BlockchainEncodingService>()
                .As<IBlockchainEncodingService>()
                .SingleInstance();

            builder.RegisterType<BlockchainEventDecoder>()
                .As<IBlockchainEventDecoder>()
                .SingleInstance();

            builder.RegisterType<SettingsService>()
                .As<ISettingsService>()
                .WithParameter("partnersPaymentsAddress",
                    _appSettings.CurrentValue.PartnersPaymentsAddress)
                .WithParameter("requestsExpirationPeriod",
                    _appSettings.CurrentValue.RequestsExpirationPeriod)
                .WithParameter("masterWalletAddress",
                    _appSettings.CurrentValue.MasterWalletAddress)
                .SingleInstance();

            builder.RegisterType<UndecodedEventHandler>()
                .As<IUndecodedEventHandler>()
                .SingleInstance();

            builder.RegisterType<TransactionFailedEventHandler>()
                .As<ITransactionFailedEventHandler>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .SingleInstance();

            builder.RegisterType<ExpiredRequestsManager>()
                .WithParameter("idlePeriod", _appSettings.CurrentValue.PartnersPaymentsJob.IdlePeriod)
                .WithParameter("paymentsExpirationPeriod", _appSettings.CurrentValue.PaymentsExpirationPeriod)
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance();
        }
    }
}
