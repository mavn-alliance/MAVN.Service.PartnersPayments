namespace MAVN.Service.PartnersPayments.Domain.Enums
{
    /// <summary>
    /// The partners payment contract event types
    /// </summary>
    public enum BlockchainEventType
    {
        /// <summary>
        /// Unknown event type, not expected
        /// </summary>
        Unknown,
        
        /// <summary>
        /// When tokens have been successfully transferred to the contract address 
        /// </summary>
        TransferReceived,
        
        /// <summary>
        /// When the payment have been accepted and tokens burnt
        /// </summary>
        TransferAccepted,
        
        /// <summary>
        /// When the payment has not been accepted, tokens are being refunded
        /// </summary>
        TransferRejected
    }
}
