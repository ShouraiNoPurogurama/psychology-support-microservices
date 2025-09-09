using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Public.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_schema = 'public'
                          AND table_name = 'patient_profiles'
                          AND column_name = 'subject_ref'
                    ) THEN
                        ALTER TABLE public.patient_profiles
                        ADD COLUMN subject_ref uuid NOT NULL 
                        DEFAULT '00000000-0000-0000-0000-000000000000';
                    END IF;
                END$$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "subject_ref",
                schema: "public",
                table: "patient_profiles");
        }
    }
}
