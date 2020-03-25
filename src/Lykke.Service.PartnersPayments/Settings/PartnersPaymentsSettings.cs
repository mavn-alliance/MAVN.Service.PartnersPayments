using JetBrains.Annotations;

namespace Lykke.Service.PartnersPayments.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class PartnersPaymentsSettings
    {
        public DbSettings Db { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }

        public Constants Constants { get; set; }
    }
}
