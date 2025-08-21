using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Test.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdjustDbNamingConventionToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionOptions_TestQuestions_TestQuestionId",
                table: "QuestionOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionOptionTestResult_QuestionOptions_SelectedOptionsId",
                table: "QuestionOptionTestResult");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionOptionTestResult_TestResults_TestResultsId",
                table: "QuestionOptionTestResult");

            migrationBuilder.DropForeignKey(
                name: "FK_TestQuestions_Tests_TestId",
                table: "TestQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_TestResults_Tests_TestId",
                table: "TestResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tests",
                table: "Tests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestResults",
                table: "TestResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestQuestions",
                table: "TestQuestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuestionOptionTestResult",
                table: "QuestionOptionTestResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuestionOptions",
                table: "QuestionOptions");

            migrationBuilder.RenameTable(
                name: "Tests",
                newName: "tests");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "categories");

            migrationBuilder.RenameTable(
                name: "TestResults",
                newName: "test_results");

            migrationBuilder.RenameTable(
                name: "TestQuestions",
                newName: "test_questions");

            migrationBuilder.RenameTable(
                name: "QuestionOptionTestResult",
                newName: "question_option_test_result");

            migrationBuilder.RenameTable(
                name: "QuestionOptions",
                newName: "question_options");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "tests",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "tests",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "tests",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "tests",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "tests",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "tests",
                newName: "category_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "categories",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "categories",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "categories",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "categories",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "categories",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "categories",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "categories",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "test_results",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TestId",
                table: "test_results",
                newName: "test_id");

            migrationBuilder.RenameColumn(
                name: "TakenAt",
                table: "test_results",
                newName: "taken_at");

            migrationBuilder.RenameColumn(
                name: "StressScore",
                table: "test_results",
                newName: "stress_score");

            migrationBuilder.RenameColumn(
                name: "SeverityLevel",
                table: "test_results",
                newName: "severity_level");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "test_results",
                newName: "patient_id");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "test_results",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "test_results",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "DepressionScore",
                table: "test_results",
                newName: "depression_score");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "test_results",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "test_results",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AnxietyScore",
                table: "test_results",
                newName: "anxiety_score");

            migrationBuilder.RenameIndex(
                name: "IX_TestResults_TestId",
                table: "test_results",
                newName: "ix_test_results_test_id");

            migrationBuilder.RenameColumn(
                name: "Order",
                table: "test_questions",
                newName: "order");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "test_questions",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "test_questions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TestId",
                table: "test_questions",
                newName: "test_id");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "test_questions",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "test_questions",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "test_questions",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "test_questions",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_TestQuestions_TestId",
                table: "test_questions",
                newName: "ix_test_questions_test_id");

            migrationBuilder.RenameColumn(
                name: "TestResultsId",
                table: "question_option_test_result",
                newName: "test_results_id");

            migrationBuilder.RenameColumn(
                name: "SelectedOptionsId",
                table: "question_option_test_result",
                newName: "selected_options_id");

            migrationBuilder.RenameIndex(
                name: "IX_QuestionOptionTestResult_TestResultsId",
                table: "question_option_test_result",
                newName: "ix_question_option_test_result_test_results_id");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "question_options",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "question_options",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TestQuestionId",
                table: "question_options",
                newName: "test_question_id");

            migrationBuilder.RenameColumn(
                name: "QuestionId",
                table: "question_options",
                newName: "question_id");

            migrationBuilder.RenameColumn(
                name: "OptionValue",
                table: "question_options",
                newName: "option_value");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "question_options",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "question_options",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "question_options",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "question_options",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_QuestionOptions_TestQuestionId",
                table: "question_options",
                newName: "ix_question_options_test_question_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_tests",
                table: "tests",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_categories",
                table: "categories",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_test_results",
                table: "test_results",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_test_questions",
                table: "test_questions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_question_option_test_result",
                table: "question_option_test_result",
                columns: new[] { "selected_options_id", "test_results_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_question_options",
                table: "question_options",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_question_option_test_result_question_options_selected_optio",
                table: "question_option_test_result",
                column: "selected_options_id",
                principalTable: "question_options",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_question_option_test_result_test_results_test_results_id",
                table: "question_option_test_result",
                column: "test_results_id",
                principalTable: "test_results",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_question_options_test_questions_test_question_id",
                table: "question_options",
                column: "test_question_id",
                principalTable: "test_questions",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_test_questions_tests_test_id",
                table: "test_questions",
                column: "test_id",
                principalTable: "tests",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_test_results_tests_test_id",
                table: "test_results",
                column: "test_id",
                principalTable: "tests",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_question_option_test_result_question_options_selected_optio",
                table: "question_option_test_result");

            migrationBuilder.DropForeignKey(
                name: "fk_question_option_test_result_test_results_test_results_id",
                table: "question_option_test_result");

            migrationBuilder.DropForeignKey(
                name: "fk_question_options_test_questions_test_question_id",
                table: "question_options");

            migrationBuilder.DropForeignKey(
                name: "fk_test_questions_tests_test_id",
                table: "test_questions");

            migrationBuilder.DropForeignKey(
                name: "fk_test_results_tests_test_id",
                table: "test_results");

            migrationBuilder.DropPrimaryKey(
                name: "pk_tests",
                table: "tests");

            migrationBuilder.DropPrimaryKey(
                name: "pk_categories",
                table: "categories");

            migrationBuilder.DropPrimaryKey(
                name: "pk_test_results",
                table: "test_results");

            migrationBuilder.DropPrimaryKey(
                name: "pk_test_questions",
                table: "test_questions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_question_options",
                table: "question_options");

            migrationBuilder.DropPrimaryKey(
                name: "pk_question_option_test_result",
                table: "question_option_test_result");

            migrationBuilder.RenameTable(
                name: "tests",
                newName: "Tests");

            migrationBuilder.RenameTable(
                name: "categories",
                newName: "Categories");

            migrationBuilder.RenameTable(
                name: "test_results",
                newName: "TestResults");

            migrationBuilder.RenameTable(
                name: "test_questions",
                newName: "TestQuestions");

            migrationBuilder.RenameTable(
                name: "question_options",
                newName: "QuestionOptions");

            migrationBuilder.RenameTable(
                name: "question_option_test_result",
                newName: "QuestionOptionTestResult");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Tests",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                table: "Tests",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Tests",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "Tests",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Tests",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "category_id",
                table: "Tests",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Categories",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Categories",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Categories",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                table: "Categories",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Categories",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "Categories",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Categories",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "TestResults",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "test_id",
                table: "TestResults",
                newName: "TestId");

            migrationBuilder.RenameColumn(
                name: "taken_at",
                table: "TestResults",
                newName: "TakenAt");

            migrationBuilder.RenameColumn(
                name: "stress_score",
                table: "TestResults",
                newName: "StressScore");

            migrationBuilder.RenameColumn(
                name: "severity_level",
                table: "TestResults",
                newName: "SeverityLevel");

            migrationBuilder.RenameColumn(
                name: "patient_id",
                table: "TestResults",
                newName: "PatientId");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                table: "TestResults",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "TestResults",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "depression_score",
                table: "TestResults",
                newName: "DepressionScore");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "TestResults",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "TestResults",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "anxiety_score",
                table: "TestResults",
                newName: "AnxietyScore");

            migrationBuilder.RenameIndex(
                name: "ix_test_results_test_id",
                table: "TestResults",
                newName: "IX_TestResults_TestId");

            migrationBuilder.RenameColumn(
                name: "order",
                table: "TestQuestions",
                newName: "Order");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "TestQuestions",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "TestQuestions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "test_id",
                table: "TestQuestions",
                newName: "TestId");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                table: "TestQuestions",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "TestQuestions",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "TestQuestions",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "TestQuestions",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_test_questions_test_id",
                table: "TestQuestions",
                newName: "IX_TestQuestions_TestId");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "QuestionOptions",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "QuestionOptions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "test_question_id",
                table: "QuestionOptions",
                newName: "TestQuestionId");

            migrationBuilder.RenameColumn(
                name: "question_id",
                table: "QuestionOptions",
                newName: "QuestionId");

            migrationBuilder.RenameColumn(
                name: "option_value",
                table: "QuestionOptions",
                newName: "OptionValue");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                table: "QuestionOptions",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "QuestionOptions",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "QuestionOptions",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "QuestionOptions",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_question_options_test_question_id",
                table: "QuestionOptions",
                newName: "IX_QuestionOptions_TestQuestionId");

            migrationBuilder.RenameColumn(
                name: "test_results_id",
                table: "QuestionOptionTestResult",
                newName: "TestResultsId");

            migrationBuilder.RenameColumn(
                name: "selected_options_id",
                table: "QuestionOptionTestResult",
                newName: "SelectedOptionsId");

            migrationBuilder.RenameIndex(
                name: "ix_question_option_test_result_test_results_id",
                table: "QuestionOptionTestResult",
                newName: "IX_QuestionOptionTestResult_TestResultsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tests",
                table: "Tests",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestResults",
                table: "TestResults",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestQuestions",
                table: "TestQuestions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuestionOptions",
                table: "QuestionOptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuestionOptionTestResult",
                table: "QuestionOptionTestResult",
                columns: new[] { "SelectedOptionsId", "TestResultsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionOptions_TestQuestions_TestQuestionId",
                table: "QuestionOptions",
                column: "TestQuestionId",
                principalTable: "TestQuestions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionOptionTestResult_QuestionOptions_SelectedOptionsId",
                table: "QuestionOptionTestResult",
                column: "SelectedOptionsId",
                principalTable: "QuestionOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionOptionTestResult_TestResults_TestResultsId",
                table: "QuestionOptionTestResult",
                column: "TestResultsId",
                principalTable: "TestResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TestQuestions_Tests_TestId",
                table: "TestQuestions",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TestResults_Tests_TestId",
                table: "TestResults",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
