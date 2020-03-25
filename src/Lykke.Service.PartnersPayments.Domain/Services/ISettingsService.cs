using System;
using System.Collections.Generic;

namespace Lykke.Service.PartnersPayments.Domain.Services
{
    public interface ISettingsService
    {
        string GetPartnersPaymentsAddress();
        string GetMasterWalletAddress();
        TimeSpan GetRequestsExpirationPeriod();
    }
}
