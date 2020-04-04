using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.PartnersPayments.MsSqlRepositories.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "partners_payments");

            migrationBuilder.CreateTable(
                name: "partners_payments",
                schema: "partners_payments",
                columns: table => new
                {
                    payment_request_id = table.Column<string>(nullable: false),
                    customer_id = table.Column<string>(nullable: false),
                    partner_id = table.Column<string>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    location_id = table.Column<string>(nullable: true),
                    tokens_amount = table.Column<long>(nullable: true),
                    tokens_amount_paid_by_customer = table.Column<long>(nullable: false),
                    fiat_amount = table.Column<decimal>(nullable: true),
                    total_bill_amount = table.Column<decimal>(nullable: false),
                    currency = table.Column<string>(nullable: true),
                    payment_info = table.Column<string>(nullable: true),
                    tokens_reserve_timestamp = table.Column<DateTime>(nullable: true),
                    tokens_burn_timestamp = table.Column<DateTime>(nullable: true),
                    timestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partners_payments", x => x.payment_request_id);
                });

            migrationBuilder.CreateTable(
                name: "payment_request_blockchain_data",
                schema: "partners_payments",
                columns: table => new
                {
                    payment_request_id = table.Column<string>(nullable: false),
                    last_operation_id = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_request_blockchain_data", x => x.payment_request_id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "partners_payments",
                schema: "partners_payments");

            migrationBuilder.DropTable(
                name: "payment_request_blockchain_data",
                schema: "partners_payments");
        }
    }
}
