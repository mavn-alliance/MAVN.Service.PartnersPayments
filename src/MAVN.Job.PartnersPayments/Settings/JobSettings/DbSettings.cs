using Lykke.SettingsReader.Attributes;

namespace MAVN.Job.PartnersPayments.Settings.JobSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }

        public string DataConnString { get; set; }
    }
}
