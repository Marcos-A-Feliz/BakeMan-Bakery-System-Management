using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BakeryControlSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialWithCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeDetails_Recipes_RecipeId1",
                table: "RecipeDetails");

            migrationBuilder.DropIndex(
                name: "IX_RecipeDetails_RecipeId1",
                table: "RecipeDetails");

            migrationBuilder.DropColumn(
                name: "RecipeId1",
                table: "RecipeDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecipeId1",
                table: "RecipeDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeDetails_RecipeId1",
                table: "RecipeDetails",
                column: "RecipeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeDetails_Recipes_RecipeId1",
                table: "RecipeDetails",
                column: "RecipeId1",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
