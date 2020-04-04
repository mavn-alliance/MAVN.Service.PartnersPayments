using JetBrains.Annotations;

namespace MAVN.Service.PartnersPayments.Client
{
    /// <summary>
    /// PartnersPayments client interface.
    /// </summary>
    [PublicAPI]
    public interface IPartnersPaymentsClient
    {
        // Make your app's controller interfaces visible by adding corresponding properties here.
        // NO actual methods should be placed here (these go to controller interfaces, for example - IPartnersPaymentsApi).
        // ONLY properties for accessing controller interfaces are allowed.

        /// <summary>Application Api interface</summary>
        IPartnersPaymentsApi Api { get; }
    }
}
