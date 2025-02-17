using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "DoctorProfiles",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    Specialty = table.Column<string>(type: "text", nullable: false),
                    Qualifications = table.Column<string>(type: "text", nullable: false),
                    YearsOfExperience = table.Column<int>(type: "integer", nullable: false),
                    Bio = table.Column<string>(type: "text", nullable: false),
                    Rating = table.Column<float>(type: "real", nullable: false),
                    TotalReviews = table.Column<int>(type: "integer", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MentalDisorders",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentalDisorders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatientProfiles",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    Allergies = table.Column<string>(type: "text", nullable: true),
                    PersonalityTraits = table.Column<int>(type: "integer", nullable: false),
                    MedicalHistoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhysicalSymptoms",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicalSymptoms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpecificMentalDisorders",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MentalDisorderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecificMentalDisorders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecificMentalDisorders_MentalDisorders_MentalDisorderId",
                        column: x => x.MentalDisorderId,
                        principalSchema: "public",
                        principalTable: "MentalDisorders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalHistories",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DiagnosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalHistories_PatientProfiles_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalHistoryPhysicalSymptom",
                schema: "public",
                columns: table => new
                {
                    MedicalHistoriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhysicalSymptomsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalHistoryPhysicalSymptom", x => new { x.MedicalHistoriesId, x.PhysicalSymptomsId });
                    table.ForeignKey(
                        name: "FK_MedicalHistoryPhysicalSymptom_MedicalHistories_MedicalHisto~",
                        column: x => x.MedicalHistoriesId,
                        principalSchema: "public",
                        principalTable: "MedicalHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalHistoryPhysicalSymptom_PhysicalSymptoms_PhysicalSymp~",
                        column: x => x.PhysicalSymptomsId,
                        principalSchema: "public",
                        principalTable: "PhysicalSymptoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalHistorySpecificMentalDisorder",
                schema: "public",
                columns: table => new
                {
                    MedicalHistoriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecificMentalDisordersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalHistorySpecificMentalDisorder", x => new { x.MedicalHistoriesId, x.SpecificMentalDisordersId });
                    table.ForeignKey(
                        name: "FK_MedicalHistorySpecificMentalDisorder_MedicalHistories_Medic~",
                        column: x => x.MedicalHistoriesId,
                        principalSchema: "public",
                        principalTable: "MedicalHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalHistorySpecificMentalDisorder_SpecificMentalDisorder~",
                        column: x => x.SpecificMentalDisordersId,
                        principalSchema: "public",
                        principalTable: "SpecificMentalDisorders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalRecords",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicalHistoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalRecords_DoctorProfiles_DoctorProfileId",
                        column: x => x.DoctorProfileId,
                        principalSchema: "public",
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalRecords_MedicalHistories_MedicalHistoryId",
                        column: x => x.MedicalHistoryId,
                        principalSchema: "public",
                        principalTable: "MedicalHistories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MedicalRecords_PatientProfiles_PatientProfileId",
                        column: x => x.PatientProfileId,
                        principalSchema: "public",
                        principalTable: "PatientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalRecordSpecificMentalDisorder",
                schema: "public",
                columns: table => new
                {
                    MedicalRecordsId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecificMentalDisordersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRecordSpecificMentalDisorder", x => new { x.MedicalRecordsId, x.SpecificMentalDisordersId });
                    table.ForeignKey(
                        name: "FK_MedicalRecordSpecificMentalDisorder_MedicalRecords_MedicalR~",
                        column: x => x.MedicalRecordsId,
                        principalSchema: "public",
                        principalTable: "MedicalRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalRecordSpecificMentalDisorder_SpecificMentalDisorders~",
                        column: x => x.SpecificMentalDisordersId,
                        principalSchema: "public",
                        principalTable: "SpecificMentalDisorders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_PatientId",
                schema: "public",
                table: "MedicalHistories",
                column: "PatientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistoryPhysicalSymptom_PhysicalSymptomsId",
                schema: "public",
                table: "MedicalHistoryPhysicalSymptom",
                column: "PhysicalSymptomsId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistorySpecificMentalDisorder_SpecificMentalDisorder~",
                schema: "public",
                table: "MedicalHistorySpecificMentalDisorder",
                column: "SpecificMentalDisordersId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_DoctorProfileId",
                schema: "public",
                table: "MedicalRecords",
                column: "DoctorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_MedicalHistoryId",
                schema: "public",
                table: "MedicalRecords",
                column: "MedicalHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PatientProfileId",
                schema: "public",
                table: "MedicalRecords",
                column: "PatientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecordSpecificMentalDisorder_SpecificMentalDisorders~",
                schema: "public",
                table: "MedicalRecordSpecificMentalDisorder",
                column: "SpecificMentalDisordersId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecificMentalDisorders_MentalDisorderId",
                schema: "public",
                table: "SpecificMentalDisorders",
                column: "MentalDisorderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalHistoryPhysicalSymptom",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MedicalHistorySpecificMentalDisorder",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MedicalRecordSpecificMentalDisorder",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PhysicalSymptoms",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MedicalRecords",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SpecificMentalDisorders",
                schema: "public");

            migrationBuilder.DropTable(
                name: "DoctorProfiles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MedicalHistories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MentalDisorders",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PatientProfiles",
                schema: "public");
        }
    }
}
