using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MAVN.Service.PartnersPayments.Domain.Models;

namespace MAVN.Service.PartnersPayments.MsSqlRepositories.Entities
{
    [Table("payment_request_blockchain_data")]
    public class PaymentRequestBlockchainEntity : IPaymentRequestBlockchainData
    {
        [Key, Required]
        [Column("payment_request_id")]
        public string PaymentRequestId { get; set; }

        /// <summary>
        /// Id of the last BC operation which was requested for this payment ID
        /// </summary>
        [Required]
        [Column("last_operation_id")]
        public string LastOperationId { get; set; }

        public static PaymentRequestBlockchainEntity Create(string paymentRequestId, string lastOperationId)
        {
            return new PaymentRequestBlockchainEntity
            {
                PaymentRequestId = paymentRequestId,
                LastOperationId = lastOperationId
            };
        }

    }
}
