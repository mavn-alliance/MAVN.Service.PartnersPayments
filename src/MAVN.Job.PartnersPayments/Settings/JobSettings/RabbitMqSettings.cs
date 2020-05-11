using Lykke.SettingsReader.Attributes;

namespace MAVN.Job.PartnersPayments.Settings.JobSettings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string RabbitMqConnectionString { get; set; }
    }
}
