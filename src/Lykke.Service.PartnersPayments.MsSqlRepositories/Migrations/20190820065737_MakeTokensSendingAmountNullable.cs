using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PartnersPayments.MsSqlRepositories.Migrations
{
    public partial class MakeTokensSendingAmountNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "tokens_amount_paid_by_customer",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: true,
                oldClrType: typeof(long));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "tokens_amount_paid_by_customer",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);
        }
    }
}
