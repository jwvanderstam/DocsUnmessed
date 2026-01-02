using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocsUnmessed.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    LogId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EntityId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    Details = table.Column<string>(type: "TEXT", nullable: true),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.LogId);
                });

            migrationBuilder.CreateTable(
                name: "Duplicates",
                columns: table => new
                {
                    DuplicateGroupId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    FileCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalSize = table.Column<long>(type: "INTEGER", nullable: false),
                    FirstSeenAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastSeenAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Duplicates", x => x.DuplicateGroupId);
                });

            migrationBuilder.CreateTable(
                name: "Rules",
                columns: table => new
                {
                    RuleId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Configuration = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastUsedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UsageCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.RuleId);
                });

            migrationBuilder.CreateTable(
                name: "Scans",
                columns: table => new
                {
                    ScanId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    ProviderId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RootPath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TotalItems = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalSize = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalFiles = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalFolders = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Configuration = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scans", x => x.ScanId);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    ScanId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Extension = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    Hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AccessedUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ParentPath = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Depth = table.Column<int>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false),
                    Attributes = table.Column<string>(type: "TEXT", nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Items_Scans_ScanId",
                        column: x => x.ScanId,
                        principalTable: "Scans",
                        principalColumn: "ScanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MigrationPlans",
                columns: table => new
                {
                    PlanId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    ScanId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TotalOperations = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedOperations = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedOperations = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalFiles = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalSize = table.Column<long>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Configuration = table.Column<string>(type: "TEXT", nullable: true),
                    Metrics = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MigrationPlans", x => x.PlanId);
                    table.ForeignKey(
                        name: "FK_MigrationPlans_Scans_ScanId",
                        column: x => x.ScanId,
                        principalTable: "Scans",
                        principalColumn: "ScanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DuplicateItems",
                columns: table => new
                {
                    DuplicateGroupId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    ItemId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuplicateItems", x => new { x.DuplicateGroupId, x.ItemId });
                    table.ForeignKey(
                        name: "FK_DuplicateItems_Duplicates_DuplicateGroupId",
                        column: x => x.DuplicateGroupId,
                        principalTable: "Duplicates",
                        principalColumn: "DuplicateGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DuplicateItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Suggestions",
                columns: table => new
                {
                    SuggestionId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    ScanId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    ItemId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    RuleId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    SourcePath = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    SourceName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TargetPath = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    TargetName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Confidence = table.Column<double>(type: "REAL", nullable: false),
                    Reasons = table.Column<string>(type: "TEXT", nullable: true),
                    ConflictPolicy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    AppliedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suggestions", x => x.SuggestionId);
                    table.ForeignKey(
                        name: "FK_Suggestions_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Suggestions_Rules_RuleId",
                        column: x => x.RuleId,
                        principalTable: "Rules",
                        principalColumn: "RuleId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Suggestions_Scans_ScanId",
                        column: x => x.ScanId,
                        principalTable: "Scans",
                        principalColumn: "ScanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MigrationOperations",
                columns: table => new
                {
                    OperationId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    PlanId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    ItemId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SourcePath = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    TargetPath = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VerificationHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MigrationOperations", x => x.OperationId);
                    table.ForeignKey(
                        name: "FK_MigrationOperations_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_MigrationOperations_MigrationPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "MigrationPlans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Action",
                table: "AuditLog",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EntityType_EntityId",
                table: "AuditLog",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Success",
                table: "AuditLog",
                column: "Success");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Timestamp",
                table: "AuditLog",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateItems_DuplicateGroupId",
                table: "DuplicateItems",
                column: "DuplicateGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateItems_ItemId",
                table: "DuplicateItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Duplicates_FileCount",
                table: "Duplicates",
                column: "FileCount");

            migrationBuilder.CreateIndex(
                name: "IX_Duplicates_Hash",
                table: "Duplicates",
                column: "Hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_Extension",
                table: "Items",
                column: "Extension",
                filter: "Extension IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Hash",
                table: "Items",
                column: "Hash",
                filter: "Hash IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ModifiedUtc",
                table: "Items",
                column: "ModifiedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Path",
                table: "Items",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ScanId",
                table: "Items",
                column: "ScanId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_SizeBytes",
                table: "Items",
                column: "SizeBytes");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Type",
                table: "Items",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationOperations_ItemId",
                table: "MigrationOperations",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationOperations_PlanId",
                table: "MigrationOperations",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationOperations_Priority",
                table: "MigrationOperations",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationOperations_Status",
                table: "MigrationOperations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationOperations_Type",
                table: "MigrationOperations",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationPlans_CreatedAt",
                table: "MigrationPlans",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationPlans_ScanId",
                table: "MigrationPlans",
                column: "ScanId");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationPlans_Status",
                table: "MigrationPlans",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Rules_Name",
                table: "Rules",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rules_Priority_IsEnabled",
                table: "Rules",
                columns: new[] { "Priority", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_Rules_Type",
                table: "Rules",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Scans_ProviderId",
                table: "Scans",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Scans_StartedAt",
                table: "Scans",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Scans_Status",
                table: "Scans",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Suggestions_Confidence",
                table: "Suggestions",
                column: "Confidence");

            migrationBuilder.CreateIndex(
                name: "IX_Suggestions_ItemId",
                table: "Suggestions",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Suggestions_RuleId",
                table: "Suggestions",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Suggestions_ScanId",
                table: "Suggestions",
                column: "ScanId");

            migrationBuilder.CreateIndex(
                name: "IX_Suggestions_Status",
                table: "Suggestions",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "DuplicateItems");

            migrationBuilder.DropTable(
                name: "MigrationOperations");

            migrationBuilder.DropTable(
                name: "Suggestions");

            migrationBuilder.DropTable(
                name: "Duplicates");

            migrationBuilder.DropTable(
                name: "MigrationPlans");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Rules");

            migrationBuilder.DropTable(
                name: "Scans");
        }
    }
}
