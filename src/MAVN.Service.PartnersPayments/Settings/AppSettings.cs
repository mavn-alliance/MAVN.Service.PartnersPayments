using System;
using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.CurrencyConvertor.Client;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.EligibilityEngine.Client;
using Lykke.Service.PartnerManagement.Client;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.WalletManagement.Client;

namespace MAVN.Service.PartnersPayments.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public PartnersPaymentsSettings PartnersPaymentsService { get; set; }

        public PrivateBlockchainFacadeServiceClientSettings PrivateBlockchainFacadeService { get; set; }

        public CustomerProfileServiceClientSettings CustomerProfileService { get; set; }

        public WalletManagementServiceClientSettings WalletManagementService { get; set; }

        public PartnerManagementServiceClientSettings PartnerManagementServiceClient { get; set; }

        public EligibilityEngineServiceClientSettings EligibilityEngineServiceClient { get; set; }

        public string PartnersPaymentsAddress { get; set; }

        public string MasterWalletAddress { get; set; }

        public TimeSpan PaymentsExpirationPeriod { get; set; }

        public TimeSpan RequestsExpirationPeriod { get; set; }
    }
}
