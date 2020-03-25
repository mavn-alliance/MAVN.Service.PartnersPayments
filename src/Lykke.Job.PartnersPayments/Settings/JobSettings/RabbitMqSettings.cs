using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.PartnersPayments.Settings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string RabbitMqConnectionString { get; set; }
    }
}
