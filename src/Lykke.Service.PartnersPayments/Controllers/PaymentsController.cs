using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PartnersPayments.Client;
using Lykke.Service.PartnersPayments.Client.Enums;
using Lykke.Service.PartnersPayments.Client.Models;
using Lykke.Service.PartnersPayments.Domain.Models;
using Lykke.Service.PartnersPayments.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PartnersPayments.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentsController : ControllerBase, IPartnersPaymentsApi
    {
        private readonly IPaymentsService _paymentsService;
        private readonly IPaymentsStatusUpdater _paymentsStatusUpdater;
        private readonly IMapper _mapper;

        public PaymentsController(
            IPaymentsService paymentsService,
            IPaymentsStatusUpdater paymentsStatusUpdater,
            IMapper mapper)
        {
            _paymentsService = paymentsService;
            _paymentsStatusUpdater = paymentsStatusUpdater;
            _mapper = mapper;
        }

        /// <summary>
        /// Create a payment request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(PaymentRequestResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<PaymentRequestResponseModel> PartnerPaymentAsync([FromBody] PaymentRequestModel request)
        {
            var paymentRequest = _mapper.Map<PaymentRequest>(request);

            var result = await _paymentsService.InitiatePartnerPaymentAsync(paymentRequest);

            return _mapper.Map<PaymentRequestResponseModel>(result);
        }

        /// <summary>
        /// Approve a payment request as a customer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("customer/approval")]
        [ProducesResponseType(typeof(PaymentStatusUpdateResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<PaymentStatusUpdateResponse> CustomerApprovePartnerPaymentAsync([FromBody] CustomerApprovePaymentRequest request)
        {
            var result = await _paymentsStatusUpdater.ApproveByCustomerAsync(request.PaymentRequestId,
                request.SendingAmount,
                request.CustomerId);

            return new PaymentStatusUpdateResponse
            {
                Error = (PaymentStatusUpdateErrorCodes)result
            };
        }

        /// <summary>
        /// Reject a payment request as a customer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("customer/rejection")]
        [ProducesResponseType(typeof(PaymentStatusUpdateResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<PaymentStatusUpdateResponse> CustomerRejectPartnerPaymentAsync([FromBody] CustomerRejectPaymentRequest request)
        {
            var result = await _paymentsStatusUpdater.RejectByCustomerAsync(request.PaymentRequestId,
                request.CustomerId);

            return new PaymentStatusUpdateResponse
            {
                Error = (PaymentStatusUpdateErrorCodes)result
            };
        }

        /// <summary>
        /// Approve a payment request as a receptionist
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("partner/approval")]
        [ProducesResponseType(typeof(PaymentStatusUpdateResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<PaymentStatusUpdateResponse> ReceptionistApprovePaymentAsync([FromBody] ReceptionistProcessPaymentRequest request)
        {
            var result = await _paymentsStatusUpdater.ApproveByReceptionistAsync(request.PaymentRequestId);

            return new PaymentStatusUpdateResponse
            {
                Error = (PaymentStatusUpdateErrorCodes)result
            };
        }

        /// <summary>
        /// Reject a payment request as a receptionist
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("partner/cancellation")]
        [ProducesResponseType(typeof(PaymentStatusUpdateResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<PaymentStatusUpdateResponse> PartnerCancelPaymentAsync([FromBody] ReceptionistProcessPaymentRequest request)
        {
            var result = await _paymentsStatusUpdater.CancelByPartnerAsync(request.PaymentRequestId);

            return new PaymentStatusUpdateResponse
            {
                Error = (PaymentStatusUpdateErrorCodes)result
            };
        }

        /// <summary>
        /// Return details of a payment request
        /// </summary>
        /// <param name="paymentRequestId"></param>
        /// <returns></returns>
        [HttpGet("{paymentRequestId}")]
        [ProducesResponseType(typeof(PaymentDetailsResponseModel), (int)HttpStatusCode.OK)]
        public async Task<PaymentDetailsResponseModel> GetPaymentDetailsAsync(string paymentRequestId)
        {
            var paymentDetails = await _paymentsService.GetPaymentDetailsByPaymentId(paymentRequestId);

            if (paymentDetails == null)
                return null;

            var result = _mapper.Map<PaymentDetailsResponseModel>(paymentDetails);

            return result;
        }

        /// <summary>
        /// Returns pending payments
        /// </summary>
        /// <returns></returns>
        [HttpGet("customer/pending")]
        [ProducesResponseType(typeof(PaginatedPaymentRequestsResponse), (int)HttpStatusCode.OK)]
        public async Task<PaginatedPaymentRequestsResponse> GetPendingPaymentsAsync([FromQuery] PaginatedRequestForCustomer request)
        {
            var result = await _paymentsService.GetPendingPaymentRequestsForCustomerAsync(request.CustomerId,
                request.CurrentPage,
                request.PageSize);

            return _mapper.Map<PaginatedPaymentRequestsResponse>(result);
        }

        /// <summary>
        /// Returns succeeded payments
        /// </summary>
        /// <returns></returns>
        [HttpGet("customer/succeeded")]
        [ProducesResponseType(typeof(PaginatedPaymentRequestsResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<PaginatedPaymentRequestsResponse> GetSucceededPaymentsAsync([FromQuery] PaginatedRequestForCustomer request)
        {
            var result =
                await _paymentsService.GetSucceededPaymentRequestsForCustomerAsync(request.CustomerId,
                    request.CurrentPage,
                    request.PageSize);

            return _mapper.Map<PaginatedPaymentRequestsResponse>(result);
        }

        /// <summary>
        /// Returns failed payments
        /// </summary>
        /// <returns></returns>
        [HttpGet("customer/failed")]
        [ProducesResponseType(typeof(PaginatedPaymentRequestsResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<PaginatedPaymentRequestsResponse> GetFailedPaymentsAsync([FromQuery] PaginatedRequestForCustomer request)
        {
            var result =
                await _paymentsService.GetFailedPaymentRequestsForCustomerAsync(request.CustomerId, request.CurrentPage,
                    request.PageSize);

            return _mapper.Map<PaginatedPaymentRequestsResponse> (result);
        }
    }
}
