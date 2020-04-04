using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MAVN.Service.PartnersPayments.Client.Models;
using Refit;

namespace MAVN.Service.PartnersPayments.Client
{
    // This is an example of service controller interfaces.
    // Actual interface methods must be placed here (not in IPartnersPaymentsClient interface).

    /// <summary>
    /// PartnersPayments client API interface.
    /// </summary>
    [PublicAPI]
    public interface IPartnersPaymentsApi
    {
        /// <summary>
        /// Creates payment request
        /// </summary>
        /// <param name="request"></param>
        [Post("/api/payments")]
        Task<PaymentRequestResponseModel> PartnerPaymentAsync(PaymentRequestModel request);

        /// <summary>
        /// Return details of a payment request
        /// </summary>
        /// <param name="paymentRequestId"></param>
        /// <returns></returns>
        [Get("/api/payments/{paymentRequestId}")]
        Task<PaymentDetailsResponseModel> GetPaymentDetailsAsync(string paymentRequestId);

        /// <summary>
        /// Approve a payment request as a customer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Post("/api/payments/customer/approval")]
        Task<PaymentStatusUpdateResponse> CustomerApprovePartnerPaymentAsync(CustomerApprovePaymentRequest request);

        /// <summary>
        /// Reject a payment request as a customer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Post("/api/payments/customer/rejection")]
        Task<PaymentStatusUpdateResponse> CustomerRejectPartnerPaymentAsync(CustomerRejectPaymentRequest request);

        /// <summary>
        /// Approve a payment request as a receptionist
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Post("/api/payments/partner/approval")]
        Task<PaymentStatusUpdateResponse> ReceptionistApprovePaymentAsync(ReceptionistProcessPaymentRequest request);

        /// <summary>
        /// Reject a payment request as a receptionist
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Post("/api/payments/partner/cancellation")]
        Task<PaymentStatusUpdateResponse> PartnerCancelPaymentAsync(ReceptionistProcessPaymentRequest request);


        /// <summary>
        /// Return pending payments
        /// </summary>
        /// <returns></returns>
        [Get("/api/payments/customer/pending")]
        Task<PaginatedPaymentRequestsResponse> GetPendingPaymentsAsync(PaginatedRequestForCustomer request);

        /// <summary>
        /// Returns succeeded payments
        /// </summary>
        /// <returns></returns>
        [Get("/api/payments/customer/succeeded")]
        Task<PaginatedPaymentRequestsResponse> GetSucceededPaymentsAsync(PaginatedRequestForCustomer request);

        /// <summary>
        /// Returns failed payments
        /// </summary>
        /// <returns></returns>
        [Get("/api/payments/customer/failed")]
        Task<PaginatedPaymentRequestsResponse> GetFailedPaymentsAsync(PaginatedRequestForCustomer request);
    }
}
