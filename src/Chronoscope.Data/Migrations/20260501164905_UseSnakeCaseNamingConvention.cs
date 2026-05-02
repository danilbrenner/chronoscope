using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chronoscope.Data.Migrations
{
    /// <inheritdoc />
    public partial class UseSnakeCaseNamingConvention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_photos_sources_SourceId",
                table: "photos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sources",
                table: "sources");

            migrationBuilder.DropPrimaryKey(
                name: "PK_photos",
                table: "photos");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "sources",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "sources",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "FolderPath",
                table: "sources",
                newName: "folder_path");

            migrationBuilder.RenameColumn(
                name: "DeltaToken",
                table: "sources",
                newName: "delta_token");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "sources",
                newName: "created_at_utc");

            migrationBuilder.RenameColumn(
                name: "AuthState",
                table: "sources",
                newName: "auth_state");

            migrationBuilder.RenameColumn(
                name: "Thumbnail",
                table: "photos",
                newName: "thumbnail");

            migrationBuilder.RenameColumn(
                name: "Filename",
                table: "photos",
                newName: "filename");

            migrationBuilder.RenameColumn(
                name: "TakenAt",
                table: "photos",
                newName: "taken_at");

            migrationBuilder.RenameColumn(
                name: "SourceId",
                table: "photos",
                newName: "source_id");

            migrationBuilder.RenameColumn(
                name: "SizeBytes",
                table: "photos",
                newName: "size_bytes");

            migrationBuilder.RenameColumn(
                name: "ProcessingStatus",
                table: "photos",
                newName: "processing_status");

            migrationBuilder.RenameColumn(
                name: "ExternalId",
                table: "photos",
                newName: "external_id");

            migrationBuilder.RenameColumn(
                name: "InternalId",
                table: "photos",
                newName: "internal_id");

            migrationBuilder.RenameIndex(
                name: "IX_photos_SourceId_ExternalId",
                table: "photos",
                newName: "ix_photos_source_id_external_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sources",
                table: "sources",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_photos",
                table: "photos",
                column: "internal_id");

            migrationBuilder.AddForeignKey(
                name: "fk_photos_sources_source_id",
                table: "photos",
                column: "source_id",
                principalTable: "sources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_photos_sources_source_id",
                table: "photos");

            migrationBuilder.DropPrimaryKey(
                name: "pk_sources",
                table: "sources");

            migrationBuilder.DropPrimaryKey(
                name: "pk_photos",
                table: "photos");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "sources",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "sources",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "folder_path",
                table: "sources",
                newName: "FolderPath");

            migrationBuilder.RenameColumn(
                name: "delta_token",
                table: "sources",
                newName: "DeltaToken");

            migrationBuilder.RenameColumn(
                name: "created_at_utc",
                table: "sources",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "auth_state",
                table: "sources",
                newName: "AuthState");

            migrationBuilder.RenameColumn(
                name: "thumbnail",
                table: "photos",
                newName: "Thumbnail");

            migrationBuilder.RenameColumn(
                name: "filename",
                table: "photos",
                newName: "Filename");

            migrationBuilder.RenameColumn(
                name: "taken_at",
                table: "photos",
                newName: "TakenAt");

            migrationBuilder.RenameColumn(
                name: "source_id",
                table: "photos",
                newName: "SourceId");

            migrationBuilder.RenameColumn(
                name: "size_bytes",
                table: "photos",
                newName: "SizeBytes");

            migrationBuilder.RenameColumn(
                name: "processing_status",
                table: "photos",
                newName: "ProcessingStatus");

            migrationBuilder.RenameColumn(
                name: "external_id",
                table: "photos",
                newName: "ExternalId");

            migrationBuilder.RenameColumn(
                name: "internal_id",
                table: "photos",
                newName: "InternalId");

            migrationBuilder.RenameIndex(
                name: "ix_photos_source_id_external_id",
                table: "photos",
                newName: "IX_photos_SourceId_ExternalId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sources",
                table: "sources",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_photos",
                table: "photos",
                column: "InternalId");

            migrationBuilder.AddForeignKey(
                name: "FK_photos_sources_SourceId",
                table: "photos",
                column: "SourceId",
                principalTable: "sources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
