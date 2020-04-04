using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.PartnersPayments.MsSqlRepositories.Migrations
{
    public partial class AddLastUpdatedTimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "last_updated_timestamp",
                schema: "partners_payments",
                table: "partners_payments",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_updated_timestamp",
                schema: "partners_payments",
                table: "partners_payments");
        }
    }
}
