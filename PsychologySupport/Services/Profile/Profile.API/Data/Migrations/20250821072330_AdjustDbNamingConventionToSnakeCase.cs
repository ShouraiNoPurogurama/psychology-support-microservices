using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdjustDbNamingConventionToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorProfileSpecialty_DoctorProfiles_DoctorProfilesId",
                schema: "public",
                table: "DoctorProfileSpecialty");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorProfileSpecialty_Specialties_SpecialtiesId",
                schema: "public",
                table: "DoctorProfileSpecialty");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Industries_IndustryId",
                schema: "public",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistories_PatientProfiles_PatientId",
                schema: "public",
                table: "MedicalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistoryPhysicalSymptom_MedicalHistories_MedicalHisto~",
                schema: "public",
                table: "MedicalHistoryPhysicalSymptom");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistoryPhysicalSymptom_PhysicalSymptoms_PhysicalSymp~",
                schema: "public",
                table: "MedicalHistoryPhysicalSymptom");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistorySpecificMentalDisorder_MedicalHistories_Medic~",
                schema: "public",
                table: "MedicalHistorySpecificMentalDisorder");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistorySpecificMentalDisorder_SpecificMentalDisorder~",
                schema: "public",
                table: "MedicalHistorySpecificMentalDisorder");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_DoctorProfiles_DoctorProfileId",
                schema: "public",
                table: "MedicalRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_MedicalHistories_MedicalHistoryId",
                schema: "public",
                table: "MedicalRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_PatientProfiles_PatientProfileId",
                schema: "public",
                table: "MedicalRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecordSpecificMentalDisorder_MedicalRecords_MedicalR~",
                schema: "public",
                table: "MedicalRecordSpecificMentalDisorder");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecordSpecificMentalDisorder_SpecificMentalDisorders~",
                schema: "public",
                table: "MedicalRecordSpecificMentalDisorder");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientProfiles_Jobs_JobId",
                schema: "public",
                table: "PatientProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_SpecificMentalDisorders_MentalDisorders_MentalDisorderId",
                schema: "public",
                table: "SpecificMentalDisorders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Specialties",
                schema: "public",
                table: "Specialties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Jobs",
                schema: "public",
                table: "Jobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Industries",
                schema: "public",
                table: "Industries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SpecificMentalDisorders",
                schema: "public",
                table: "SpecificMentalDisorders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhysicalSymptoms",
                schema: "public",
                table: "PhysicalSymptoms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PatientProfiles",
                schema: "public",
                table: "PatientProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MentalDisorders",
                schema: "public",
                table: "MentalDisorders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicalRecordSpecificMentalDisorder",
                schema: "public",
                table: "MedicalRecordSpecificMentalDisorder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicalRecords",
                schema: "public",
                table: "MedicalRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicalHistorySpecificMentalDisorder",
                schema: "public",
                table: "MedicalHistorySpecificMentalDisorder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicalHistoryPhysicalSymptom",
                schema: "public",
                table: "MedicalHistoryPhysicalSymptom");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicalHistories",
                schema: "public",
                table: "MedicalHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorProfileSpecialty",
                schema: "public",
                table: "DoctorProfileSpecialty");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorProfiles",
                schema: "public",
                table: "DoctorProfiles");

            migrationBuilder.RenameTable(
                name: "Specialties",
                schema: "public",
                newName: "specialties",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Jobs",
                schema: "public",
                newName: "jobs",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Industries",
                schema: "public",
                newName: "industries",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "SpecificMentalDisorders",
                schema: "public",
                newName: "specific_mental_disorders",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "PhysicalSymptoms",
                schema: "public",
                newName: "physical_symptoms",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "PatientProfiles",
                schema: "public",
                newName: "patient_profiles",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "MentalDisorders",
                schema: "public",
                newName: "mental_disorders",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "MedicalRecordSpecificMentalDisorder",
                schema: "public",
                newName: "medical_record_specific_mental_disorder",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "MedicalRecords",
                schema: "public",
                newName: "medical_records",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "MedicalHistorySpecificMentalDisorder",
                schema: "public",
                newName: "medical_history_specific_mental_disorder",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "MedicalHistoryPhysicalSymptom",
                schema: "public",
                newName: "medical_history_physical_symptom",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "MedicalHistories",
                schema: "public",
                newName: "medical_histories",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "DoctorProfileSpecialty",
                schema: "public",
                newName: "doctor_profile_specialty",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "DoctorProfiles",
                schema: "public",
                newName: "doctor_profiles",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "public",
                table: "specialties",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "specialties",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "jobs",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "jobs",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                schema: "public",
                table: "jobs",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "JobTitle",
                schema: "public",
                table: "jobs",
                newName: "job_title");

            migrationBuilder.RenameColumn(
                name: "IndustryId",
                schema: "public",
                table: "jobs",
                newName: "industry_id");

            migrationBuilder.RenameColumn(
                name: "EducationLevel",
                schema: "public",
                table: "jobs",
                newName: "education_level");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "public",
                table: "jobs",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "public",
                table: "jobs",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_IndustryId",
                schema: "public",
                table: "jobs",
                newName: "ix_jobs_industry_id");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "industries",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "industries",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "industries",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                schema: "public",
                table: "industries",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IndustryName",
                schema: "public",
                table: "industries",
                newName: "industry_name");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "public",
                table: "industries",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "public",
                table: "industries",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "public",
                table: "specific_mental_disorders",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "specific_mental_disorders",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "specific_mental_disorders",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "MentalDisorderId",
                schema: "public",
                table: "specific_mental_disorders",
                newName: "mental_disorder_id");

            migrationBuilder.RenameIndex(
                name: "IX_SpecificMentalDisorders_MentalDisorderId",
                schema: "public",
                table: "specific_mental_disorders",
                newName: "ix_specific_mental_disorders_mental_disorder_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "public",
                table: "physical_symptoms",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "physical_symptoms",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "physical_symptoms",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Allergies",
                schema: "public",
                table: "patient_profiles",
                newName: "allergies");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "patient_profiles",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "public",
                table: "patient_profiles",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "MedicalHistoryId",
                schema: "public",
                table: "patient_profiles",
                newName: "medical_history_id");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "patient_profiles",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                schema: "public",
                table: "patient_profiles",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "JobId",
                schema: "public",
                table: "patient_profiles",
                newName: "job_id");

            migrationBuilder.RenameColumn(
                name: "IsProfileCompleted",
                schema: "public",
                table: "patient_profiles",
                newName: "is_profile_completed");

            migrationBuilder.RenameColumn(
                name: "FullName",
                schema: "public",
                table: "patient_profiles",
                newName: "full_name");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "public",
                table: "patient_profiles",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "public",
                table: "patient_profiles",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "BirthDate",
                schema: "public",
                table: "patient_profiles",
                newName: "birth_date");

            migrationBuilder.RenameIndex(
                name: "IX_PatientProfiles_JobId",
                schema: "public",
                table: "patient_profiles",
                newName: "ix_patient_profiles_job_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "public",
                table: "mental_disorders",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "mental_disorders",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "mental_disorders",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "mental_disorders",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                schema: "public",
                table: "mental_disorders",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "public",
                table: "mental_disorders",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "public",
                table: "mental_disorders",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "SpecificMentalDisordersId",
                schema: "public",
                table: "medical_record_specific_mental_disorder",
                newName: "specific_mental_disorders_id");

            migrationBuilder.RenameColumn(
                name: "MedicalRecordsId",
                schema: "public",
                table: "medical_record_specific_mental_disorder",
                newName: "medical_records_id");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalRecordSpecificMentalDisorder_SpecificMentalDisorders~",
                schema: "public",
                table: "medical_record_specific_mental_disorder",
                newName: "ix_medical_record_specific_mental_disorder_specific_mental_dis");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "public",
                table: "medical_records",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Notes",
                schema: "public",
                table: "medical_records",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "medical_records",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PatientProfileId",
                schema: "public",
                table: "medical_records",
                newName: "patient_profile_id");

            migrationBuilder.RenameColumn(
                name: "MedicalHistoryId",
                schema: "public",
                table: "medical_records",
                newName: "medical_history_id");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "medical_records",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                schema: "public",
                table: "medical_records",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "DoctorProfileId",
                schema: "public",
                table: "medical_records",
                newName: "doctor_profile_id");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "public",
                table: "medical_records",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "public",
                table: "medical_records",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalRecords_PatientProfileId",
                schema: "public",
                table: "medical_records",
                newName: "ix_medical_records_patient_profile_id");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalRecords_MedicalHistoryId",
                schema: "public",
                table: "medical_records",
                newName: "ix_medical_records_medical_history_id");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalRecords_DoctorProfileId",
                schema: "public",
                table: "medical_records",
                newName: "ix_medical_records_doctor_profile_id");

            migrationBuilder.RenameColumn(
                name: "SpecificMentalDisordersId",
                schema: "public",
                table: "medical_history_specific_mental_disorder",
                newName: "specific_mental_disorders_id");

            migrationBuilder.RenameColumn(
                name: "MedicalHistoriesId",
                schema: "public",
                table: "medical_history_specific_mental_disorder",
                newName: "medical_histories_id");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalHistorySpecificMentalDisorder_SpecificMentalDisorder~",
                schema: "public",
                table: "medical_history_specific_mental_disorder",
                newName: "ix_medical_history_specific_mental_disorder_specific_mental_di");

            migrationBuilder.RenameColumn(
                name: "PhysicalSymptomsId",
                schema: "public",
                table: "medical_history_physical_symptom",
                newName: "physical_symptoms_id");

            migrationBuilder.RenameColumn(
                name: "MedicalHistoriesId",
                schema: "public",
                table: "medical_history_physical_symptom",
                newName: "medical_histories_id");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalHistoryPhysicalSymptom_PhysicalSymptomsId",
                schema: "public",
                table: "medical_history_physical_symptom",
                newName: "ix_medical_history_physical_symptom_physical_symptoms_id");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "medical_histories",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "medical_histories",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                schema: "public",
                table: "medical_histories",
                newName: "patient_id");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "medical_histories",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                schema: "public",
                table: "medical_histories",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "DiagnosedAt",
                schema: "public",
                table: "medical_histories",
                newName: "diagnosed_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "public",
                table: "medical_histories",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "public",
                table: "medical_histories",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalHistories_PatientId",
                schema: "public",
                table: "medical_histories",
                newName: "ix_medical_histories_patient_id");

            migrationBuilder.RenameColumn(
                name: "SpecialtiesId",
                schema: "public",
                table: "doctor_profile_specialty",
                newName: "specialties_id");

            migrationBuilder.RenameColumn(
                name: "DoctorProfilesId",
                schema: "public",
                table: "doctor_profile_specialty",
                newName: "doctor_profiles_id");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorProfileSpecialty_SpecialtiesId",
                schema: "public",
                table: "doctor_profile_specialty",
                newName: "ix_doctor_profile_specialty_specialties_id");

            migrationBuilder.RenameColumn(
                name: "Rating",
                schema: "public",
                table: "doctor_profiles",
                newName: "rating");

            migrationBuilder.RenameColumn(
                name: "Qualifications",
                schema: "public",
                table: "doctor_profiles",
                newName: "qualifications");

            migrationBuilder.RenameColumn(
                name: "Bio",
                schema: "public",
                table: "doctor_profiles",
                newName: "bio");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "doctor_profiles",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "YearsOfExperience",
                schema: "public",
                table: "doctor_profiles",
                newName: "years_of_experience");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "public",
                table: "doctor_profiles",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "TotalReviews",
                schema: "public",
                table: "doctor_profiles",
                newName: "total_reviews");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "doctor_profiles",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                schema: "public",
                table: "doctor_profiles",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "FullName",
                schema: "public",
                table: "doctor_profiles",
                newName: "full_name");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "public",
                table: "doctor_profiles",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "public",
                table: "doctor_profiles",
                newName: "created_at");

            migrationBuilder.AddPrimaryKey(
                name: "pk_specialties",
                schema: "public",
                table: "specialties",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_jobs",
                schema: "public",
                table: "jobs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_industries",
                schema: "public",
                table: "industries",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_specific_mental_disorders",
                schema: "public",
                table: "specific_mental_disorders",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_physical_symptoms",
                schema: "public",
                table: "physical_symptoms",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_patient_profiles",
                schema: "public",
                table: "patient_profiles",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_mental_disorders",
                schema: "public",
                table: "mental_disorders",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_medical_record_specific_mental_disorder",
                schema: "public",
                table: "medical_record_specific_mental_disorder",
                columns: new[] { "medical_records_id", "specific_mental_disorders_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_medical_records",
                schema: "public",
                table: "medical_records",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_medical_history_specific_mental_disorder",
                schema: "public",
                table: "medical_history_specific_mental_disorder",
                columns: new[] { "medical_histories_id", "specific_mental_disorders_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_medical_history_physical_symptom",
                schema: "public",
                table: "medical_history_physical_symptom",
                columns: new[] { "medical_histories_id", "physical_symptoms_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_medical_histories",
                schema: "public",
                table: "medical_histories",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_doctor_profile_specialty",
                schema: "public",
                table: "doctor_profile_specialty",
                columns: new[] { "doctor_profiles_id", "specialties_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_doctor_profiles",
                schema: "public",
                table: "doctor_profiles",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_doctor_profile_specialty_doctor_profiles_doctor_profiles_id",
                schema: "public",
                table: "doctor_profile_specialty",
                column: "doctor_profiles_id",
                principalSchema: "public",
                principalTable: "doctor_profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_doctor_profile_specialty_specialties_specialties_id",
                schema: "public",
                table: "doctor_profile_specialty",
                column: "specialties_id",
                principalSchema: "public",
                principalTable: "specialties",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_jobs_industries_industry_id",
                schema: "public",
                table: "jobs",
                column: "industry_id",
                principalSchema: "public",
                principalTable: "industries",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_medical_histories_patient_profiles_patient_id",
                schema: "public",
                table: "medical_histories",
                column: "patient_id",
                principalSchema: "public",
                principalTable: "patient_profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_medical_history_physical_symptom_medical_histories_medical_",
                schema: "public",
                table: "medical_history_physical_symptom",
                column: "medical_histories_id",
                principalSchema: "public",
                principalTable: "medical_histories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_medical_history_physical_symptom_physical_symptoms_physical",
                schema: "public",
                table: "medical_history_physical_symptom",
                column: "physical_symptoms_id",
                principalSchema: "public",
                principalTable: "physical_symptoms",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_medical_history_specific_mental_disorder_medical_histories_",
                schema: "public",
                table: "medical_history_specific_mental_disorder",
                column: "medical_histories_id",
                principalSchema: "public",
                principalTable: "medical_histories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_medical_history_specific_mental_disorder_specific_mental_di",
                schema: "public",
                table: "medical_history_specific_mental_disorder",
                column: "specific_mental_disorders_id",
                principalSchema: "public",
                principalTable: "specific_mental_disorders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_medical_record_specific_mental_disorder_medical_records_med",
                schema: "public",
                table: "medical_record_specific_mental_disorder",
                column: "medical_records_id",
                principalSchema: "public",
                principalTable: "medical_records",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_medical_record_specific_mental_disorder_specific_mental_dis",
                schema: "public",
                table: "medical_record_specific_mental_disorder",
                column: "specific_mental_disorders_id",
                principalSchema: "public",
                principalTable: "specific_mental_disorders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_medical_records_doctor_profiles_doctor_profile_id",
                schema: "public",
                table: "medical_records",
                column: "doctor_profile_id",
                principalSchema: "public",
                principalTable: "doctor_profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_medical_records_medical_histories_medical_history_id",
                schema: "public",
                table: "medical_records",
                column: "medical_history_id",
                principalSchema: "public",
                principalTable: "medical_histories",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_medical_records_patient_profiles_patient_profile_id",
                schema: "public",
                table: "medical_records",
                column: "patient_profile_id",
                principalSchema: "public",
                principalTable: "patient_profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_patient_profiles_jobs_job_id",
                schema: "public",
                table: "patient_profiles",
                column: "job_id",
                principalSchema: "public",
                principalTable: "jobs",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_specific_mental_disorders_mental_disorders_mental_disorder_",
                schema: "public",
                table: "specific_mental_disorders",
                column: "mental_disorder_id",
                principalSchema: "public",
                principalTable: "mental_disorders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_doctor_profile_specialty_doctor_profiles_doctor_profiles_id",
                schema: "public",
                table: "doctor_profile_specialty");

            migrationBuilder.DropForeignKey(
                name: "fk_doctor_profile_specialty_specialties_specialties_id",
                schema: "public",
                table: "doctor_profile_specialty");

            migrationBuilder.DropForeignKey(
                name: "fk_jobs_industries_industry_id",
                schema: "public",
                table: "jobs");

            migrationBuilder.DropForeignKey(
                name: "fk_medical_histories_patient_profiles_patient_id",
                schema: "public",
                table: "medical_histories");

            migrationBuilder.DropForeignKey(
                name: "fk_medical_history_physical_symptom_medical_histories_medical_",
                schema: "public",
                table: "medical_history_physical_symptom");

            migrationBuilder.DropForeignKey(
                name: "fk_medical_history_physical_symptom_physical_symptoms_physical",
                schema: "public",
                table: "medical_history_physical_symptom");

            migrationBuilder.DropForeignKey(
                name: "fk_medical_history_specific_mental_disorder_medical_histories_",
                schema: "public",
                table: "medical_history_specific_mental_disorder");

            migrationBuilder.DropForeignKey(
                name: "fk_medical_history_specific_mental_disorder_specific_mental_di",
                schema: "public",
                table: "medical_history_specific_mental_disorder");

            migrationBuilder.DropForeignKey(
                name: "fk_medical_record_specific_mental_disorder_medical_records_med",
                schema: "public",
                table: "medical_record_specific_mental_disorder");

            migrationBuilder.DropForeignKey(
                name: "fk_medical_record_specific_mental_disorder_specific_mental_dis",
                schema: "public",
                table: "medical_record_specific_mental_disorder");

            migrationBuilder.DropForeignKey(
                name: "fk_medical_records_doctor_profiles_doctor_profile_id",
                schema: "public",
                table: "medical_records");

            migrationBuilder.DropForeignKey(
                name: "fk_medical_records_medical_histories_medical_history_id",
                schema: "public",
                table: "medical_records");

            migrationBuilder.DropForeignKey(
                name: "fk_medical_records_patient_profiles_patient_profile_id",
                schema: "public",
                table: "medical_records");

            migrationBuilder.DropForeignKey(
                name: "fk_patient_profiles_jobs_job_id",
                schema: "public",
                table: "patient_profiles");

            migrationBuilder.DropForeignKey(
                name: "fk_specific_mental_disorders_mental_disorders_mental_disorder_",
                schema: "public",
                table: "specific_mental_disorders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_specialties",
                schema: "public",
                table: "specialties");

            migrationBuilder.DropPrimaryKey(
                name: "pk_jobs",
                schema: "public",
                table: "jobs");

            migrationBuilder.DropPrimaryKey(
                name: "pk_industries",
                schema: "public",
                table: "industries");

            migrationBuilder.DropPrimaryKey(
                name: "pk_specific_mental_disorders",
                schema: "public",
                table: "specific_mental_disorders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_physical_symptoms",
                schema: "public",
                table: "physical_symptoms");

            migrationBuilder.DropPrimaryKey(
                name: "pk_patient_profiles",
                schema: "public",
                table: "patient_profiles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_mental_disorders",
                schema: "public",
                table: "mental_disorders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_medical_records",
                schema: "public",
                table: "medical_records");

            migrationBuilder.DropPrimaryKey(
                name: "pk_medical_record_specific_mental_disorder",
                schema: "public",
                table: "medical_record_specific_mental_disorder");

            migrationBuilder.DropPrimaryKey(
                name: "pk_medical_history_specific_mental_disorder",
                schema: "public",
                table: "medical_history_specific_mental_disorder");

            migrationBuilder.DropPrimaryKey(
                name: "pk_medical_history_physical_symptom",
                schema: "public",
                table: "medical_history_physical_symptom");

            migrationBuilder.DropPrimaryKey(
                name: "pk_medical_histories",
                schema: "public",
                table: "medical_histories");

            migrationBuilder.DropPrimaryKey(
                name: "pk_doctor_profiles",
                schema: "public",
                table: "doctor_profiles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_doctor_profile_specialty",
                schema: "public",
                table: "doctor_profile_specialty");

            migrationBuilder.RenameTable(
                name: "specialties",
                schema: "public",
                newName: "Specialties",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "jobs",
                schema: "public",
                newName: "Jobs",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "industries",
                schema: "public",
                newName: "Industries",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "specific_mental_disorders",
                schema: "public",
                newName: "SpecificMentalDisorders",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "physical_symptoms",
                schema: "public",
                newName: "PhysicalSymptoms",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "patient_profiles",
                schema: "public",
                newName: "PatientProfiles",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "mental_disorders",
                schema: "public",
                newName: "MentalDisorders",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "medical_records",
                schema: "public",
                newName: "MedicalRecords",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "medical_record_specific_mental_disorder",
                schema: "public",
                newName: "MedicalRecordSpecificMentalDisorder",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "medical_history_specific_mental_disorder",
                schema: "public",
                newName: "MedicalHistorySpecificMentalDisorder",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "medical_history_physical_symptom",
                schema: "public",
                newName: "MedicalHistoryPhysicalSymptom",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "medical_histories",
                schema: "public",
                newName: "MedicalHistories",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "doctor_profiles",
                schema: "public",
                newName: "DoctorProfiles",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "doctor_profile_specialty",
                schema: "public",
                newName: "DoctorProfileSpecialty",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "Specialties",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "Specialties",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "Jobs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                schema: "public",
                table: "Jobs",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                schema: "public",
                table: "Jobs",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "job_title",
                schema: "public",
                table: "Jobs",
                newName: "JobTitle");

            migrationBuilder.RenameColumn(
                name: "industry_id",
                schema: "public",
                table: "Jobs",
                newName: "IndustryId");

            migrationBuilder.RenameColumn(
                name: "education_level",
                schema: "public",
                table: "Jobs",
                newName: "EducationLevel");

            migrationBuilder.RenameColumn(
                name: "created_by",
                schema: "public",
                table: "Jobs",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "public",
                table: "Jobs",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_jobs_industry_id",
                schema: "public",
                table: "Jobs",
                newName: "IX_Jobs_IndustryId");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "public",
                table: "Industries",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "Industries",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                schema: "public",
                table: "Industries",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                schema: "public",
                table: "Industries",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "industry_name",
                schema: "public",
                table: "Industries",
                newName: "IndustryName");

            migrationBuilder.RenameColumn(
                name: "created_by",
                schema: "public",
                table: "Industries",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "public",
                table: "Industries",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "SpecificMentalDisorders",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "public",
                table: "SpecificMentalDisorders",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "SpecificMentalDisorders",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "mental_disorder_id",
                schema: "public",
                table: "SpecificMentalDisorders",
                newName: "MentalDisorderId");

            migrationBuilder.RenameIndex(
                name: "ix_specific_mental_disorders_mental_disorder_id",
                schema: "public",
                table: "SpecificMentalDisorders",
                newName: "IX_SpecificMentalDisorders_MentalDisorderId");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "PhysicalSymptoms",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "public",
                table: "PhysicalSymptoms",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "PhysicalSymptoms",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "allergies",
                schema: "public",
                table: "PatientProfiles",
                newName: "Allergies");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "PatientProfiles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "public",
                table: "PatientProfiles",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "medical_history_id",
                schema: "public",
                table: "PatientProfiles",
                newName: "MedicalHistoryId");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                schema: "public",
                table: "PatientProfiles",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                schema: "public",
                table: "PatientProfiles",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "job_id",
                schema: "public",
                table: "PatientProfiles",
                newName: "JobId");

            migrationBuilder.RenameColumn(
                name: "is_profile_completed",
                schema: "public",
                table: "PatientProfiles",
                newName: "IsProfileCompleted");

            migrationBuilder.RenameColumn(
                name: "full_name",
                schema: "public",
                table: "PatientProfiles",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "created_by",
                schema: "public",
                table: "PatientProfiles",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "public",
                table: "PatientProfiles",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "birth_date",
                schema: "public",
                table: "PatientProfiles",
                newName: "BirthDate");

            migrationBuilder.RenameIndex(
                name: "ix_patient_profiles_job_id",
                schema: "public",
                table: "PatientProfiles",
                newName: "IX_PatientProfiles_JobId");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "MentalDisorders",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "public",
                table: "MentalDisorders",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "MentalDisorders",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                schema: "public",
                table: "MentalDisorders",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                schema: "public",
                table: "MentalDisorders",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "created_by",
                schema: "public",
                table: "MentalDisorders",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "public",
                table: "MentalDisorders",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "public",
                table: "MedicalRecords",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "notes",
                schema: "public",
                table: "MedicalRecords",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "MedicalRecords",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "patient_profile_id",
                schema: "public",
                table: "MedicalRecords",
                newName: "PatientProfileId");

            migrationBuilder.RenameColumn(
                name: "medical_history_id",
                schema: "public",
                table: "MedicalRecords",
                newName: "MedicalHistoryId");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                schema: "public",
                table: "MedicalRecords",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                schema: "public",
                table: "MedicalRecords",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "doctor_profile_id",
                schema: "public",
                table: "MedicalRecords",
                newName: "DoctorProfileId");

            migrationBuilder.RenameColumn(
                name: "created_by",
                schema: "public",
                table: "MedicalRecords",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "public",
                table: "MedicalRecords",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_medical_records_patient_profile_id",
                schema: "public",
                table: "MedicalRecords",
                newName: "IX_MedicalRecords_PatientProfileId");

            migrationBuilder.RenameIndex(
                name: "ix_medical_records_medical_history_id",
                schema: "public",
                table: "MedicalRecords",
                newName: "IX_MedicalRecords_MedicalHistoryId");

            migrationBuilder.RenameIndex(
                name: "ix_medical_records_doctor_profile_id",
                schema: "public",
                table: "MedicalRecords",
                newName: "IX_MedicalRecords_DoctorProfileId");

            migrationBuilder.RenameColumn(
                name: "specific_mental_disorders_id",
                schema: "public",
                table: "MedicalRecordSpecificMentalDisorder",
                newName: "SpecificMentalDisordersId");

            migrationBuilder.RenameColumn(
                name: "medical_records_id",
                schema: "public",
                table: "MedicalRecordSpecificMentalDisorder",
                newName: "MedicalRecordsId");

            migrationBuilder.RenameIndex(
                name: "ix_medical_record_specific_mental_disorder_specific_mental_dis",
                schema: "public",
                table: "MedicalRecordSpecificMentalDisorder",
                newName: "IX_MedicalRecordSpecificMentalDisorder_SpecificMentalDisorders~");

            migrationBuilder.RenameColumn(
                name: "specific_mental_disorders_id",
                schema: "public",
                table: "MedicalHistorySpecificMentalDisorder",
                newName: "SpecificMentalDisordersId");

            migrationBuilder.RenameColumn(
                name: "medical_histories_id",
                schema: "public",
                table: "MedicalHistorySpecificMentalDisorder",
                newName: "MedicalHistoriesId");

            migrationBuilder.RenameIndex(
                name: "ix_medical_history_specific_mental_disorder_specific_mental_di",
                schema: "public",
                table: "MedicalHistorySpecificMentalDisorder",
                newName: "IX_MedicalHistorySpecificMentalDisorder_SpecificMentalDisorder~");

            migrationBuilder.RenameColumn(
                name: "physical_symptoms_id",
                schema: "public",
                table: "MedicalHistoryPhysicalSymptom",
                newName: "PhysicalSymptomsId");

            migrationBuilder.RenameColumn(
                name: "medical_histories_id",
                schema: "public",
                table: "MedicalHistoryPhysicalSymptom",
                newName: "MedicalHistoriesId");

            migrationBuilder.RenameIndex(
                name: "ix_medical_history_physical_symptom_physical_symptoms_id",
                schema: "public",
                table: "MedicalHistoryPhysicalSymptom",
                newName: "IX_MedicalHistoryPhysicalSymptom_PhysicalSymptomsId");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "public",
                table: "MedicalHistories",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "MedicalHistories",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "patient_id",
                schema: "public",
                table: "MedicalHistories",
                newName: "PatientId");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                schema: "public",
                table: "MedicalHistories",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                schema: "public",
                table: "MedicalHistories",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "diagnosed_at",
                schema: "public",
                table: "MedicalHistories",
                newName: "DiagnosedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                schema: "public",
                table: "MedicalHistories",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "public",
                table: "MedicalHistories",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_medical_histories_patient_id",
                schema: "public",
                table: "MedicalHistories",
                newName: "IX_MedicalHistories_PatientId");

            migrationBuilder.RenameColumn(
                name: "rating",
                schema: "public",
                table: "DoctorProfiles",
                newName: "Rating");

            migrationBuilder.RenameColumn(
                name: "qualifications",
                schema: "public",
                table: "DoctorProfiles",
                newName: "Qualifications");

            migrationBuilder.RenameColumn(
                name: "bio",
                schema: "public",
                table: "DoctorProfiles",
                newName: "Bio");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "DoctorProfiles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "years_of_experience",
                schema: "public",
                table: "DoctorProfiles",
                newName: "YearsOfExperience");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "public",
                table: "DoctorProfiles",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "total_reviews",
                schema: "public",
                table: "DoctorProfiles",
                newName: "TotalReviews");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                schema: "public",
                table: "DoctorProfiles",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                schema: "public",
                table: "DoctorProfiles",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "full_name",
                schema: "public",
                table: "DoctorProfiles",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "created_by",
                schema: "public",
                table: "DoctorProfiles",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "public",
                table: "DoctorProfiles",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "specialties_id",
                schema: "public",
                table: "DoctorProfileSpecialty",
                newName: "SpecialtiesId");

            migrationBuilder.RenameColumn(
                name: "doctor_profiles_id",
                schema: "public",
                table: "DoctorProfileSpecialty",
                newName: "DoctorProfilesId");

            migrationBuilder.RenameIndex(
                name: "ix_doctor_profile_specialty_specialties_id",
                schema: "public",
                table: "DoctorProfileSpecialty",
                newName: "IX_DoctorProfileSpecialty_SpecialtiesId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Specialties",
                schema: "public",
                table: "Specialties",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Jobs",
                schema: "public",
                table: "Jobs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Industries",
                schema: "public",
                table: "Industries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SpecificMentalDisorders",
                schema: "public",
                table: "SpecificMentalDisorders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhysicalSymptoms",
                schema: "public",
                table: "PhysicalSymptoms",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PatientProfiles",
                schema: "public",
                table: "PatientProfiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MentalDisorders",
                schema: "public",
                table: "MentalDisorders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicalRecords",
                schema: "public",
                table: "MedicalRecords",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicalRecordSpecificMentalDisorder",
                schema: "public",
                table: "MedicalRecordSpecificMentalDisorder",
                columns: new[] { "MedicalRecordsId", "SpecificMentalDisordersId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicalHistorySpecificMentalDisorder",
                schema: "public",
                table: "MedicalHistorySpecificMentalDisorder",
                columns: new[] { "MedicalHistoriesId", "SpecificMentalDisordersId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicalHistoryPhysicalSymptom",
                schema: "public",
                table: "MedicalHistoryPhysicalSymptom",
                columns: new[] { "MedicalHistoriesId", "PhysicalSymptomsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicalHistories",
                schema: "public",
                table: "MedicalHistories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorProfiles",
                schema: "public",
                table: "DoctorProfiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorProfileSpecialty",
                schema: "public",
                table: "DoctorProfileSpecialty",
                columns: new[] { "DoctorProfilesId", "SpecialtiesId" });

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorProfileSpecialty_DoctorProfiles_DoctorProfilesId",
                schema: "public",
                table: "DoctorProfileSpecialty",
                column: "DoctorProfilesId",
                principalSchema: "public",
                principalTable: "DoctorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorProfileSpecialty_Specialties_SpecialtiesId",
                schema: "public",
                table: "DoctorProfileSpecialty",
                column: "SpecialtiesId",
                principalSchema: "public",
                principalTable: "Specialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Industries_IndustryId",
                schema: "public",
                table: "Jobs",
                column: "IndustryId",
                principalSchema: "public",
                principalTable: "Industries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistories_PatientProfiles_PatientId",
                schema: "public",
                table: "MedicalHistories",
                column: "PatientId",
                principalSchema: "public",
                principalTable: "PatientProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistoryPhysicalSymptom_MedicalHistories_MedicalHisto~",
                schema: "public",
                table: "MedicalHistoryPhysicalSymptom",
                column: "MedicalHistoriesId",
                principalSchema: "public",
                principalTable: "MedicalHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistoryPhysicalSymptom_PhysicalSymptoms_PhysicalSymp~",
                schema: "public",
                table: "MedicalHistoryPhysicalSymptom",
                column: "PhysicalSymptomsId",
                principalSchema: "public",
                principalTable: "PhysicalSymptoms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistorySpecificMentalDisorder_MedicalHistories_Medic~",
                schema: "public",
                table: "MedicalHistorySpecificMentalDisorder",
                column: "MedicalHistoriesId",
                principalSchema: "public",
                principalTable: "MedicalHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistorySpecificMentalDisorder_SpecificMentalDisorder~",
                schema: "public",
                table: "MedicalHistorySpecificMentalDisorder",
                column: "SpecificMentalDisordersId",
                principalSchema: "public",
                principalTable: "SpecificMentalDisorders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_DoctorProfiles_DoctorProfileId",
                schema: "public",
                table: "MedicalRecords",
                column: "DoctorProfileId",
                principalSchema: "public",
                principalTable: "DoctorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_MedicalHistories_MedicalHistoryId",
                schema: "public",
                table: "MedicalRecords",
                column: "MedicalHistoryId",
                principalSchema: "public",
                principalTable: "MedicalHistories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_PatientProfiles_PatientProfileId",
                schema: "public",
                table: "MedicalRecords",
                column: "PatientProfileId",
                principalSchema: "public",
                principalTable: "PatientProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecordSpecificMentalDisorder_MedicalRecords_MedicalR~",
                schema: "public",
                table: "MedicalRecordSpecificMentalDisorder",
                column: "MedicalRecordsId",
                principalSchema: "public",
                principalTable: "MedicalRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecordSpecificMentalDisorder_SpecificMentalDisorders~",
                schema: "public",
                table: "MedicalRecordSpecificMentalDisorder",
                column: "SpecificMentalDisordersId",
                principalSchema: "public",
                principalTable: "SpecificMentalDisorders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PatientProfiles_Jobs_JobId",
                schema: "public",
                table: "PatientProfiles",
                column: "JobId",
                principalSchema: "public",
                principalTable: "Jobs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SpecificMentalDisorders_MentalDisorders_MentalDisorderId",
                schema: "public",
                table: "SpecificMentalDisorders",
                column: "MentalDisorderId",
                principalSchema: "public",
                principalTable: "MentalDisorders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
