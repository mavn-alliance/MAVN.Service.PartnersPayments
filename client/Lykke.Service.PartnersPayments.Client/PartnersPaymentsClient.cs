using Lykke.HttpClientGenerator;

namespace Lykke.Service.PartnersPayments.Client
{
    /// <summary>
    /// PartnersPayments API aggregating interface.
    /// </summary>
    public class PartnersPaymentsClient : IPartnersPaymentsClient
    {
        // Note: Add similar Api properties for each new service controller

        /// <summary>Inerface to PartnersPayments Api.</summary>
        public IPartnersPaymentsApi Api { get; private set; }

        /// <summary>C-tor</summary>
        public PartnersPaymentsClient(IHttpClientGenerator httpClientGenerator)
        {
            Api = httpClientGenerator.Generate<IPartnersPaymentsApi>();
        }
    }
}
