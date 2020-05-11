using System;
using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using MAVN.Service.CustomerProfile.Client;
using MAVN.Service.EligibilityEngine.Client;
using MAVN.Service.PartnerManagement.Client;
using MAVN.Service.PrivateBlockchainFacade.Client;
using MAVN.Service.WalletManagement.Client;

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
