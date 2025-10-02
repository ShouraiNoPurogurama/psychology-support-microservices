using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixSetNamingRuleToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OutboxMessages",
                schema: "public",
                table: "OutboxMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailTraces",
                schema: "public",
                table: "EmailTraces");

            migrationBuilder.RenameTable(
                name: "OutboxMessages",
                schema: "public",
                newName: "outbox_messages",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "EmailTraces",
                schema: "public",
                newName: "email_traces",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "Type",
                schema: "public",
                table: "outbox_messages",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Content",
                schema: "public",
                table: "outbox_messages",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "outbox_messages",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ProcessedOn",
                schema: "public",
                table: "outbox_messages",
                newName: "processed_on");

            migrationBuilder.RenameColumn(
                name: "OccuredOn",
                schema: "public",
                table: "outbox_messages",
                newName: "occured_on");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "outbox_messages",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                schema: "public",
                table: "outbox_messages",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "public",
                table: "outbox_messages",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "public",
                table: "outbox_messages",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "To",
                schema: "public",
                table: "email_traces",
                newName: "to");

            migrationBuilder.RenameColumn(
                name: "Subject",
                schema: "public",
                table: "email_traces",
                newName: "subject");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "public",
                table: "email_traces",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Body",
                schema: "public",
                table: "email_traces",
                newName: "body");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "email_traces",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TrackerId",
                schema: "public",
                table: "email_traces",
                newName: "tracker_id");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                schema: "public",
                table: "email_traces",
                newName: "message_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "public",
                table: "email_traces",
                newName: "created_at");

            migrationBuilder.AddPrimaryKey(
                name: "pk_outbox_messages",
                schema: "public",
                table: "outbox_messages",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_email_traces",
                schema: "public",
                table: "email_traces",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_outbox_messages",
                schema: "public",
                table: "outbox_messages");

            migrationBuilder.DropPrimaryKey(
                name: "pk_email_traces",
                schema: "public",
                table: "email_traces");

            migrationBuilder.RenameTable(
                name: "outbox_messages",
                schema: "public",
                newName: "OutboxMessages",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "email_traces",
                schema: "public",
                newName: "EmailTraces",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "type",
                schema: "public",
                table: "OutboxMessages",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "content",
                schema: "public",
                table: "OutboxMessages",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "OutboxMessages",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "processed_on",
                schema: "public",
                table: "OutboxMessages",
                newName: "ProcessedOn");

            migrationBuilder.RenameColumn(
                name: "occured_on",
                schema: "public",
                table: "OutboxMessages",
                newName: "OccuredOn");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                schema: "public",
                table: "OutboxMessages",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                schema: "public",
                table: "OutboxMessages",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "created_by",
                schema: "public",
                table: "OutboxMessages",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "public",
                table: "OutboxMessages",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "to",
                schema: "public",
                table: "EmailTraces",
                newName: "To");

            migrationBuilder.RenameColumn(
                name: "subject",
                schema: "public",
                table: "EmailTraces",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "public",
                table: "EmailTraces",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "body",
                schema: "public",
                table: "EmailTraces",
                newName: "Body");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "EmailTraces",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tracker_id",
                schema: "public",
                table: "EmailTraces",
                newName: "TrackerId");

            migrationBuilder.RenameColumn(
                name: "message_id",
                schema: "public",
                table: "EmailTraces",
                newName: "MessageId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "public",
                table: "EmailTraces",
                newName: "CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OutboxMessages",
                schema: "public",
                table: "OutboxMessages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailTraces",
                schema: "public",
                table: "EmailTraces",
                column: "Id");
        }
    }
}
