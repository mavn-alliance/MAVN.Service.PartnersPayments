using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PartnersPayments.MsSqlRepositories.Migrations
{
    public partial class MakeFiatAmountPaidByCustomerDecimal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "fiat_amount_paid_by_customer",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "fiat_amount_paid_by_customer",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: true,
                oldClrType: typeof(decimal),
                oldNullable: true);
        }
    }
}
