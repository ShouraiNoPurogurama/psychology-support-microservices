using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Public.Migrations
{
    /// <inheritdoc />
    public partial class Baseline_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");
            //
            // migrationBuilder.CreateTable(
            //     name: "doctor_profiles",
            //     schema: "public",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         user_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         full_name = table.Column<string>(type: "text", nullable: false),
            //         gender = table.Column<string>(type: "text", nullable: false),
            //         qualifications = table.Column<string>(type: "text", nullable: true),
            //         years_of_experience = table.Column<int>(type: "integer", nullable: false),
            //         bio = table.Column<string>(type: "text", nullable: true),
            //         rating = table.Column<float>(type: "real", nullable: false),
            //         total_reviews = table.Column<int>(type: "integer", nullable: false),
            //         address = table.Column<string>(type: "text", nullable: false),
            //         email = table.Column<string>(type: "text", nullable: false),
            //         phone_number = table.Column<string>(type: "text", nullable: true),
            //         created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         created_by = table.Column<string>(type: "text", nullable: true),
            //         last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         last_modified_by = table.Column<string>(type: "text", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("pk_doctor_profiles", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "industries",
            //     schema: "public",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         industry_name = table.Column<string>(type: "text", nullable: false),
            //         description = table.Column<string>(type: "text", nullable: false),
            //         created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         created_by = table.Column<string>(type: "text", nullable: true),
            //         last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         last_modified_by = table.Column<string>(type: "text", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("pk_industries", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "mental_disorders",
            //     schema: "public",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         name = table.Column<string>(type: "text", nullable: false),
            //         description = table.Column<string>(type: "text", nullable: false),
            //         created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         created_by = table.Column<string>(type: "text", nullable: true),
            //         last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         last_modified_by = table.Column<string>(type: "text", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("pk_mental_disorders", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "physical_symptoms",
            //     schema: "public",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         name = table.Column<string>(type: "text", nullable: false),
            //         description = table.Column<string>(type: "text", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("pk_physical_symptoms", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "specialties",
            //     schema: "public",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         name = table.Column<string>(type: "text", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("pk_specialties", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "jobs",
            //     schema: "public",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         industry_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         job_title = table.Column<string>(type: "text", nullable: false),
            //         education_level = table.Column<string>(type: "VARCHAR(30)", nullable: false),
            //         created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         created_by = table.Column<string>(type: "text", nullable: true),
            //         last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         last_modified_by = table.Column<string>(type: "text", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("pk_jobs", x => x.id);
            //         table.ForeignKey(
            //             name: "fk_jobs_industries_industry_id",
            //             column: x => x.industry_id,
            //             principalSchema: "public",
            //             principalTable: "industries",
            //             principalColumn: "id",
            //             onDelete: ReferentialAction.Cascade);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "specific_mental_disorders",
            //     schema: "public",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         mental_disorder_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         name = table.Column<string>(type: "text", nullable: false),
            //         description = table.Column<string>(type: "text", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("pk_specific_mental_disorders", x => x.id);
            //         table.ForeignKey(
            //             name: "fk_specific_mental_disorders_mental_disorders_mental_disorder_",
            //             column: x => x.mental_disorder_id,
            //             principalSchema: "public",
            //             principalTable: "mental_disorders",
            //             principalColumn: "id",
            //             onDelete: ReferentialAction.Cascade);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "doctor_profile_specialty",
            //     schema: "public",
            //     columns: table => new
            //     {
            //         doctor_profiles_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         specialties_id = table.Column<Guid>(type: "uuid", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("pk_doctor_profile_specialty", x => new { x.doctor_profiles_id, x.specialties_id });
            //         table.ForeignKey(
            //             name: "fk_doctor_profile_specialty_doctor_profiles_doctor_profiles_id",
            //             column: x => x.doctor_profiles_id,
            //             principalSchema: "public",
            //             principalTable: "doctor_profiles",
            //             principalColumn: "id",
            //             onDelete: ReferentialAction.Cascade);
            //         table.ForeignKey(
            //             name: "fk_doctor_profile_specialty_specialties_specialties_id",
            //             column: x => x.specialties_id,
            //             principalSchema: "public",
            //             principalTable: "specialties",
            //             principalColumn: "id",
            //             onDelete: ReferentialAction.Cascade);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "patient_profiles",
            //     schema: "public",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         user_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         allergies = table.Column<string>(type: "text", nullable: true),
            //         personality_traits = table.Column<string>(type: "text", nullable: false),
            //         medical_history_id = table.Column<Guid>(type: "uuid", nullable: true),
            //         job_id = table.Column<Guid>(type: "uuid", nullable: true),
            //         is_profile_completed = table.Column<bool>(type: "boolean", nullable: false),
            //         created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         created_by = table.Column<string>(type: "text", nullable: true),
            //         last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         last_modified_by = table.Column<string>(type: "text", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("pk_patient_profiles", x => x.id);
            //         table.ForeignKey(
            //             name: "fk_patient_profiles_jobs_job_id",
            //             column: x => x.job_id,
            //             principalSchema: "public",
            //             principalTable: "jobs",
            //             principalColumn: "id");
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "medical_histories",
            //     schema: "public",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         patient_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         description = table.Column<string>(type: "text", nullable: false),
            //         diagnosed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            //         created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         created_by = table.Column<string>(type: "text", nullable: true),
            //         last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         last_modified_by = table.Column<string>(type: "text", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("pk_medical_histories", x => x.id);
            //         table.ForeignKey(
            //             name: "fk_medical_histories_patient_profiles_patient_id",
            //             column: x => x.patient_id,
            //             principalSchema: "public",
            //             principalTable: "patient_profiles",
            //             principalColumn: "id",
            //             onDelete: ReferentialAction.Cascade);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "medical_history_physical_symptom",
            //     schema: "public",
            //     columns: table => new
            //     {
            //         medical_histories_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         physical_symptoms_id = table.Column<Guid>(type: "uuid", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("pk_medical_history_physical_symptom", x => new { x.medical_histories_id, x.physical_symptoms_id });
            //         table.ForeignKey(
            //             name: "fk_medical_history_physical_symptom_medical_histories_medical_",
            //             column: x => x.medical_histories_id,
            //             principalSchema: "public",
            //             principalTable: "medical_histories",
            //             principalColumn: "id",
            //             onDelete: ReferentialAction.Cascade);
            //         table.ForeignKey(
            //             name: "fk_medical_history_physical_symptom_physical_symptoms_physical",
            //             column: x => x.physical_symptoms_id,
            //             principalSchema: "public",
            //             principalTable: "physical_symptoms",
            //             principalColumn: "id",
            //             onDelete: ReferentialAction.Cascade);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "medical_history_specific_mental_disorder",
            //     schema: "public",
            //     columns: table => new
            //     {
            //         medical_histories_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         specific_mental_disorders_id = table.Column<Guid>(type: "uuid", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("pk_medical_history_specific_mental_disorder", x => new { x.medical_histories_id, x.specific_mental_disorders_id });
            //         table.ForeignKey(
            //             name: "fk_medical_history_specific_mental_disorder_medical_histories_",
            //             column: x => x.medical_histories_id,
            //             principalSchema: "public",
            //             principalTable: "medical_histories",
            //             principalColumn: "id",
            //             onDelete: ReferentialAction.Cascade);
            //         table.ForeignKey(
            //             name: "fk_medical_history_specific_mental_disorder_specific_mental_di",
            //             column: x => x.specific_mental_disorders_id,
            //             principalSchema: "public",
            //             principalTable: "specific_mental_disorders",
            //             principalColumn: "id",
            //             onDelete: ReferentialAction.Cascade);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "medical_records",
            //     schema: "public",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         patient_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         doctor_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         medical_history_id = table.Column<Guid>(type: "uuid", nullable: true),
            //         notes = table.Column<string>(type: "text", nullable: false),
            //         status = table.Column<string>(type: "VARCHAR(20)", nullable: false),
            //         created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         created_by = table.Column<string>(type: "text", nullable: true),
            //         last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         last_modified_by = table.Column<string>(type: "text", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("pk_medical_records", x => x.id);
            //         table.ForeignKey(
            //             name: "fk_medical_records_doctor_profiles_doctor_profile_id",
            //             column: x => x.doctor_profile_id,
            //             principalSchema: "public",
            //             principalTable: "doctor_profiles",
            //             principalColumn: "id",
            //             onDelete: ReferentialAction.Cascade);
            //         table.ForeignKey(
            //             name: "fk_medical_records_medical_histories_medical_history_id",
            //             column: x => x.medical_history_id,
            //             principalSchema: "public",
            //             principalTable: "medical_histories",
            //             principalColumn: "id");
            //         table.ForeignKey(
            //             name: "fk_medical_records_patient_profiles_patient_profile_id",
            //             column: x => x.patient_profile_id,
            //             principalSchema: "public",
            //             principalTable: "patient_profiles",
            //             principalColumn: "id",
            //             onDelete: ReferentialAction.Cascade);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "medical_record_specific_mental_disorder",
            //     schema: "public",
            //     columns: table => new
            //     {
            //         medical_records_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         specific_mental_disorders_id = table.Column<Guid>(type: "uuid", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("pk_medical_record_specific_mental_disorder", x => new { x.medical_records_id, x.specific_mental_disorders_id });
            //         table.ForeignKey(
            //             name: "fk_medical_record_specific_mental_disorder_medical_records_med",
            //             column: x => x.medical_records_id,
            //             principalSchema: "public",
            //             principalTable: "medical_records",
            //             principalColumn: "id",
            //             onDelete: ReferentialAction.Cascade);
            //         table.ForeignKey(
            //             name: "fk_medical_record_specific_mental_disorder_specific_mental_dis",
            //             column: x => x.specific_mental_disorders_id,
            //             principalSchema: "public",
            //             principalTable: "specific_mental_disorders",
            //             principalColumn: "id",
            //             onDelete: ReferentialAction.Cascade);
            //     });
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_doctor_profile_specialty_specialties_id",
            //     schema: "public",
            //     table: "doctor_profile_specialty",
            //     column: "specialties_id");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_jobs_industry_id",
            //     schema: "public",
            //     table: "jobs",
            //     column: "industry_id");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_medical_histories_patient_id",
            //     schema: "public",
            //     table: "medical_histories",
            //     column: "patient_id",
            //     unique: true);
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_medical_history_physical_symptom_physical_symptoms_id",
            //     schema: "public",
            //     table: "medical_history_physical_symptom",
            //     column: "physical_symptoms_id");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_medical_history_specific_mental_disorder_specific_mental_di",
            //     schema: "public",
            //     table: "medical_history_specific_mental_disorder",
            //     column: "specific_mental_disorders_id");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_medical_record_specific_mental_disorder_specific_mental_dis",
            //     schema: "public",
            //     table: "medical_record_specific_mental_disorder",
            //     column: "specific_mental_disorders_id");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_medical_records_doctor_profile_id",
            //     schema: "public",
            //     table: "medical_records",
            //     column: "doctor_profile_id");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_medical_records_medical_history_id",
            //     schema: "public",
            //     table: "medical_records",
            //     column: "medical_history_id");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_medical_records_patient_profile_id",
            //     schema: "public",
            //     table: "medical_records",
            //     column: "patient_profile_id");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_patient_profiles_job_id",
            //     schema: "public",
            //     table: "patient_profiles",
            //     column: "job_id");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_specific_mental_disorders_mental_disorder_id",
            //     schema: "public",
            //     table: "specific_mental_disorders",
            //     column: "mental_disorder_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "doctor_profile_specialty",
                schema: "public");

            migrationBuilder.DropTable(
                name: "medical_history_physical_symptom",
                schema: "public");

            migrationBuilder.DropTable(
                name: "medical_history_specific_mental_disorder",
                schema: "public");

            migrationBuilder.DropTable(
                name: "medical_record_specific_mental_disorder",
                schema: "public");

            migrationBuilder.DropTable(
                name: "specialties",
                schema: "public");

            migrationBuilder.DropTable(
                name: "physical_symptoms",
                schema: "public");

            migrationBuilder.DropTable(
                name: "medical_records",
                schema: "public");

            migrationBuilder.DropTable(
                name: "specific_mental_disorders",
                schema: "public");

            migrationBuilder.DropTable(
                name: "doctor_profiles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "medical_histories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "mental_disorders",
                schema: "public");

            migrationBuilder.DropTable(
                name: "patient_profiles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "jobs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "industries",
                schema: "public");
        }
    }
}
