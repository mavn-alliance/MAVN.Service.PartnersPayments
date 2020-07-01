using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.PartnersPayments.MsSqlRepositories.Migrations
{
    public partial class Initial : Migration
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
                    pos_id = table.Column<string>(nullable: true),
                    tokens_amount = table.Column<string>(nullable: false),
                    tokens_amount_paid_by_customer = table.Column<string>(nullable: true),
                    fiat_amount_paid_by_customer = table.Column<decimal>(nullable: true),
                    fiat_amount = table.Column<decimal>(nullable: false),
                    total_bill_amount = table.Column<decimal>(nullable: false),
                    currency = table.Column<string>(nullable: true),
                    partner_message_id = table.Column<string>(nullable: true),
                    tokens_to_fiat_conversion_rate = table.Column<string>(nullable: false),
                    tokens_reserve_timestamp = table.Column<DateTime>(nullable: true),
                    tokens_burn_timestamp = table.Column<DateTime>(nullable: true),
                    timestamp = table.Column<DateTime>(nullable: false),
                    last_updated_timestamp = table.Column<DateTime>(nullable: false),
                    customer_action_expiration_timestamp = table.Column<DateTime>(nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_partners_payments_customer_id",
                schema: "partners_payments",
                table: "partners_payments",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_request_blockchain_data_last_operation_id",
                schema: "partners_payments",
                table: "payment_request_blockchain_data",
                column: "last_operation_id");
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
