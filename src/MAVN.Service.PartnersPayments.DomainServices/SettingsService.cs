using System;
using System.Collections.Generic;
using MAVN.Service.PartnersPayments.Domain.Services;

namespace MAVN.Service.PartnersPayments.DomainServices
{
    public class SettingsService : ISettingsService
    {
        private readonly string _partnersPaymentsAddress;
        private readonly string _masterWalletAddress;
        private readonly TimeSpan _requestsExpirationPeriod;

        public SettingsService(
            string partnersPaymentsAddress,
            string masterWalletAddress,
            TimeSpan requestsExpirationPeriod)
        {
            _partnersPaymentsAddress = partnersPaymentsAddress;
            _masterWalletAddress = masterWalletAddress;
            _requestsExpirationPeriod = requestsExpirationPeriod;
        }

        public string GetPartnersPaymentsAddress()
            => _partnersPaymentsAddress;

        public string GetMasterWalletAddress()
            => _masterWalletAddress;

        public TimeSpan GetRequestsExpirationPeriod()
            => _requestsExpirationPeriod;

    }
}
