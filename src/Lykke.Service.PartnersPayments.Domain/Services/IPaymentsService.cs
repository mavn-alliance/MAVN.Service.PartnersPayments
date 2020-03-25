using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PartnersPayments.Domain.Models;

namespace Lykke.Service.PartnersPayments.Domain.Services
{
    public interface IPaymentsService
    {
        Task<PaymentRequestResult> InitiatePartnerPaymentAsync(IPaymentRequest paymentRequest);

        Task<PaymentModel> GetPaymentDetailsByPaymentId(string paymentRequestId);

        Task<PaginatedPaymentsModel> GetPendingPaymentRequestsForCustomerAsync(string customerId, int currentPage, int pageSize);

        Task<PaginatedPaymentsModel> GetSucceededPaymentRequestsForCustomerAsync(string customerId, int currentPage, int pageSize);

        Task<PaginatedPaymentsModel> GetFailedPaymentRequestsForCustomerAsync(string customerId, int currentPage, int pageSize);

        Task MarkPaymentsAsExpiredAsync(TimeSpan expirationPeriod);

        Task MarkRequestsAsExpiredAsync();
    }
}
