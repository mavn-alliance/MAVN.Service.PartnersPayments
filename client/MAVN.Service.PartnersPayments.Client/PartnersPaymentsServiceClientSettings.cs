using Lykke.SettingsReader.Attributes;

namespace MAVN.Service.PartnersPayments.Client 
{
    /// <summary>
    /// PartnersPayments client settings.
    /// </summary>
    public class PartnersPaymentsServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}
