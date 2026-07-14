using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiWarehouse.Service.Migrations
{
    /// <inheritdoc />
    public partial class mig1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("5ae7d32a-9f42-48ba-8779-8d8e1ecf5881"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "CreatedDate", "Email", "EmailConfirmed", "FirstName", "IsActive", "LastLoginDate", "LastName", "PasswordHash", "Phone", "Role", "UpdatedDate" },
                values: new object[] { new Guid("6dd6622b-464b-4ac0-9c9a-e584cb25a3bd"), "", new DateTime(2026, 7, 14, 14, 28, 8, 215, DateTimeKind.Utc).AddTicks(4272), "string", false, "System", true, null, "Admin", "string", "", 0, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("6dd6622b-464b-4ac0-9c9a-e584cb25a3bd"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "CreatedDate", "Email", "EmailConfirmed", "FirstName", "IsActive", "LastLoginDate", "LastName", "PasswordHash", "Phone", "Role", "UpdatedDate" },
                values: new object[] { new Guid("5ae7d32a-9f42-48ba-8779-8d8e1ecf5881"), "", new DateTime(2026, 7, 13, 12, 20, 44, 359, DateTimeKind.Utc).AddTicks(3083), "admin@depo.com", false, "System", true, null, "Admin", "hashed_123456_demo_string", "", 0, null });
        }
    }
}
