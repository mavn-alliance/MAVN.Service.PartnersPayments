using System;

namespace Lykke.Job.PartnersPayments.Settings.JobSettings
{
    public class PartnersPaymentsJobSettings
    {
        public DbSettings Db { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }

        public TimeSpan IdlePeriod { get; set; }

        public Constants Constants { get; set; }

    }
}
