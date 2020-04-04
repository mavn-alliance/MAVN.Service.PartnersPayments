using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.PartnersPayments.MsSqlRepositories.Migrations
{
    public partial class AddConversionRateAndChangeAmountsToNonNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            UPDATE
                partners_payments.partners_payments
            SET
                tokens_amount = fiat_amount
            where tokens_amount IS NULL

            UPDATE
                partners_payments.partners_payments
            SET
                fiat_amount = tokens_amount
            where fiat_amount IS NULL");

            migrationBuilder.AlterColumn<long>(
                name: "tokens_amount",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "fiat_amount",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: false,
                oldClrType: typeof(decimal),
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tokens_to_fiat_conversion_rate",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(@"
            UPDATE
                partners_payments.partners_payments
            SET
                tokens_to_fiat_conversion_rate = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tokens_to_fiat_conversion_rate",
                schema: "partners_payments",
                table: "partners_payments");

            migrationBuilder.AlterColumn<long>(
                name: "tokens_amount",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<decimal>(
                name: "fiat_amount",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: true,
                oldClrType: typeof(decimal));
        }
    }
}
