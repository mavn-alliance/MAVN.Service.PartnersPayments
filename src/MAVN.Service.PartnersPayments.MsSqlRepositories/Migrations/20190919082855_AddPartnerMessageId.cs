using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.PartnersPayments.MsSqlRepositories.Migrations
{
    public partial class AddPartnerMessageId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "payment_info",
                schema: "partners_payments",
                table: "partners_payments",
                newName: "partner_message_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "partner_message_id",
                schema: "partners_payments",
                table: "partners_payments",
                newName: "payment_info");
        }
    }
}
