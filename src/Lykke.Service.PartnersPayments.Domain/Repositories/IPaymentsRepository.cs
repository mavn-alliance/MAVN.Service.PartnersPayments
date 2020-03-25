using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PartnersPayments.Domain.Enums;
using Lykke.Service.PartnersPayments.Domain.Models;

namespace Lykke.Service.PartnersPayments.Domain.Repositories
{
    public interface IPaymentsRepository
    {
        Task AddAsync(IPaymentRequest paymentRequest);
        Task<PaymentModel> GetByPaymentRequestIdAsync(string paymentRequestId);
        Task SetStatusAsync(string paymentRequestId, PaymentRequestStatus status);
        Task UpdatePaymentAsync(PaymentModel paymentModel);
        Task<(IEnumerable<PaymentModel>, int)> GetPendingPaymentRequestsForCustomerAsync(string customerId, int skip, int take);
        Task<(IEnumerable<PaymentModel>, int)> GetSucceededPaymentRequestsForCustomerAsync(string customerId, int skip, int take);
        Task<(IEnumerable<PaymentModel>, int)> GetFailedPaymentRequestsForCustomerAsync(string customerId, int skip, int take);
        Task<string[]> GetExpiredRequestsAsync();
        Task<string[]> GetExpiredPaymentsAsync(DateTime expirationDate);
    }
}
