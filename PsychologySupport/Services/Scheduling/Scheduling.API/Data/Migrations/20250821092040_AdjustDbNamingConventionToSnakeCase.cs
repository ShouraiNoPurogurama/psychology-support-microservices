using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheduling.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdjustDbNamingConventionToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleActivities_Sessions_SessionId",
                table: "ScheduleActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleFeedbacks_Schedules_ScheduleId",
                table: "ScheduleFeedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Schedules_ScheduleId",
                table: "Sessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sessions",
                table: "Sessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Schedules",
                table: "Schedules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bookings",
                table: "Bookings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeSlotTemplates",
                table: "TimeSlotTemplates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduleFeedbacks",
                table: "ScheduleFeedbacks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduleActivities",
                table: "ScheduleActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorSlotDurations",
                table: "DoctorSlotDurations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorAvailabilities",
                table: "DoctorAvailabilities");

            migrationBuilder.RenameTable(
                name: "Sessions",
                newName: "sessions");

            migrationBuilder.RenameTable(
                name: "Schedules",
                newName: "schedules");

            migrationBuilder.RenameTable(
                name: "Bookings",
                newName: "bookings");

            migrationBuilder.RenameTable(
                name: "TimeSlotTemplates",
                newName: "time_slot_templates");

            migrationBuilder.RenameTable(
                name: "ScheduleFeedbacks",
                newName: "schedule_feedbacks");

            migrationBuilder.RenameTable(
                name: "ScheduleActivities",
                newName: "schedule_activities");

            migrationBuilder.RenameTable(
                name: "DoctorSlotDurations",
                newName: "doctor_slot_durations");

            migrationBuilder.RenameTable(
                name: "DoctorAvailabilities",
                newName: "doctor_availabilities");

            migrationBuilder.RenameColumn(
                name: "Purpose",
                table: "sessions",
                newName: "purpose");

            migrationBuilder.RenameColumn(
                name: "Order",
                table: "sessions",
                newName: "order");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "sessions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "sessions",
                newName: "start_date");

            migrationBuilder.RenameColumn(
                name: "ScheduleId",
                table: "sessions",
                newName: "schedule_id");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "sessions",
                newName: "end_date");

            migrationBuilder.RenameIndex(
                name: "IX_Sessions_ScheduleId",
                table: "sessions",
                newName: "ix_sessions_schedule_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "schedules",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "schedules",
                newName: "start_date");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "schedules",
                newName: "patient_id");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "schedules",
                newName: "end_date");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "schedules",
                newName: "doctor_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "bookings",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "bookings",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "bookings",
                newName: "duration");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "bookings",
                newName: "date");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "bookings",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "bookings",
                newName: "start_time");

            migrationBuilder.RenameColumn(
                name: "PromoCodeId",
                table: "bookings",
                newName: "promo_code_id");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "bookings",
                newName: "patient_id");

            migrationBuilder.RenameColumn(
                name: "GiftCodeId",
                table: "bookings",
                newName: "gift_code_id");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "bookings",
                newName: "doctor_id");

            migrationBuilder.RenameColumn(
                name: "BookingCode",
                table: "bookings",
                newName: "booking_code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "time_slot_templates",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "time_slot_templates",
                newName: "start_time");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "time_slot_templates",
                newName: "end_time");

            migrationBuilder.RenameColumn(
                name: "DayOfWeek",
                table: "time_slot_templates",
                newName: "day_of_week");

            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "schedule_feedbacks",
                newName: "rating");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "schedule_feedbacks",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "schedule_feedbacks",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ScheduleId",
                table: "schedule_feedbacks",
                newName: "schedule_id");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "schedule_feedbacks",
                newName: "patient_id");

            migrationBuilder.RenameColumn(
                name: "FeedbackDate",
                table: "schedule_feedbacks",
                newName: "feedback_date");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleFeedbacks_ScheduleId",
                table: "schedule_feedbacks",
                newName: "ix_schedule_feedbacks_schedule_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "schedule_activities",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "schedule_activities",
                newName: "duration");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "schedule_activities",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "schedule_activities",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TimeRange",
                table: "schedule_activities",
                newName: "time_range");

            migrationBuilder.RenameColumn(
                name: "TherapeuticActivityId",
                table: "schedule_activities",
                newName: "therapeutic_activity_id");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "schedule_activities",
                newName: "session_id");

            migrationBuilder.RenameColumn(
                name: "PhysicalActivityId",
                table: "schedule_activities",
                newName: "physical_activity_id");

            migrationBuilder.RenameColumn(
                name: "FoodActivityId",
                table: "schedule_activities",
                newName: "food_activity_id");

            migrationBuilder.RenameColumn(
                name: "EntertainmentActivityId",
                table: "schedule_activities",
                newName: "entertainment_activity_id");

            migrationBuilder.RenameColumn(
                name: "DateNumber",
                table: "schedule_activities",
                newName: "date_number");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleActivities_SessionId",
                table: "schedule_activities",
                newName: "ix_schedule_activities_session_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "doctor_slot_durations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SlotsPerDay",
                table: "doctor_slot_durations",
                newName: "slots_per_day");

            migrationBuilder.RenameColumn(
                name: "SlotDuration",
                table: "doctor_slot_durations",
                newName: "slot_duration");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "doctor_slot_durations",
                newName: "doctor_id");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "doctor_availabilities",
                newName: "date");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "doctor_availabilities",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "doctor_availabilities",
                newName: "start_time");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "doctor_availabilities",
                newName: "doctor_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sessions",
                table: "sessions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_schedules",
                table: "schedules",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_bookings",
                table: "bookings",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_time_slot_templates",
                table: "time_slot_templates",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_schedule_feedbacks",
                table: "schedule_feedbacks",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_schedule_activities",
                table: "schedule_activities",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_doctor_slot_durations",
                table: "doctor_slot_durations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_doctor_availabilities",
                table: "doctor_availabilities",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_schedule_activities_sessions_session_id",
                table: "schedule_activities",
                column: "session_id",
                principalTable: "sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_schedule_feedbacks_schedules_schedule_id",
                table: "schedule_feedbacks",
                column: "schedule_id",
                principalTable: "schedules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_sessions_schedules_schedule_id",
                table: "sessions",
                column: "schedule_id",
                principalTable: "schedules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_schedule_activities_sessions_session_id",
                table: "schedule_activities");

            migrationBuilder.DropForeignKey(
                name: "fk_schedule_feedbacks_schedules_schedule_id",
                table: "schedule_feedbacks");

            migrationBuilder.DropForeignKey(
                name: "fk_sessions_schedules_schedule_id",
                table: "sessions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_sessions",
                table: "sessions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_schedules",
                table: "schedules");

            migrationBuilder.DropPrimaryKey(
                name: "pk_bookings",
                table: "bookings");

            migrationBuilder.DropPrimaryKey(
                name: "pk_time_slot_templates",
                table: "time_slot_templates");

            migrationBuilder.DropPrimaryKey(
                name: "pk_schedule_feedbacks",
                table: "schedule_feedbacks");

            migrationBuilder.DropPrimaryKey(
                name: "pk_schedule_activities",
                table: "schedule_activities");

            migrationBuilder.DropPrimaryKey(
                name: "pk_doctor_slot_durations",
                table: "doctor_slot_durations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_doctor_availabilities",
                table: "doctor_availabilities");

            migrationBuilder.RenameTable(
                name: "sessions",
                newName: "Sessions");

            migrationBuilder.RenameTable(
                name: "schedules",
                newName: "Schedules");

            migrationBuilder.RenameTable(
                name: "bookings",
                newName: "Bookings");

            migrationBuilder.RenameTable(
                name: "time_slot_templates",
                newName: "TimeSlotTemplates");

            migrationBuilder.RenameTable(
                name: "schedule_feedbacks",
                newName: "ScheduleFeedbacks");

            migrationBuilder.RenameTable(
                name: "schedule_activities",
                newName: "ScheduleActivities");

            migrationBuilder.RenameTable(
                name: "doctor_slot_durations",
                newName: "DoctorSlotDurations");

            migrationBuilder.RenameTable(
                name: "doctor_availabilities",
                newName: "DoctorAvailabilities");

            migrationBuilder.RenameColumn(
                name: "purpose",
                table: "Sessions",
                newName: "Purpose");

            migrationBuilder.RenameColumn(
                name: "order",
                table: "Sessions",
                newName: "Order");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Sessions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "start_date",
                table: "Sessions",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "schedule_id",
                table: "Sessions",
                newName: "ScheduleId");

            migrationBuilder.RenameColumn(
                name: "end_date",
                table: "Sessions",
                newName: "EndDate");

            migrationBuilder.RenameIndex(
                name: "ix_sessions_schedule_id",
                table: "Sessions",
                newName: "IX_Sessions_ScheduleId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Schedules",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "start_date",
                table: "Schedules",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "patient_id",
                table: "Schedules",
                newName: "PatientId");

            migrationBuilder.RenameColumn(
                name: "end_date",
                table: "Schedules",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "doctor_id",
                table: "Schedules",
                newName: "DoctorId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Bookings",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "price",
                table: "Bookings",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "duration",
                table: "Bookings",
                newName: "Duration");

            migrationBuilder.RenameColumn(
                name: "date",
                table: "Bookings",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Bookings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "start_time",
                table: "Bookings",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "promo_code_id",
                table: "Bookings",
                newName: "PromoCodeId");

            migrationBuilder.RenameColumn(
                name: "patient_id",
                table: "Bookings",
                newName: "PatientId");

            migrationBuilder.RenameColumn(
                name: "gift_code_id",
                table: "Bookings",
                newName: "GiftCodeId");

            migrationBuilder.RenameColumn(
                name: "doctor_id",
                table: "Bookings",
                newName: "DoctorId");

            migrationBuilder.RenameColumn(
                name: "booking_code",
                table: "Bookings",
                newName: "BookingCode");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "TimeSlotTemplates",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "start_time",
                table: "TimeSlotTemplates",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "end_time",
                table: "TimeSlotTemplates",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "day_of_week",
                table: "TimeSlotTemplates",
                newName: "DayOfWeek");

            migrationBuilder.RenameColumn(
                name: "rating",
                table: "ScheduleFeedbacks",
                newName: "Rating");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "ScheduleFeedbacks",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ScheduleFeedbacks",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "schedule_id",
                table: "ScheduleFeedbacks",
                newName: "ScheduleId");

            migrationBuilder.RenameColumn(
                name: "patient_id",
                table: "ScheduleFeedbacks",
                newName: "PatientId");

            migrationBuilder.RenameColumn(
                name: "feedback_date",
                table: "ScheduleFeedbacks",
                newName: "FeedbackDate");

            migrationBuilder.RenameIndex(
                name: "ix_schedule_feedbacks_schedule_id",
                table: "ScheduleFeedbacks",
                newName: "IX_ScheduleFeedbacks_ScheduleId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "ScheduleActivities",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "duration",
                table: "ScheduleActivities",
                newName: "Duration");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "ScheduleActivities",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ScheduleActivities",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "time_range",
                table: "ScheduleActivities",
                newName: "TimeRange");

            migrationBuilder.RenameColumn(
                name: "therapeutic_activity_id",
                table: "ScheduleActivities",
                newName: "TherapeuticActivityId");

            migrationBuilder.RenameColumn(
                name: "session_id",
                table: "ScheduleActivities",
                newName: "SessionId");

            migrationBuilder.RenameColumn(
                name: "physical_activity_id",
                table: "ScheduleActivities",
                newName: "PhysicalActivityId");

            migrationBuilder.RenameColumn(
                name: "food_activity_id",
                table: "ScheduleActivities",
                newName: "FoodActivityId");

            migrationBuilder.RenameColumn(
                name: "entertainment_activity_id",
                table: "ScheduleActivities",
                newName: "EntertainmentActivityId");

            migrationBuilder.RenameColumn(
                name: "date_number",
                table: "ScheduleActivities",
                newName: "DateNumber");

            migrationBuilder.RenameIndex(
                name: "ix_schedule_activities_session_id",
                table: "ScheduleActivities",
                newName: "IX_ScheduleActivities_SessionId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "DoctorSlotDurations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "slots_per_day",
                table: "DoctorSlotDurations",
                newName: "SlotsPerDay");

            migrationBuilder.RenameColumn(
                name: "slot_duration",
                table: "DoctorSlotDurations",
                newName: "SlotDuration");

            migrationBuilder.RenameColumn(
                name: "doctor_id",
                table: "DoctorSlotDurations",
                newName: "DoctorId");

            migrationBuilder.RenameColumn(
                name: "date",
                table: "DoctorAvailabilities",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "DoctorAvailabilities",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "start_time",
                table: "DoctorAvailabilities",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "doctor_id",
                table: "DoctorAvailabilities",
                newName: "DoctorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sessions",
                table: "Sessions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Schedules",
                table: "Schedules",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bookings",
                table: "Bookings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeSlotTemplates",
                table: "TimeSlotTemplates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduleFeedbacks",
                table: "ScheduleFeedbacks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduleActivities",
                table: "ScheduleActivities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorSlotDurations",
                table: "DoctorSlotDurations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorAvailabilities",
                table: "DoctorAvailabilities",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleActivities_Sessions_SessionId",
                table: "ScheduleActivities",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleFeedbacks_Schedules_ScheduleId",
                table: "ScheduleFeedbacks",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Schedules_ScheduleId",
                table: "Sessions",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
