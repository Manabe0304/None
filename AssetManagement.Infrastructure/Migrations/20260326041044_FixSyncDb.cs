using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSyncDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "expected_return_date",
                table: "return_requests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "repair_cost",
                table: "maintenance_records",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "estimated_cost",
                table: "maintenance_records",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "due_date",
                table: "maintenance_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "purchase_value",
                table: "assets",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "beyond_repair_reason",
            //    table: "assets",
            //    type: "text",
            //    nullable: true);

            //migrationBuilder.AddColumn<bool>(
            //    name: "is_beyond_repair",
            //    table: "assets",
            //    type: "boolean",
            //    nullable: false,
            //    defaultValue: false);

            //migrationBuilder.AddColumn<string>(
            //    name: "model",
            //    table: "assets",
            //    type: "text",
            //    nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "request_type",
                table: "asset_requests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "asset_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    changed_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "text", nullable: false),
                    previous_status = table.Column<string>(type: "text", nullable: true),
                    new_status = table.Column<string>(type: "text", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_histories", x => x.id);
                    table.ForeignKey(
                        name: "FK_asset_histories_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_asset_histories_assignments_assignment_id",
                        column: x => x.assignment_id,
                        principalTable: "assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_asset_histories_users_changed_by_id",
                        column: x => x.changed_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_asset_histories_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "asset_inspections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    return_request_id = table.Column<Guid>(type: "uuid", nullable: true),
                    inspected_by = table.Column<Guid>(type: "uuid", nullable: false),
                    condition = table.Column<string>(type: "text", nullable: true),
                    findings = table.Column<string>(type: "text", nullable: true),
                    inspection_result = table.Column<string>(type: "text", nullable: true),
                    next_action = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    inspected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_inspections", x => x.id);
                    table.ForeignKey(
                        name: "FK_asset_inspections_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_asset_inspections_return_requests_return_request_id",
                        column: x => x.return_request_id,
                        principalTable: "return_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_asset_inspections_users_inspected_by",
                        column: x => x.inspected_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asset_reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reserved_by = table.Column<Guid>(type: "uuid", nullable: false),
                    reserved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expected_assign_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_reservations", x => x.id);
                    table.ForeignKey(
                        name: "FK_asset_reservations_asset_requests_request_id",
                        column: x => x.request_id,
                        principalTable: "asset_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_asset_reservations_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_asset_reservations_users_reserved_by",
                        column: x => x.reserved_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_imported_by",
                table: "import_batches",
                column: "imported_by");

            migrationBuilder.CreateIndex(
                name: "IX_asset_histories_asset_id",
                table: "asset_histories",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_asset_histories_assignment_id",
                table: "asset_histories",
                column: "assignment_id");

            migrationBuilder.CreateIndex(
                name: "IX_asset_histories_changed_by_id",
                table: "asset_histories",
                column: "changed_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_asset_histories_user_id",
                table: "asset_histories",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_asset_inspections_asset_id",
                table: "asset_inspections",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_asset_inspections_inspected_by",
                table: "asset_inspections",
                column: "inspected_by");

            migrationBuilder.CreateIndex(
                name: "IX_asset_inspections_return_request_id",
                table: "asset_inspections",
                column: "return_request_id");

            migrationBuilder.CreateIndex(
                name: "IX_asset_reservations_request_id",
                table: "asset_reservations",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_asset_reservations_reserved_by",
                table: "asset_reservations",
                column: "reserved_by");

            migrationBuilder.CreateIndex(
                name: "uq_reservation_asset",
                table: "asset_reservations",
                columns: new[] { "asset_id", "status" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_import_batches_users_imported_by",
                table: "import_batches",
                column: "imported_by",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_import_batches_users_imported_by",
                table: "import_batches");

            migrationBuilder.DropTable(
                name: "asset_histories");

            migrationBuilder.DropTable(
                name: "asset_inspections");

            migrationBuilder.DropTable(
                name: "asset_reservations");

            migrationBuilder.DropIndex(
                name: "IX_import_batches_imported_by",
                table: "import_batches");

            migrationBuilder.DropColumn(
                name: "expected_return_date",
                table: "return_requests");

            migrationBuilder.DropColumn(
                name: "due_date",
                table: "maintenance_records");

            migrationBuilder.DropColumn(
                name: "beyond_repair_reason",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "is_beyond_repair",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "model",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "request_type",
                table: "asset_requests");

            migrationBuilder.AlterColumn<decimal>(
                name: "repair_cost",
                table: "maintenance_records",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "estimated_cost",
                table: "maintenance_records",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "purchase_value",
                table: "assets",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);
        }
    }
}
