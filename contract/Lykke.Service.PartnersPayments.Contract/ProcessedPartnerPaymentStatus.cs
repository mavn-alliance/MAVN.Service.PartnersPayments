using JetBrains.Annotations;

namespace Lykke.Service.PartnersPayments.Contract
{
    [PublicAPI]
    public enum ProcessedPartnerPaymentStatus
    {
        /// <summary>
        /// The payment transfer was accepted/approved
        /// </summary>
        Accepted,
        /// <summary>
        /// The payment transfer was rejected
        /// </summary>
        Rejected
    }
}
