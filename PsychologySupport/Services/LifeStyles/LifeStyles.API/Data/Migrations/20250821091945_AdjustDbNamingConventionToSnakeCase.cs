using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeStyles.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdjustDbNamingConventionToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmotionSelections_Emotions_EmotionId",
                schema: "public",
                table: "EmotionSelections");

            migrationBuilder.DropForeignKey(
                name: "FK_EmotionSelections_PatientEmotionCheckpoints_EmotionCheckpoi~",
                schema: "public",
                table: "EmotionSelections");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodActivityFoodCategory_FoodActivities_FoodActivitiesId",
                schema: "public",
                table: "FoodActivityFoodCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodActivityFoodCategory_FoodCategories_FoodCategoriesId",
                schema: "public",
                table: "FoodActivityFoodCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodActivityFoodNutrient_FoodActivities_FoodActivitiesId",
                schema: "public",
                table: "FoodActivityFoodNutrient");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodActivityFoodNutrient_FoodNutrients_FoodNutrientsId",
                schema: "public",
                table: "FoodActivityFoodNutrient");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientImprovementGoals_ImprovementGoals_GoalId",
                schema: "public",
                table: "PatientImprovementGoals");

            migrationBuilder.DropForeignKey(
                name: "FK_TherapeuticActivities_TherapeuticTypes_TherapeuticTypeId",
                schema: "public",
                table: "TherapeuticActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Emotions",
                schema: "public",
                table: "Emotions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TherapeuticTypes",
                schema: "public",
                table: "TherapeuticTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TherapeuticActivities",
                schema: "public",
                table: "TherapeuticActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhysicalActivities",
                schema: "public",
                table: "PhysicalActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PatientTherapeuticActivities",
                schema: "public",
                table: "PatientTherapeuticActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PatientPhysicalActivities",
                schema: "public",
                table: "PatientPhysicalActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PatientImprovementGoals",
                schema: "public",
                table: "PatientImprovementGoals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PatientFoodActivities",
                schema: "public",
                table: "PatientFoodActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PatientEntertainmentActivities",
                schema: "public",
                table: "PatientEntertainmentActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PatientEmotionCheckpoints",
                schema: "public",
                table: "PatientEmotionCheckpoints");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LifestyleLogs",
                schema: "public",
                table: "LifestyleLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImprovementGoals",
                schema: "public",
                table: "ImprovementGoals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodNutrients",
                schema: "public",
                table: "FoodNutrients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodCategories",
                schema: "public",
                table: "FoodCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodActivityFoodNutrient",
                schema: "public",
                table: "FoodActivityFoodNutrient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodActivityFoodCategory",
                schema: "public",
                table: "FoodActivityFoodCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodActivities",
                schema: "public",
                table: "FoodActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EntertainmentActivities",
                schema: "public",
                table: "EntertainmentActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmotionSelections",
                schema: "public",
                table: "EmotionSelections");

            migrationBuilder.RenameTable(
                name: "Emotions",
                schema: "public",
                newName: "emotions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "TherapeuticTypes",
                schema: "public",
                newName: "therapeutic_types",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "TherapeuticActivities",
                schema: "public",
                newName: "therapeutic_activities",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "PhysicalActivities",
                schema: "public",
                newName: "physical_activities",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "PatientTherapeuticActivities",
                schema: "public",
                newName: "patient_therapeutic_activities",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "PatientPhysicalActivities",
                schema: "public",
                newName: "patient_physical_activities",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "PatientImprovementGoals",
                schema: "public",
                newName: "patient_improvement_goals",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "PatientFoodActivities",
                schema: "public",
                newName: "patient_food_activities",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "PatientEntertainmentActivities",
                schema: "public",
                newName: "patient_entertainment_activities",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "PatientEmotionCheckpoints",
                schema: "public",
                newName: "patient_emotion_checkpoints",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "LifestyleLogs",
                schema: "public",
                newName: "lifestyle_logs",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "ImprovementGoals",
                schema: "public",
                newName: "improvement_goals",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "FoodNutrients",
                schema: "public",
                newName: "food_nutrients",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "FoodCategories",
                schema: "public",
                newName: "food_categories",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "FoodActivityFoodNutrient",
                schema: "public",
                newName: "food_activity_food_nutrient",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "FoodActivityFoodCategory",
                schema: "public",
                newName: "food_activity_food_category",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "FoodActivities",
                schema: "public",
                newName: "food_activities",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "EntertainmentActivities",
                schema: "public",
                newName: "entertainment_activities",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "EmotionSelections",
                schema: "public",
                newName: "emotion_selections",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "public",
                table: "emotions",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "emotions",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "emotions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "IconUrl",
                schema: "public",
                table: "emotions",
                newName: "icon_url");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "public",
                table: "therapeutic_types",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "therapeutic_types",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "therapeutic_types",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "public",
                table: "therapeutic_activities",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Instructions",
                schema: "public",
                table: "therapeutic_activities",
                newName: "instructions");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "therapeutic_activities",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "therapeutic_activities",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TherapeuticTypeId",
                schema: "public",
                table: "therapeutic_activities",
                newName: "therapeutic_type_id");

            migrationBuilder.RenameColumn(
                name: "IntensityLevel",
                schema: "public",
                table: "therapeutic_activities",
                newName: "intensity_level");

            migrationBuilder.RenameColumn(
                name: "ImpactLevel",
                schema: "public",
                table: "therapeutic_activities",
                newName: "impact_level");

            migrationBuilder.RenameIndex(
                name: "IX_TherapeuticActivities_TherapeuticTypeId",
                schema: "public",
                table: "therapeutic_activities",
                newName: "ix_therapeutic_activities_therapeutic_type_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "public",
                table: "physical_activities",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "physical_activities",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "physical_activities",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "IntensityLevel",
                schema: "public",
                table: "physical_activities",
                newName: "intensity_level");

            migrationBuilder.RenameColumn(
                name: "ImpactLevel",
                schema: "public",
                table: "physical_activities",
                newName: "impact_level");

            migrationBuilder.RenameColumn(
                name: "PreferenceLevel",
                schema: "public",
                table: "patient_therapeutic_activities",
                newName: "preference_level");

            migrationBuilder.RenameColumn(
                name: "TherapeuticActivityId",
                schema: "public",
                table: "patient_therapeutic_activities",
                newName: "therapeutic_activity_id");

            migrationBuilder.RenameColumn(
                name: "PatientProfileId",
                schema: "public",
                table: "patient_therapeutic_activities",
                newName: "patient_profile_id");

            migrationBuilder.RenameColumn(
                name: "PreferenceLevel",
                schema: "public",
                table: "patient_physical_activities",
                newName: "preference_level");

            migrationBuilder.RenameColumn(
                name: "PhysicalActivityId",
                schema: "public",
                table: "patient_physical_activities",
                newName: "physical_activity_id");

            migrationBuilder.RenameColumn(
                name: "PatientProfileId",
                schema: "public",
                table: "patient_physical_activities",
                newName: "patient_profile_id");

            migrationBuilder.RenameColumn(
                name: "AssignedAt",
                schema: "public",
                table: "patient_improvement_goals",
                newName: "assigned_at");

            migrationBuilder.RenameColumn(
                name: "GoalId",
                schema: "public",
                table: "patient_improvement_goals",
                newName: "goal_id");

            migrationBuilder.RenameColumn(
                name: "PatientProfileId",
                schema: "public",
                table: "patient_improvement_goals",
                newName: "patient_profile_id");

            migrationBuilder.RenameIndex(
                name: "IX_PatientImprovementGoals_GoalId",
                schema: "public",
                table: "patient_improvement_goals",
                newName: "ix_patient_improvement_goals_goal_id");

            migrationBuilder.RenameColumn(
                name: "PreferenceLevel",
                schema: "public",
                table: "patient_food_activities",
                newName: "preference_level");

            migrationBuilder.RenameColumn(
                name: "FoodActivityId",
                schema: "public",
                table: "patient_food_activities",
                newName: "food_activity_id");

            migrationBuilder.RenameColumn(
                name: "PatientProfileId",
                schema: "public",
                table: "patient_food_activities",
                newName: "patient_profile_id");

            migrationBuilder.RenameColumn(
                name: "PreferenceLevel",
                schema: "public",
                table: "patient_entertainment_activities",
                newName: "preference_level");

            migrationBuilder.RenameColumn(
                name: "EntertainmentActivityId",
                schema: "public",
                table: "patient_entertainment_activities",
                newName: "entertainment_activity_id");

            migrationBuilder.RenameColumn(
                name: "PatientProfileId",
                schema: "public",
                table: "patient_entertainment_activities",
                newName: "patient_profile_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "patient_emotion_checkpoints",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PatientProfileId",
                schema: "public",
                table: "patient_emotion_checkpoints",
                newName: "patient_profile_id");

            migrationBuilder.RenameColumn(
                name: "LogDate",
                schema: "public",
                table: "patient_emotion_checkpoints",
                newName: "log_date");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "lifestyle_logs",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SleepHours",
                schema: "public",
                table: "lifestyle_logs",
                newName: "sleep_hours");

            migrationBuilder.RenameColumn(
                name: "PatientProfileId",
                schema: "public",
                table: "lifestyle_logs",
                newName: "patient_profile_id");

            migrationBuilder.RenameColumn(
                name: "LogDate",
                schema: "public",
                table: "lifestyle_logs",
                newName: "log_date");

            migrationBuilder.RenameColumn(
                name: "ExerciseFrequency",
                schema: "public",
                table: "lifestyle_logs",
                newName: "exercise_frequency");

            migrationBuilder.RenameColumn(
                name: "AvailableTimePerDay",
                schema: "public",
                table: "lifestyle_logs",
                newName: "available_time_per_day");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "public",
                table: "improvement_goals",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "improvement_goals",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "improvement_goals",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "public",
                table: "food_nutrients",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "food_nutrients",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "food_nutrients",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "public",
                table: "food_categories",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "food_categories",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "food_categories",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "FoodNutrientsId",
                schema: "public",
                table: "food_activity_food_nutrient",
                newName: "food_nutrients_id");

            migrationBuilder.RenameColumn(
                name: "FoodActivitiesId",
                schema: "public",
                table: "food_activity_food_nutrient",
                newName: "food_activities_id");

            migrationBuilder.RenameIndex(
                name: "IX_FoodActivityFoodNutrient_FoodNutrientsId",
                schema: "public",
                table: "food_activity_food_nutrient",
                newName: "ix_food_activity_food_nutrient_food_nutrients_id");

            migrationBuilder.RenameColumn(
                name: "FoodCategoriesId",
                schema: "public",
                table: "food_activity_food_category",
                newName: "food_categories_id");

            migrationBuilder.RenameColumn(
                name: "FoodActivitiesId",
                schema: "public",
                table: "food_activity_food_category",
                newName: "food_activities_id");

            migrationBuilder.RenameIndex(
                name: "IX_FoodActivityFoodCategory_FoodCategoriesId",
                schema: "public",
                table: "food_activity_food_category",
                newName: "ix_food_activity_food_category_food_categories_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "public",
                table: "food_activities",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "food_activities",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "food_activities",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "MealTime",
                schema: "public",
                table: "food_activities",
                newName: "meal_time");

            migrationBuilder.RenameColumn(
                name: "IntensityLevel",
                schema: "public",
                table: "food_activities",
                newName: "intensity_level");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "public",
                table: "entertainment_activities",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "entertainment_activities",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "entertainment_activities",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "IntensityLevel",
                schema: "public",
                table: "entertainment_activities",
                newName: "intensity_level");

            migrationBuilder.RenameColumn(
                name: "ImpactLevel",
                schema: "public",
                table: "entertainment_activities",
                newName: "impact_level");

            migrationBuilder.RenameColumn(
                name: "Rank",
                schema: "public",
                table: "emotion_selections",
                newName: "rank");

            migrationBuilder.RenameColumn(
                name: "Intensity",
                schema: "public",
                table: "emotion_selections",
                newName: "intensity");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "emotion_selections",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "EmotionId",
                schema: "public",
                table: "emotion_selections",
                newName: "emotion_id");

            migrationBuilder.RenameColumn(
                name: "EmotionCheckpointId",
                schema: "public",
                table: "emotion_selections",
                newName: "emotion_checkpoint_id");

            migrationBuilder.RenameIndex(
                name: "IX_EmotionSelections_EmotionId",
                schema: "public",
                table: "emotion_selections",
                newName: "ix_emotion_selections_emotion_id");

            migrationBuilder.RenameIndex(
                name: "IX_EmotionSelections_EmotionCheckpointId",
                schema: "public",
                table: "emotion_selections",
                newName: "ix_emotion_selections_emotion_checkpoint_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_emotions",
                schema: "public",
                table: "emotions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_therapeutic_types",
                schema: "public",
                table: "therapeutic_types",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_therapeutic_activities",
                schema: "public",
                table: "therapeutic_activities",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_physical_activities",
                schema: "public",
                table: "physical_activities",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_patient_therapeutic_activities",
                schema: "public",
                table: "patient_therapeutic_activities",
                columns: new[] { "patient_profile_id", "therapeutic_activity_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_patient_physical_activities",
                schema: "public",
                table: "patient_physical_activities",
                columns: new[] { "patient_profile_id", "physical_activity_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_patient_improvement_goals",
                schema: "public",
                table: "patient_improvement_goals",
                columns: new[] { "patient_profile_id", "goal_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_patient_food_activities",
                schema: "public",
                table: "patient_food_activities",
                columns: new[] { "patient_profile_id", "food_activity_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_patient_entertainment_activities",
                schema: "public",
                table: "patient_entertainment_activities",
                columns: new[] { "patient_profile_id", "entertainment_activity_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_patient_emotion_checkpoints",
                schema: "public",
                table: "patient_emotion_checkpoints",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_lifestyle_logs",
                schema: "public",
                table: "lifestyle_logs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_improvement_goals",
                schema: "public",
                table: "improvement_goals",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_food_nutrients",
                schema: "public",
                table: "food_nutrients",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_food_categories",
                schema: "public",
                table: "food_categories",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_food_activity_food_nutrient",
                schema: "public",
                table: "food_activity_food_nutrient",
                columns: new[] { "food_activities_id", "food_nutrients_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_food_activity_food_category",
                schema: "public",
                table: "food_activity_food_category",
                columns: new[] { "food_activities_id", "food_categories_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_food_activities",
                schema: "public",
                table: "food_activities",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_entertainment_activities",
                schema: "public",
                table: "entertainment_activities",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_emotion_selections",
                schema: "public",
                table: "emotion_selections",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_emotion_selections_emotions_emotion_id",
                schema: "public",
                table: "emotion_selections",
                column: "emotion_id",
                principalSchema: "public",
                principalTable: "emotions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_emotion_selections_patient_emotion_checkpoints_emotion_chec",
                schema: "public",
                table: "emotion_selections",
                column: "emotion_checkpoint_id",
                principalSchema: "public",
                principalTable: "patient_emotion_checkpoints",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_food_activity_food_category_food_activities_food_activities",
                schema: "public",
                table: "food_activity_food_category",
                column: "food_activities_id",
                principalSchema: "public",
                principalTable: "food_activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_food_activity_food_category_food_categories_food_categories",
                schema: "public",
                table: "food_activity_food_category",
                column: "food_categories_id",
                principalSchema: "public",
                principalTable: "food_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_food_activity_food_nutrient_food_activities_food_activities",
                schema: "public",
                table: "food_activity_food_nutrient",
                column: "food_activities_id",
                principalSchema: "public",
                principalTable: "food_activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_food_activity_food_nutrient_food_nutrients_food_nutrients_id",
                schema: "public",
                table: "food_activity_food_nutrient",
                column: "food_nutrients_id",
                principalSchema: "public",
                principalTable: "food_nutrients",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_patient_improvement_goals_improvement_goals_goal_id",
                schema: "public",
                table: "patient_improvement_goals",
                column: "goal_id",
                principalSchema: "public",
                principalTable: "improvement_goals",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_therapeutic_activities_therapeutic_types_therapeutic_type_id",
                schema: "public",
                table: "therapeutic_activities",
                column: "therapeutic_type_id",
                principalSchema: "public",
                principalTable: "therapeutic_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_emotion_selections_emotions_emotion_id",
                schema: "public",
                table: "emotion_selections");

            migrationBuilder.DropForeignKey(
                name: "fk_emotion_selections_patient_emotion_checkpoints_emotion_chec",
                schema: "public",
                table: "emotion_selections");

            migrationBuilder.DropForeignKey(
                name: "fk_food_activity_food_category_food_activities_food_activities",
                schema: "public",
                table: "food_activity_food_category");

            migrationBuilder.DropForeignKey(
                name: "fk_food_activity_food_category_food_categories_food_categories",
                schema: "public",
                table: "food_activity_food_category");

            migrationBuilder.DropForeignKey(
                name: "fk_food_activity_food_nutrient_food_activities_food_activities",
                schema: "public",
                table: "food_activity_food_nutrient");

            migrationBuilder.DropForeignKey(
                name: "fk_food_activity_food_nutrient_food_nutrients_food_nutrients_id",
                schema: "public",
                table: "food_activity_food_nutrient");

            migrationBuilder.DropForeignKey(
                name: "fk_patient_improvement_goals_improvement_goals_goal_id",
                schema: "public",
                table: "patient_improvement_goals");

            migrationBuilder.DropForeignKey(
                name: "fk_therapeutic_activities_therapeutic_types_therapeutic_type_id",
                schema: "public",
                table: "therapeutic_activities");

            migrationBuilder.DropPrimaryKey(
                name: "pk_emotions",
                schema: "public",
                table: "emotions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_therapeutic_types",
                schema: "public",
                table: "therapeutic_types");

            migrationBuilder.DropPrimaryKey(
                name: "pk_therapeutic_activities",
                schema: "public",
                table: "therapeutic_activities");

            migrationBuilder.DropPrimaryKey(
                name: "pk_physical_activities",
                schema: "public",
                table: "physical_activities");

            migrationBuilder.DropPrimaryKey(
                name: "pk_patient_therapeutic_activities",
                schema: "public",
                table: "patient_therapeutic_activities");

            migrationBuilder.DropPrimaryKey(
                name: "pk_patient_physical_activities",
                schema: "public",
                table: "patient_physical_activities");

            migrationBuilder.DropPrimaryKey(
                name: "pk_patient_improvement_goals",
                schema: "public",
                table: "patient_improvement_goals");

            migrationBuilder.DropPrimaryKey(
                name: "pk_patient_food_activities",
                schema: "public",
                table: "patient_food_activities");

            migrationBuilder.DropPrimaryKey(
                name: "pk_patient_entertainment_activities",
                schema: "public",
                table: "patient_entertainment_activities");

            migrationBuilder.DropPrimaryKey(
                name: "pk_patient_emotion_checkpoints",
                schema: "public",
                table: "patient_emotion_checkpoints");

            migrationBuilder.DropPrimaryKey(
                name: "pk_lifestyle_logs",
                schema: "public",
                table: "lifestyle_logs");

            migrationBuilder.DropPrimaryKey(
                name: "pk_improvement_goals",
                schema: "public",
                table: "improvement_goals");

            migrationBuilder.DropPrimaryKey(
                name: "pk_food_nutrients",
                schema: "public",
                table: "food_nutrients");

            migrationBuilder.DropPrimaryKey(
                name: "pk_food_categories",
                schema: "public",
                table: "food_categories");

            migrationBuilder.DropPrimaryKey(
                name: "pk_food_activity_food_nutrient",
                schema: "public",
                table: "food_activity_food_nutrient");

            migrationBuilder.DropPrimaryKey(
                name: "pk_food_activity_food_category",
                schema: "public",
                table: "food_activity_food_category");

            migrationBuilder.DropPrimaryKey(
                name: "pk_food_activities",
                schema: "public",
                table: "food_activities");

            migrationBuilder.DropPrimaryKey(
                name: "pk_entertainment_activities",
                schema: "public",
                table: "entertainment_activities");

            migrationBuilder.DropPrimaryKey(
                name: "pk_emotion_selections",
                schema: "public",
                table: "emotion_selections");

            migrationBuilder.RenameTable(
                name: "emotions",
                schema: "public",
                newName: "Emotions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "therapeutic_types",
                schema: "public",
                newName: "TherapeuticTypes",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "therapeutic_activities",
                schema: "public",
                newName: "TherapeuticActivities",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "physical_activities",
                schema: "public",
                newName: "PhysicalActivities",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "patient_therapeutic_activities",
                schema: "public",
                newName: "PatientTherapeuticActivities",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "patient_physical_activities",
                schema: "public",
                newName: "PatientPhysicalActivities",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "patient_improvement_goals",
                schema: "public",
                newName: "PatientImprovementGoals",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "patient_food_activities",
                schema: "public",
                newName: "PatientFoodActivities",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "patient_entertainment_activities",
                schema: "public",
                newName: "PatientEntertainmentActivities",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "patient_emotion_checkpoints",
                schema: "public",
                newName: "PatientEmotionCheckpoints",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "lifestyle_logs",
                schema: "public",
                newName: "LifestyleLogs",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "improvement_goals",
                schema: "public",
                newName: "ImprovementGoals",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "food_nutrients",
                schema: "public",
                newName: "FoodNutrients",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "food_categories",
                schema: "public",
                newName: "FoodCategories",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "food_activity_food_nutrient",
                schema: "public",
                newName: "FoodActivityFoodNutrient",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "food_activity_food_category",
                schema: "public",
                newName: "FoodActivityFoodCategory",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "food_activities",
                schema: "public",
                newName: "FoodActivities",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "entertainment_activities",
                schema: "public",
                newName: "EntertainmentActivities",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "emotion_selections",
                schema: "public",
                newName: "EmotionSelections",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "Emotions",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "public",
                table: "Emotions",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "Emotions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "icon_url",
                schema: "public",
                table: "Emotions",
                newName: "IconUrl");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "TherapeuticTypes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "public",
                table: "TherapeuticTypes",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "TherapeuticTypes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "TherapeuticActivities",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "instructions",
                schema: "public",
                table: "TherapeuticActivities",
                newName: "Instructions");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "public",
                table: "TherapeuticActivities",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "TherapeuticActivities",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "therapeutic_type_id",
                schema: "public",
                table: "TherapeuticActivities",
                newName: "TherapeuticTypeId");

            migrationBuilder.RenameColumn(
                name: "intensity_level",
                schema: "public",
                table: "TherapeuticActivities",
                newName: "IntensityLevel");

            migrationBuilder.RenameColumn(
                name: "impact_level",
                schema: "public",
                table: "TherapeuticActivities",
                newName: "ImpactLevel");

            migrationBuilder.RenameIndex(
                name: "ix_therapeutic_activities_therapeutic_type_id",
                schema: "public",
                table: "TherapeuticActivities",
                newName: "IX_TherapeuticActivities_TherapeuticTypeId");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "PhysicalActivities",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "public",
                table: "PhysicalActivities",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "PhysicalActivities",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "intensity_level",
                schema: "public",
                table: "PhysicalActivities",
                newName: "IntensityLevel");

            migrationBuilder.RenameColumn(
                name: "impact_level",
                schema: "public",
                table: "PhysicalActivities",
                newName: "ImpactLevel");

            migrationBuilder.RenameColumn(
                name: "preference_level",
                schema: "public",
                table: "PatientTherapeuticActivities",
                newName: "PreferenceLevel");

            migrationBuilder.RenameColumn(
                name: "therapeutic_activity_id",
                schema: "public",
                table: "PatientTherapeuticActivities",
                newName: "TherapeuticActivityId");

            migrationBuilder.RenameColumn(
                name: "patient_profile_id",
                schema: "public",
                table: "PatientTherapeuticActivities",
                newName: "PatientProfileId");

            migrationBuilder.RenameColumn(
                name: "preference_level",
                schema: "public",
                table: "PatientPhysicalActivities",
                newName: "PreferenceLevel");

            migrationBuilder.RenameColumn(
                name: "physical_activity_id",
                schema: "public",
                table: "PatientPhysicalActivities",
                newName: "PhysicalActivityId");

            migrationBuilder.RenameColumn(
                name: "patient_profile_id",
                schema: "public",
                table: "PatientPhysicalActivities",
                newName: "PatientProfileId");

            migrationBuilder.RenameColumn(
                name: "assigned_at",
                schema: "public",
                table: "PatientImprovementGoals",
                newName: "AssignedAt");

            migrationBuilder.RenameColumn(
                name: "goal_id",
                schema: "public",
                table: "PatientImprovementGoals",
                newName: "GoalId");

            migrationBuilder.RenameColumn(
                name: "patient_profile_id",
                schema: "public",
                table: "PatientImprovementGoals",
                newName: "PatientProfileId");

            migrationBuilder.RenameIndex(
                name: "ix_patient_improvement_goals_goal_id",
                schema: "public",
                table: "PatientImprovementGoals",
                newName: "IX_PatientImprovementGoals_GoalId");

            migrationBuilder.RenameColumn(
                name: "preference_level",
                schema: "public",
                table: "PatientFoodActivities",
                newName: "PreferenceLevel");

            migrationBuilder.RenameColumn(
                name: "food_activity_id",
                schema: "public",
                table: "PatientFoodActivities",
                newName: "FoodActivityId");

            migrationBuilder.RenameColumn(
                name: "patient_profile_id",
                schema: "public",
                table: "PatientFoodActivities",
                newName: "PatientProfileId");

            migrationBuilder.RenameColumn(
                name: "preference_level",
                schema: "public",
                table: "PatientEntertainmentActivities",
                newName: "PreferenceLevel");

            migrationBuilder.RenameColumn(
                name: "entertainment_activity_id",
                schema: "public",
                table: "PatientEntertainmentActivities",
                newName: "EntertainmentActivityId");

            migrationBuilder.RenameColumn(
                name: "patient_profile_id",
                schema: "public",
                table: "PatientEntertainmentActivities",
                newName: "PatientProfileId");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "PatientEmotionCheckpoints",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "patient_profile_id",
                schema: "public",
                table: "PatientEmotionCheckpoints",
                newName: "PatientProfileId");

            migrationBuilder.RenameColumn(
                name: "log_date",
                schema: "public",
                table: "PatientEmotionCheckpoints",
                newName: "LogDate");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "LifestyleLogs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "sleep_hours",
                schema: "public",
                table: "LifestyleLogs",
                newName: "SleepHours");

            migrationBuilder.RenameColumn(
                name: "patient_profile_id",
                schema: "public",
                table: "LifestyleLogs",
                newName: "PatientProfileId");

            migrationBuilder.RenameColumn(
                name: "log_date",
                schema: "public",
                table: "LifestyleLogs",
                newName: "LogDate");

            migrationBuilder.RenameColumn(
                name: "exercise_frequency",
                schema: "public",
                table: "LifestyleLogs",
                newName: "ExerciseFrequency");

            migrationBuilder.RenameColumn(
                name: "available_time_per_day",
                schema: "public",
                table: "LifestyleLogs",
                newName: "AvailableTimePerDay");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "ImprovementGoals",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "public",
                table: "ImprovementGoals",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "ImprovementGoals",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "FoodNutrients",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "public",
                table: "FoodNutrients",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "FoodNutrients",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "FoodCategories",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "public",
                table: "FoodCategories",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "FoodCategories",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "food_nutrients_id",
                schema: "public",
                table: "FoodActivityFoodNutrient",
                newName: "FoodNutrientsId");

            migrationBuilder.RenameColumn(
                name: "food_activities_id",
                schema: "public",
                table: "FoodActivityFoodNutrient",
                newName: "FoodActivitiesId");

            migrationBuilder.RenameIndex(
                name: "ix_food_activity_food_nutrient_food_nutrients_id",
                schema: "public",
                table: "FoodActivityFoodNutrient",
                newName: "IX_FoodActivityFoodNutrient_FoodNutrientsId");

            migrationBuilder.RenameColumn(
                name: "food_categories_id",
                schema: "public",
                table: "FoodActivityFoodCategory",
                newName: "FoodCategoriesId");

            migrationBuilder.RenameColumn(
                name: "food_activities_id",
                schema: "public",
                table: "FoodActivityFoodCategory",
                newName: "FoodActivitiesId");

            migrationBuilder.RenameIndex(
                name: "ix_food_activity_food_category_food_categories_id",
                schema: "public",
                table: "FoodActivityFoodCategory",
                newName: "IX_FoodActivityFoodCategory_FoodCategoriesId");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "FoodActivities",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "public",
                table: "FoodActivities",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "FoodActivities",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "meal_time",
                schema: "public",
                table: "FoodActivities",
                newName: "MealTime");

            migrationBuilder.RenameColumn(
                name: "intensity_level",
                schema: "public",
                table: "FoodActivities",
                newName: "IntensityLevel");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "EntertainmentActivities",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "public",
                table: "EntertainmentActivities",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "EntertainmentActivities",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "intensity_level",
                schema: "public",
                table: "EntertainmentActivities",
                newName: "IntensityLevel");

            migrationBuilder.RenameColumn(
                name: "impact_level",
                schema: "public",
                table: "EntertainmentActivities",
                newName: "ImpactLevel");

            migrationBuilder.RenameColumn(
                name: "rank",
                schema: "public",
                table: "EmotionSelections",
                newName: "Rank");

            migrationBuilder.RenameColumn(
                name: "intensity",
                schema: "public",
                table: "EmotionSelections",
                newName: "Intensity");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "EmotionSelections",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "emotion_id",
                schema: "public",
                table: "EmotionSelections",
                newName: "EmotionId");

            migrationBuilder.RenameColumn(
                name: "emotion_checkpoint_id",
                schema: "public",
                table: "EmotionSelections",
                newName: "EmotionCheckpointId");

            migrationBuilder.RenameIndex(
                name: "ix_emotion_selections_emotion_id",
                schema: "public",
                table: "EmotionSelections",
                newName: "IX_EmotionSelections_EmotionId");

            migrationBuilder.RenameIndex(
                name: "ix_emotion_selections_emotion_checkpoint_id",
                schema: "public",
                table: "EmotionSelections",
                newName: "IX_EmotionSelections_EmotionCheckpointId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Emotions",
                schema: "public",
                table: "Emotions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TherapeuticTypes",
                schema: "public",
                table: "TherapeuticTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TherapeuticActivities",
                schema: "public",
                table: "TherapeuticActivities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhysicalActivities",
                schema: "public",
                table: "PhysicalActivities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PatientTherapeuticActivities",
                schema: "public",
                table: "PatientTherapeuticActivities",
                columns: new[] { "PatientProfileId", "TherapeuticActivityId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PatientPhysicalActivities",
                schema: "public",
                table: "PatientPhysicalActivities",
                columns: new[] { "PatientProfileId", "PhysicalActivityId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PatientImprovementGoals",
                schema: "public",
                table: "PatientImprovementGoals",
                columns: new[] { "PatientProfileId", "GoalId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PatientFoodActivities",
                schema: "public",
                table: "PatientFoodActivities",
                columns: new[] { "PatientProfileId", "FoodActivityId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PatientEntertainmentActivities",
                schema: "public",
                table: "PatientEntertainmentActivities",
                columns: new[] { "PatientProfileId", "EntertainmentActivityId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PatientEmotionCheckpoints",
                schema: "public",
                table: "PatientEmotionCheckpoints",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LifestyleLogs",
                schema: "public",
                table: "LifestyleLogs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImprovementGoals",
                schema: "public",
                table: "ImprovementGoals",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodNutrients",
                schema: "public",
                table: "FoodNutrients",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodCategories",
                schema: "public",
                table: "FoodCategories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodActivityFoodNutrient",
                schema: "public",
                table: "FoodActivityFoodNutrient",
                columns: new[] { "FoodActivitiesId", "FoodNutrientsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodActivityFoodCategory",
                schema: "public",
                table: "FoodActivityFoodCategory",
                columns: new[] { "FoodActivitiesId", "FoodCategoriesId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodActivities",
                schema: "public",
                table: "FoodActivities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EntertainmentActivities",
                schema: "public",
                table: "EntertainmentActivities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmotionSelections",
                schema: "public",
                table: "EmotionSelections",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmotionSelections_Emotions_EmotionId",
                schema: "public",
                table: "EmotionSelections",
                column: "EmotionId",
                principalSchema: "public",
                principalTable: "Emotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmotionSelections_PatientEmotionCheckpoints_EmotionCheckpoi~",
                schema: "public",
                table: "EmotionSelections",
                column: "EmotionCheckpointId",
                principalSchema: "public",
                principalTable: "PatientEmotionCheckpoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FoodActivityFoodCategory_FoodActivities_FoodActivitiesId",
                schema: "public",
                table: "FoodActivityFoodCategory",
                column: "FoodActivitiesId",
                principalSchema: "public",
                principalTable: "FoodActivities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FoodActivityFoodCategory_FoodCategories_FoodCategoriesId",
                schema: "public",
                table: "FoodActivityFoodCategory",
                column: "FoodCategoriesId",
                principalSchema: "public",
                principalTable: "FoodCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FoodActivityFoodNutrient_FoodActivities_FoodActivitiesId",
                schema: "public",
                table: "FoodActivityFoodNutrient",
                column: "FoodActivitiesId",
                principalSchema: "public",
                principalTable: "FoodActivities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FoodActivityFoodNutrient_FoodNutrients_FoodNutrientsId",
                schema: "public",
                table: "FoodActivityFoodNutrient",
                column: "FoodNutrientsId",
                principalSchema: "public",
                principalTable: "FoodNutrients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PatientImprovementGoals_ImprovementGoals_GoalId",
                schema: "public",
                table: "PatientImprovementGoals",
                column: "GoalId",
                principalSchema: "public",
                principalTable: "ImprovementGoals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TherapeuticActivities_TherapeuticTypes_TherapeuticTypeId",
                schema: "public",
                table: "TherapeuticActivities",
                column: "TherapeuticTypeId",
                principalSchema: "public",
                principalTable: "TherapeuticTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
