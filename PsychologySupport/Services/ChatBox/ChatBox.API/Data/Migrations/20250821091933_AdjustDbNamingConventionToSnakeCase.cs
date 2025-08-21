using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBox.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdjustDbNamingConventionToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIChatMessages_AIChatSessions_SessionId",
                schema: "public",
                table: "AIChatMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Messages",
                schema: "public",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorPatients",
                schema: "public",
                table: "DoctorPatients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AIChatSessions",
                schema: "public",
                table: "AIChatSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AIChatMessages",
                schema: "public",
                table: "AIChatMessages");

            migrationBuilder.RenameTable(
                name: "Messages",
                schema: "public",
                newName: "messages",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "DoctorPatients",
                schema: "public",
                newName: "doctor_patients",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "AIChatSessions",
                schema: "public",
                newName: "ai_chat_sessions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "AIChatMessages",
                schema: "public",
                newName: "ai_chat_messages",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "Content",
                schema: "public",
                table: "messages",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "messages",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SenderUserId",
                schema: "public",
                table: "messages",
                newName: "sender_user_id");

            migrationBuilder.RenameColumn(
                name: "ReceiverUserId",
                schema: "public",
                table: "messages",
                newName: "receiver_user_id");

            migrationBuilder.RenameColumn(
                name: "IsRead",
                schema: "public",
                table: "messages",
                newName: "is_read");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                schema: "public",
                table: "messages",
                newName: "created_date");

            migrationBuilder.RenameColumn(
                name: "PatientUserId",
                schema: "public",
                table: "doctor_patients",
                newName: "patient_user_id");

            migrationBuilder.RenameColumn(
                name: "DoctorUserId",
                schema: "public",
                table: "doctor_patients",
                newName: "doctor_user_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "public",
                table: "doctor_patients",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "BookingId",
                schema: "public",
                table: "doctor_patients",
                newName: "booking_id");

            migrationBuilder.RenameColumn(
                name: "Summarization",
                schema: "public",
                table: "ai_chat_sessions",
                newName: "summarization");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "public",
                table: "ai_chat_sessions",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "ai_chat_sessions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "public",
                table: "ai_chat_sessions",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "LastSummarizedIndex",
                schema: "public",
                table: "ai_chat_sessions",
                newName: "last_summarized_index");

            migrationBuilder.RenameColumn(
                name: "LastSummarizedAt",
                schema: "public",
                table: "ai_chat_sessions",
                newName: "last_summarized_at");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                schema: "public",
                table: "ai_chat_sessions",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                schema: "public",
                table: "ai_chat_sessions",
                newName: "created_date");

            migrationBuilder.RenameColumn(
                name: "Content",
                schema: "public",
                table: "ai_chat_messages",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "ai_chat_messages",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                schema: "public",
                table: "ai_chat_messages",
                newName: "session_id");

            migrationBuilder.RenameColumn(
                name: "SenderUserId",
                schema: "public",
                table: "ai_chat_messages",
                newName: "sender_user_id");

            migrationBuilder.RenameColumn(
                name: "SenderIsEmo",
                schema: "public",
                table: "ai_chat_messages",
                newName: "sender_is_emo");

            migrationBuilder.RenameColumn(
                name: "IsRead",
                schema: "public",
                table: "ai_chat_messages",
                newName: "is_read");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                schema: "public",
                table: "ai_chat_messages",
                newName: "created_date");

            migrationBuilder.RenameColumn(
                name: "BlockNumber",
                schema: "public",
                table: "ai_chat_messages",
                newName: "block_number");

            migrationBuilder.RenameIndex(
                name: "IX_AIChatMessages_SessionId",
                schema: "public",
                table: "ai_chat_messages",
                newName: "ix_ai_chat_messages_session_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_messages",
                schema: "public",
                table: "messages",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_doctor_patients",
                schema: "public",
                table: "doctor_patients",
                column: "booking_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_ai_chat_sessions",
                schema: "public",
                table: "ai_chat_sessions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_ai_chat_messages",
                schema: "public",
                table: "ai_chat_messages",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_ai_chat_messages_ai_chat_sessions_session_id",
                schema: "public",
                table: "ai_chat_messages",
                column: "session_id",
                principalSchema: "public",
                principalTable: "ai_chat_sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ai_chat_messages_ai_chat_sessions_session_id",
                schema: "public",
                table: "ai_chat_messages");

            migrationBuilder.DropPrimaryKey(
                name: "pk_messages",
                schema: "public",
                table: "messages");

            migrationBuilder.DropPrimaryKey(
                name: "pk_doctor_patients",
                schema: "public",
                table: "doctor_patients");

            migrationBuilder.DropPrimaryKey(
                name: "pk_ai_chat_sessions",
                schema: "public",
                table: "ai_chat_sessions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_ai_chat_messages",
                schema: "public",
                table: "ai_chat_messages");

            migrationBuilder.RenameTable(
                name: "messages",
                schema: "public",
                newName: "Messages",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "doctor_patients",
                schema: "public",
                newName: "DoctorPatients",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "ai_chat_sessions",
                schema: "public",
                newName: "AIChatSessions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "ai_chat_messages",
                schema: "public",
                newName: "AIChatMessages",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "content",
                schema: "public",
                table: "Messages",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "Messages",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "sender_user_id",
                schema: "public",
                table: "Messages",
                newName: "SenderUserId");

            migrationBuilder.RenameColumn(
                name: "receiver_user_id",
                schema: "public",
                table: "Messages",
                newName: "ReceiverUserId");

            migrationBuilder.RenameColumn(
                name: "is_read",
                schema: "public",
                table: "Messages",
                newName: "IsRead");

            migrationBuilder.RenameColumn(
                name: "created_date",
                schema: "public",
                table: "Messages",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "patient_user_id",
                schema: "public",
                table: "DoctorPatients",
                newName: "PatientUserId");

            migrationBuilder.RenameColumn(
                name: "doctor_user_id",
                schema: "public",
                table: "DoctorPatients",
                newName: "DoctorUserId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "public",
                table: "DoctorPatients",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "booking_id",
                schema: "public",
                table: "DoctorPatients",
                newName: "BookingId");

            migrationBuilder.RenameColumn(
                name: "summarization",
                schema: "public",
                table: "AIChatSessions",
                newName: "Summarization");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "AIChatSessions",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "AIChatSessions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "public",
                table: "AIChatSessions",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "last_summarized_index",
                schema: "public",
                table: "AIChatSessions",
                newName: "LastSummarizedIndex");

            migrationBuilder.RenameColumn(
                name: "last_summarized_at",
                schema: "public",
                table: "AIChatSessions",
                newName: "LastSummarizedAt");

            migrationBuilder.RenameColumn(
                name: "is_active",
                schema: "public",
                table: "AIChatSessions",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_date",
                schema: "public",
                table: "AIChatSessions",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "content",
                schema: "public",
                table: "AIChatMessages",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "AIChatMessages",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "session_id",
                schema: "public",
                table: "AIChatMessages",
                newName: "SessionId");

            migrationBuilder.RenameColumn(
                name: "sender_user_id",
                schema: "public",
                table: "AIChatMessages",
                newName: "SenderUserId");

            migrationBuilder.RenameColumn(
                name: "sender_is_emo",
                schema: "public",
                table: "AIChatMessages",
                newName: "SenderIsEmo");

            migrationBuilder.RenameColumn(
                name: "is_read",
                schema: "public",
                table: "AIChatMessages",
                newName: "IsRead");

            migrationBuilder.RenameColumn(
                name: "created_date",
                schema: "public",
                table: "AIChatMessages",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "block_number",
                schema: "public",
                table: "AIChatMessages",
                newName: "BlockNumber");

            migrationBuilder.RenameIndex(
                name: "ix_ai_chat_messages_session_id",
                schema: "public",
                table: "AIChatMessages",
                newName: "IX_AIChatMessages_SessionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Messages",
                schema: "public",
                table: "Messages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorPatients",
                schema: "public",
                table: "DoctorPatients",
                column: "BookingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AIChatSessions",
                schema: "public",
                table: "AIChatSessions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AIChatMessages",
                schema: "public",
                table: "AIChatMessages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AIChatMessages_AIChatSessions_SessionId",
                schema: "public",
                table: "AIChatMessages",
                column: "SessionId",
                principalSchema: "public",
                principalTable: "AIChatSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
