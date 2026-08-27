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


            migrationBuilder.AddColumn<Guid>(
                name: "CancelledById",
                table: "TransferOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "TransferOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DispatchedById",
                table: "TransferOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReceivedById",
                table: "TransferOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransferOrders_CancelledById",
                table: "TransferOrders",
                column: "CancelledById");

            migrationBuilder.CreateIndex(
                name: "IX_TransferOrders_CreatedById",
                table: "TransferOrders",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TransferOrders_DispatchedById",
                table: "TransferOrders",
                column: "DispatchedById");

            migrationBuilder.CreateIndex(
                name: "IX_TransferOrders_ReceivedById",
                table: "TransferOrders",
                column: "ReceivedById");

            migrationBuilder.AddForeignKey(
                name: "FK_TransferOrders_Users_CancelledById",
                table: "TransferOrders",
                column: "CancelledById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransferOrders_Users_CreatedById",
                table: "TransferOrders",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransferOrders_Users_DispatchedById",
                table: "TransferOrders",
                column: "DispatchedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransferOrders_Users_ReceivedById",
                table: "TransferOrders",
                column: "ReceivedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransferOrders_Users_CancelledById",
                table: "TransferOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferOrders_Users_CreatedById",
                table: "TransferOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferOrders_Users_DispatchedById",
                table: "TransferOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferOrders_Users_ReceivedById",
                table: "TransferOrders");

            migrationBuilder.DropIndex(
                name: "IX_TransferOrders_CancelledById",
                table: "TransferOrders");

            migrationBuilder.DropIndex(
                name: "IX_TransferOrders_CreatedById",
                table: "TransferOrders");

            migrationBuilder.DropIndex(
                name: "IX_TransferOrders_DispatchedById",
                table: "TransferOrders");

            migrationBuilder.DropIndex(
                name: "IX_TransferOrders_ReceivedById",
                table: "TransferOrders");

            migrationBuilder.DropColumn(
                name: "CancelledById",
                table: "TransferOrders");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "TransferOrders");

            migrationBuilder.DropColumn(
                name: "DispatchedById",
                table: "TransferOrders");

            migrationBuilder.DropColumn(
                name: "ReceivedById",
                table: "TransferOrders");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "CreatedDate", "Email", "EmailChangeToken", "EmailChangeTokenExpires", "EmailConfirmed", "FirstName", "IsActive", "LastLoginDate", "LastName", "PasswordHash", "PendingNewEmail", "Phone", "ReceiveEmailNotifications", "ReceiveInAppNotifications", "Role", "UpdatedDate", "WarehouseId" },
                values: new object[] { new Guid("8ecdafd8-c168-41c1-986c-ad3993f142b7"), "", new DateTime(2026, 8, 25, 13, 6, 0, 258, DateTimeKind.Utc).AddTicks(8872), "str@gmail.com", null, null, false, "Esra Nur", true, null, "Çomak", "$2a$11$43rRIxdH7vsTMJRC4zoXv.dntNlaWZ1yqcu1QDW7rtuymihDzgmWm", null, "", true, true, 0, null, null });
        }
    }
}
