using Autofac;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.EligibilityEngine.Client;
using Lykke.Service.PartnerManagement.Client;
using MAVN.Service.PartnersPayments.Domain.Common;
using MAVN.Service.PartnersPayments.Domain.RabbitMq.Handlers;
using MAVN.Service.PartnersPayments.Domain.Services;
using MAVN.Service.PartnersPayments.DomainServices;
using MAVN.Service.PartnersPayments.DomainServices.Common;
using MAVN.Service.PartnersPayments.DomainServices.RabbitMq.Handlers;
using MAVN.Service.PartnersPayments.Services;
using MAVN.Service.PartnersPayments.Settings;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.WalletManagement.Client;
using Lykke.SettingsReader;

namespace MAVN.Service.PartnersPayments.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
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

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .SingleInstance();

            builder.RegisterType<PaymentsService>()
                .As<IPaymentsService>()
                .WithParameter("tokenSymbol",
                    _appSettings.CurrentValue.PartnersPaymentsService.Constants.TokenSymbol)
                .SingleInstance();

            builder.RegisterType<TransactionScopeHandler>()
                .As<ITransactionScopeHandler>()
                .InstancePerLifetimeScope();

            builder.RegisterType<PaymentsStatusUpdater>()
                .As<IPaymentsStatusUpdater>()
                .WithParameter("tokenSymbol",
                    _appSettings.CurrentValue.PartnersPaymentsService.Constants.TokenSymbol)
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
        }
    }
}
