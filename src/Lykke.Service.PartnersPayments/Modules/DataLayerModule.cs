﻿using Autofac;
using JetBrains.Annotations;
using Lykke.Common.MsSql;
using Lykke.Service.PartnersPayments.Domain.Repositories;
using Lykke.Service.PartnersPayments.MsSqlRepositories;
using Lykke.Service.PartnersPayments.MsSqlRepositories.Repositories;
using Lykke.Service.PartnersPayments.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.PartnersPayments.Modules
{
    [UsedImplicitly]
    public class DataLayerModule : Module
    {
        private readonly DbSettings _settings;

        public DataLayerModule(IReloadingManager<AppSettings> appSettings)
        {
            _settings = appSettings.CurrentValue.PartnersPaymentsService.Db;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMsSql(
                _settings.DataConnString,
                connString => new PartnersPaymentsContext(connString, false),
                dbConn => new PartnersPaymentsContext(dbConn));

            builder.RegisterType<PaymentsRepository>()
                .As<IPaymentsRepository>()
                .SingleInstance();

            builder.RegisterType<PaymentRequestBlockchainRepository>()
                .As<IPaymentRequestBlockchainRepository>()
                .SingleInstance();
        }
    }
}
