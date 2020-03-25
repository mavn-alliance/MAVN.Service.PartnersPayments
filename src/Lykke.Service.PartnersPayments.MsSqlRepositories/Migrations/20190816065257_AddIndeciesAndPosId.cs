using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PartnersPayments.MsSqlRepositories.Migrations
{
    public partial class AddIndeciesAndPosId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "last_operation_id",
                schema: "partners_payments",
                table: "payment_request_blockchain_data",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<string>(
                name: "pos_id",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_request_blockchain_data_last_operation_id",
                schema: "partners_payments",
                table: "payment_request_blockchain_data",
                column: "last_operation_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_payment_request_blockchain_data_last_operation_id",
                schema: "partners_payments",
                table: "payment_request_blockchain_data");

            migrationBuilder.DropColumn(
                name: "pos_id",
                schema: "partners_payments",
                table: "partners_payments");

            migrationBuilder.AlterColumn<string>(
                name: "last_operation_id",
                schema: "partners_payments",
                table: "payment_request_blockchain_data",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
