using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PartnersPayments.Settings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string RabbitMqConnectionString { get; set; }
    }
}
