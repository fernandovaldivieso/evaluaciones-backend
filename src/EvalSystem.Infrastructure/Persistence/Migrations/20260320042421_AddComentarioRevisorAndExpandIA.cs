using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EvalSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddComentarioRevisorAndExpandIA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RecomendacionIA",
                table: "ResultadosEvaluacion",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComentarioRevisor",
                table: "RespuestasCandidato",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComentarioRevisor",
                table: "RespuestasCandidato");

            migrationBuilder.AlterColumn<string>(
                name: "RecomendacionIA",
                table: "ResultadosEvaluacion",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 8000,
                oldNullable: true);
        }
    }
}
