using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PartnersPayments.MsSqlRepositories.Migrations
{
    public partial class UseMoney18ForRate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "tokens_to_fiat_conversion_rate",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: false,
                oldClrType: typeof(decimal));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "tokens_to_fiat_conversion_rate",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
