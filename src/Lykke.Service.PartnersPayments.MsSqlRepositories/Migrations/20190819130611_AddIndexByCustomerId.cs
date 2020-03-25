using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PartnersPayments.MsSqlRepositories.Migrations
{
    public partial class AddIndexByCustomerId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "customer_id",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_partners_payments_customer_id",
                schema: "partners_payments",
                table: "partners_payments",
                column: "customer_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_partners_payments_customer_id",
                schema: "partners_payments",
                table: "partners_payments");

            migrationBuilder.AlterColumn<string>(
                name: "customer_id",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
