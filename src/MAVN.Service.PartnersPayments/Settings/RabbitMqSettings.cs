using Lykke.SettingsReader.Attributes;

namespace MAVN.Service.PartnersPayments.Settings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string RabbitMqConnectionString { get; set; }
    }
}
