using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PredatorsGym.Migrations
{
    /// <inheritdoc />
    public partial class AddNivelActividadToPerfilUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NivelActividad",
                table: "PerfilesUsuarios",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NivelActividad",
                table: "PerfilesUsuarios");
        }
    }
}
