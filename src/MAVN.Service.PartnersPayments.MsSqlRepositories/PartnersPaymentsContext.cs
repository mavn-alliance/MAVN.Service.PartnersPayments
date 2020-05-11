using System.Data.Common;
using MAVN.Common.MsSql;
using MAVN.Service.PartnersPayments.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace MAVN.Service.PartnersPayments.MsSqlRepositories
{
    public class PartnersPaymentsContext : MsSqlContext
    {
        private const string Schema = "partners_payments";

        internal DbSet<PartnerPaymentEntity> PartnersPayments { get; set; }

        internal DbSet<PaymentRequestBlockchainEntity> PaymentRequestBlockchainData { get; set; }

        public PartnersPaymentsContext() 
            : base(Schema)
        {
        }

        public PartnersPaymentsContext(string connectionString, bool isTraceEnabled) 
            : base(Schema, connectionString, isTraceEnabled)
        {
        }

        public PartnersPaymentsContext(DbConnection dbConnection)
            : base(Schema, dbConnection)
        {
        }

        protected override void OnLykkeModelCreating(ModelBuilder modelBuilder)
        {
            var paymentRequestBlockchainEntityBuilder = modelBuilder.Entity<PaymentRequestBlockchainEntity>();
            paymentRequestBlockchainEntityBuilder.HasIndex(x => x.LastOperationId).IsUnique(false);

            var paymentRequestEntityBuilder = modelBuilder.Entity<PartnerPaymentEntity>();
            paymentRequestEntityBuilder.HasIndex(x => x.CustomerId).IsUnique(false);
        }

    }
}
