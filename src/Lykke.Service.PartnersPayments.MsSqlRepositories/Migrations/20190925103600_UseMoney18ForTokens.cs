using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PartnersPayments.MsSqlRepositories.Migrations
{
    public partial class UseMoney18ForTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "tokens_amount_paid_by_customer",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: true,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "tokens_amount",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: false,
                oldClrType: typeof(long));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "tokens_amount_paid_by_customer",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "tokens_amount",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
