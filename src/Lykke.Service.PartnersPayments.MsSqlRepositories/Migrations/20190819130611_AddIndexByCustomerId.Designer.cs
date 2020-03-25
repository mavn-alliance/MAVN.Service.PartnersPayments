﻿// <auto-generated />
using System;
using Lykke.Service.PartnersPayments.MsSqlRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lykke.Service.PartnersPayments.MsSqlRepositories.Migrations
{
    [DbContext(typeof(PartnersPaymentsContext))]
    [Migration("20190819130611_AddIndexByCustomerId")]
    partial class AddIndexByCustomerId
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("partners_payments")
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Lykke.Service.PartnersPayments.MsSqlRepositories.Entities.PartnerPaymentEntity", b =>
                {
                    b.Property<string>("PaymentRequestId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("payment_request_id");

                    b.Property<string>("Currency")
                        .HasColumnName("currency");

                    b.Property<string>("CustomerId")
                        .IsRequired()
                        .HasColumnName("customer_id");

                    b.Property<decimal>("FiatAmount")
                        .HasColumnName("fiat_amount");

                    b.Property<string>("LocationId")
                        .HasColumnName("location_id");

                    b.Property<string>("PartnerId")
                        .IsRequired()
                        .HasColumnName("partner_id");

                    b.Property<string>("PaymentInfo")
                        .HasColumnName("payment_info");

                    b.Property<string>("PosId")
                        .HasColumnName("pos_id");

                    b.Property<int>("Status")
                        .HasColumnName("status");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnName("timestamp");

                    b.Property<long>("TokensAmount")
                        .HasColumnName("tokens_amount");

                    b.Property<long>("TokensAmountPaidByCustomer")
                        .HasColumnName("tokens_amount_paid_by_customer");

                    b.Property<DateTime?>("TokensBurnTimestamp")
                        .HasColumnName("tokens_burn_timestamp");

                    b.Property<DateTime?>("TokensReserveTimestamp")
                        .HasColumnName("tokens_reserve_timestamp");

                    b.Property<decimal>("TokensToFiatConversionRate")
                        .HasColumnName("tokens_to_fiat_conversion_rate");

                    b.Property<decimal>("TotalBillAmount")
                        .HasColumnName("total_bill_amount");

                    b.HasKey("PaymentRequestId");

                    b.HasIndex("CustomerId");

                    b.ToTable("partners_payments");
                });

            modelBuilder.Entity("Lykke.Service.PartnersPayments.MsSqlRepositories.Entities.PaymentRequestBlockchainEntity", b =>
                {
                    b.Property<string>("PaymentRequestId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("payment_request_id");

                    b.Property<string>("LastOperationId")
                        .IsRequired()
                        .HasColumnName("last_operation_id");

                    b.HasKey("PaymentRequestId");

                    b.HasIndex("LastOperationId");

                    b.ToTable("payment_request_blockchain_data");
                });
#pragma warning restore 612, 618
        }
    }
}
