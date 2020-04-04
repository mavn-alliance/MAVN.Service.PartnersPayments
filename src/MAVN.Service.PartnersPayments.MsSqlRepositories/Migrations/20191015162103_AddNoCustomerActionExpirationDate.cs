using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.PartnersPayments.MsSqlRepositories.Migrations
{
    public partial class AddNoCustomerActionExpirationDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "no_customer_action_expiration_date",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: false,
                defaultValue: DateTime.UtcNow.AddDays(1));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "no_customer_action_expiration_date",
                schema: "partners_payments",
                table: "partners_payments");
        }
    }
}
