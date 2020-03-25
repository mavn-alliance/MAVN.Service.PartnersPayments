using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PartnersPayments.MsSqlRepositories.Migrations
{
    public partial class AddFiatAmountPaidByCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "fiat_amount_paid_by_customer",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fiat_amount_paid_by_customer",
                schema: "partners_payments",
                table: "partners_payments");
        }
    }
}
