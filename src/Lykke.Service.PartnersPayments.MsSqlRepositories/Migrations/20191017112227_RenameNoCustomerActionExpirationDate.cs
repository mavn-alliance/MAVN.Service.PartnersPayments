using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PartnersPayments.MsSqlRepositories.Migrations
{
    public partial class RenameNoCustomerActionExpirationDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "no_customer_action_expiration_date",
                schema: "partners_payments",
                table: "partners_payments",
                newName: "customer_action_expiration_timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "customer_action_expiration_timestamp",
                schema: "partners_payments",
                table: "partners_payments",
                newName: "no_customer_action_expiration_date");
        }
    }
}
