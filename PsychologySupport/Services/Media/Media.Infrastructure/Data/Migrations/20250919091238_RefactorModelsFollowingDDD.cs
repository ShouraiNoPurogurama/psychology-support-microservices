using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Media.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorModelsFollowingDDD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_outbox_messages",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "ix_media_owners_media_id",
                table: "media_owners");

            migrationBuilder.DropPrimaryKey(
                name: "pk_idempotency_keys",
                table: "idempotency_keys");

            migrationBuilder.DropColumn(
                name: "checksum_sha256",
                table: "media_assets");

            migrationBuilder.DropColumn(
                name: "source_mime",
                table: "media_assets");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "outbox_messages",
                newName: "outbox_messages",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "media_variants",
                newName: "media_variants",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "media_processing_jobs",
                newName: "media_processing_jobs",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "media_owners",
                newName: "media_owners",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "media_moderation_audits",
                newName: "media_moderation_audits",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "media_assets",
                newName: "media_assets",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "idempotency_keys",
                newName: "idempotency_keys",
                newSchema: "public");

            migrationBuilder.RenameIndex(
                name: "ix_media_processing_jobs_media_id",
                schema: "public",
                table: "media_processing_jobs",
                newName: "ix_processing_jobs_media_id");

            migrationBuilder.RenameIndex(
                name: "ix_media_moderation_audits_media_id",
                schema: "public",
                table: "media_moderation_audits",
                newName: "ix_moderation_audits_media_id");

            migrationBuilder.RenameColumn(
                name: "source_bytes",
                schema: "public",
                table: "media_assets",
                newName: "size_in_bytes");

            migrationBuilder.RenameColumn(
                name: "raw_moderation_json",
                schema: "public",
                table: "media_assets",
                newName: "moderation_raw_json");

            migrationBuilder.AlterColumn<string>(
                name: "event_type",
                schema: "public",
                table: "outbox_messages",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                schema: "public",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "variant_type",
                schema: "public",
                table: "media_variants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)");

            migrationBuilder.AlterColumn<string>(
                name: "format",
                schema: "public",
                table: "media_variants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                schema: "public",
                table: "media_variants",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "cdn_url",
                schema: "public",
                table: "media_variants",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "bucket_key",
                schema: "public",
                table: "media_variants",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "public",
                table: "media_processing_jobs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)");

            migrationBuilder.AlterColumn<string>(
                name: "job_type",
                schema: "public",
                table: "media_processing_jobs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                schema: "public",
                table: "media_processing_jobs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "attempt",
                schema: "public",
                table: "media_processing_jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "media_owner_type",
                schema: "public",
                table: "media_owners",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                schema: "public",
                table: "media_owners",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "public",
                table: "media_moderation_audits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)");

            migrationBuilder.AlterColumn<decimal>(
                name: "score",
                schema: "public",
                table: "media_moderation_audits",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "policy_version",
                schema: "public",
                table: "media_moderation_audits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                schema: "public",
                table: "media_moderation_audits",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "state",
                schema: "public",
                table: "media_assets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)");

            migrationBuilder.AlterColumn<string>(
                name: "phash64",
                schema: "public",
                table: "media_assets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "moderation_status",
                schema: "public",
                table: "media_assets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "moderation_score",
                schema: "public",
                table: "media_assets",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "moderation_policy_version",
                schema: "public",
                table: "media_assets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "hold_thumb_until_pass",
                schema: "public",
                table: "media_assets",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "exif_removed",
                schema: "public",
                table: "media_assets",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                schema: "public",
                table: "media_assets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "aspect_ratio_denominator",
                schema: "public",
                table: "media_assets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "aspect_ratio_numerator",
                schema: "public",
                table: "media_assets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "checksum_algorithm",
                schema: "public",
                table: "media_assets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "checksum_value",
                schema: "public",
                table: "media_assets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "public",
                table: "media_assets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                schema: "public",
                table: "media_assets",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mime_type",
                schema: "public",
                table: "media_assets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "request_hash",
                schema: "public",
                table: "idempotency_keys",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                schema: "public",
                table: "idempotency_keys",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "outbox_messages_pkey",
                schema: "public",
                table: "outbox_messages",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "idempotency_keys_pkey",
                schema: "public",
                table: "idempotency_keys",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_occurred_on",
                schema: "public",
                table: "outbox_messages",
                column: "occurred_on",
                filter: "processed_on IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_on",
                schema: "public",
                table: "outbox_messages",
                column: "processed_on",
                filter: "processed_on IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_media_variants_cdn_url",
                schema: "public",
                table: "media_variants",
                column: "cdn_url",
                filter: "cdn_url IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_media_variants_media_type",
                schema: "public",
                table: "media_variants",
                columns: new[] { "media_id", "variant_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_processing_jobs_queue",
                schema: "public",
                table: "media_processing_jobs",
                columns: new[] { "status", "next_retry_at" },
                filter: "status NOT IN ('Succeeded', 'Failed', 'Cancelled')");

            migrationBuilder.CreateIndex(
                name: "ix_processing_jobs_type_status",
                schema: "public",
                table: "media_processing_jobs",
                columns: new[] { "job_type", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_media_owners_owner",
                schema: "public",
                table: "media_owners",
                columns: new[] { "media_owner_type", "media_owner_id" });

            migrationBuilder.CreateIndex(
                name: "ux_media_owners_unique",
                schema: "public",
                table: "media_owners",
                columns: new[] { "media_id", "media_owner_type", "media_owner_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_moderation_audits_checked_at",
                schema: "public",
                table: "media_moderation_audits",
                column: "checked_at");

            migrationBuilder.CreateIndex(
                name: "ix_media_assets_created_state",
                schema: "public",
                table: "media_assets",
                columns: new[] { "created_at", "state" });

            migrationBuilder.CreateIndex(
                name: "ix_media_assets_state",
                schema: "public",
                table: "media_assets",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "ix_idempotency_keys_expires_at",
                schema: "public",
                table: "idempotency_keys",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_idempotency_keys_key",
                schema: "public",
                table: "idempotency_keys",
                column: "key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "outbox_messages_pkey",
                schema: "public",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "ix_outbox_messages_occurred_on",
                schema: "public",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "ix_outbox_messages_processed_on",
                schema: "public",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "ix_media_variants_cdn_url",
                schema: "public",
                table: "media_variants");

            migrationBuilder.DropIndex(
                name: "ux_media_variants_media_type",
                schema: "public",
                table: "media_variants");

            migrationBuilder.DropIndex(
                name: "ix_processing_jobs_queue",
                schema: "public",
                table: "media_processing_jobs");

            migrationBuilder.DropIndex(
                name: "ix_processing_jobs_type_status",
                schema: "public",
                table: "media_processing_jobs");

            migrationBuilder.DropIndex(
                name: "ix_media_owners_owner",
                schema: "public",
                table: "media_owners");

            migrationBuilder.DropIndex(
                name: "ux_media_owners_unique",
                schema: "public",
                table: "media_owners");

            migrationBuilder.DropIndex(
                name: "ix_moderation_audits_checked_at",
                schema: "public",
                table: "media_moderation_audits");

            migrationBuilder.DropIndex(
                name: "ix_media_assets_created_state",
                schema: "public",
                table: "media_assets");

            migrationBuilder.DropIndex(
                name: "ix_media_assets_state",
                schema: "public",
                table: "media_assets");

            migrationBuilder.DropPrimaryKey(
                name: "idempotency_keys_pkey",
                schema: "public",
                table: "idempotency_keys");

            migrationBuilder.DropIndex(
                name: "ix_idempotency_keys_expires_at",
                schema: "public",
                table: "idempotency_keys");

            migrationBuilder.DropIndex(
                name: "ix_idempotency_keys_key",
                schema: "public",
                table: "idempotency_keys");

            migrationBuilder.DropColumn(
                name: "aspect_ratio_denominator",
                schema: "public",
                table: "media_assets");

            migrationBuilder.DropColumn(
                name: "aspect_ratio_numerator",
                schema: "public",
                table: "media_assets");

            migrationBuilder.DropColumn(
                name: "checksum_algorithm",
                schema: "public",
                table: "media_assets");

            migrationBuilder.DropColumn(
                name: "checksum_value",
                schema: "public",
                table: "media_assets");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "public",
                table: "media_assets");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                schema: "public",
                table: "media_assets");

            migrationBuilder.DropColumn(
                name: "mime_type",
                schema: "public",
                table: "media_assets");

            migrationBuilder.RenameTable(
                name: "outbox_messages",
                schema: "public",
                newName: "outbox_messages");

            migrationBuilder.RenameTable(
                name: "media_variants",
                schema: "public",
                newName: "media_variants");

            migrationBuilder.RenameTable(
                name: "media_processing_jobs",
                schema: "public",
                newName: "media_processing_jobs");

            migrationBuilder.RenameTable(
                name: "media_owners",
                schema: "public",
                newName: "media_owners");

            migrationBuilder.RenameTable(
                name: "media_moderation_audits",
                schema: "public",
                newName: "media_moderation_audits");

            migrationBuilder.RenameTable(
                name: "media_assets",
                schema: "public",
                newName: "media_assets");

            migrationBuilder.RenameTable(
                name: "idempotency_keys",
                schema: "public",
                newName: "idempotency_keys");

            migrationBuilder.RenameIndex(
                name: "ix_processing_jobs_media_id",
                table: "media_processing_jobs",
                newName: "ix_media_processing_jobs_media_id");

            migrationBuilder.RenameIndex(
                name: "ix_moderation_audits_media_id",
                table: "media_moderation_audits",
                newName: "ix_media_moderation_audits_media_id");

            migrationBuilder.RenameColumn(
                name: "size_in_bytes",
                table: "media_assets",
                newName: "source_bytes");

            migrationBuilder.RenameColumn(
                name: "moderation_raw_json",
                table: "media_assets",
                newName: "raw_moderation_json");

            migrationBuilder.AlterColumn<string>(
                name: "event_type",
                table: "outbox_messages",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "variant_type",
                table: "media_variants",
                type: "VARCHAR(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "format",
                table: "media_variants",
                type: "VARCHAR(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "media_variants",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "cdn_url",
                table: "media_variants",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "bucket_key",
                table: "media_variants",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "media_processing_jobs",
                type: "VARCHAR(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "job_type",
                table: "media_processing_jobs",
                type: "VARCHAR(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "media_processing_jobs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "attempt",
                table: "media_processing_jobs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "media_owner_type",
                table: "media_owners",
                type: "VARCHAR(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "media_owners",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "media_moderation_audits",
                type: "VARCHAR(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "score",
                table: "media_moderation_audits",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,4)",
                oldPrecision: 5,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "policy_version",
                table: "media_moderation_audits",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "media_moderation_audits",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "state",
                table: "media_assets",
                type: "VARCHAR(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "phash64",
                table: "media_assets",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "moderation_status",
                table: "media_assets",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "moderation_score",
                table: "media_assets",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,4)",
                oldPrecision: 5,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "moderation_policy_version",
                table: "media_assets",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "hold_thumb_until_pass",
                table: "media_assets",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "exif_removed",
                table: "media_assets",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "media_assets",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "checksum_sha256",
                table: "media_assets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "source_mime",
                table: "media_assets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "request_hash",
                table: "idempotency_keys",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "idempotency_keys",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddPrimaryKey(
                name: "pk_outbox_messages",
                table: "outbox_messages",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_idempotency_keys",
                table: "idempotency_keys",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_media_owners_media_id",
                table: "media_owners",
                column: "media_id");
        }
    }
}
