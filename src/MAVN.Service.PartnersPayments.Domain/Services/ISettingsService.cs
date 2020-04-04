using System;
using System.Collections.Generic;

namespace MAVN.Service.PartnersPayments.Domain.Services
{
    public interface ISettingsService
    {
        string GetPartnersPaymentsAddress();
        string GetMasterWalletAddress();
        TimeSpan GetRequestsExpirationPeriod();
    }
}
