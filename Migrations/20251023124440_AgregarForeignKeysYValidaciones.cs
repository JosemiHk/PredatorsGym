using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PredatorsGym.Migrations
{
    /// <inheritdoc />
    public partial class AgregarForeignKeysYValidaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_PerfilesUsuarios_UsuarioId",
                table: "PerfilesUsuarios",
                newName: "IX_PerfilesUsuarios_UsuarioId_Unique");

            migrationBuilder.AlterColumn<string>(
                name: "RutinaGenerada",
                table: "Rutinas",
                type: "nvarchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "Objetivo",
                table: "Rutinas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LugarEntrenamiento",
                table: "Rutinas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Genero",
                table: "Rutinas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Experiencia",
                table: "Rutinas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EstadoIMC",
                table: "Rutinas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DiasEntrenamiento",
                table: "Rutinas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CreadoPor",
                table: "Rutinas",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "Rutinas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModificadoPor",
                table: "Rutinas",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rutinas_Usuario_Fecha_Estado",
                table: "Rutinas",
                columns: new[] { "UsuarioId", "FechaCreacion", "Estado" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Rutinas_AlturaValida",
                table: "Rutinas",
                sql: "[Altura] >= 50 AND [Altura] <= 300");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Rutinas_EdadValida",
                table: "Rutinas",
                sql: "[Edad] >= 12 AND [Edad] <= 120");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Rutinas_PesoValido",
                table: "Rutinas",
                sql: "[Peso] > 0 AND [Peso] <= 500");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PerfilesUsuarios_AlturaValida",
                table: "PerfilesUsuarios",
                sql: "[Altura] IS NULL OR ([Altura] >= 50 AND [Altura] <= 300)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PerfilesUsuarios_DiasValidos",
                table: "PerfilesUsuarios",
                sql: "[DiasEntrenamientoSemana] IS NULL OR ([DiasEntrenamientoSemana] >= 1 AND [DiasEntrenamientoSemana] <= 7)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PerfilesUsuarios_PesoValido",
                table: "PerfilesUsuarios",
                sql: "[PesoActual] IS NULL OR ([PesoActual] > 0 AND [PesoActual] <= 500)");

            migrationBuilder.CreateIndex(
                name: "IX_Membresias_Usuario_Activa_Fin",
                table: "Membresias",
                columns: new[] { "UsuarioId", "EsActiva", "FechaFin" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Membresias_FechaFinMayor",
                table: "Membresias",
                sql: "[FechaFin] > [FechaInicio]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Membresias_PrecioPositivo",
                table: "Membresias",
                sql: "[Precio] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Membresias_TipoValido",
                table: "Membresias",
                sql: "[TipoMembresia] IN ('Básico', 'Premium', 'Elite')");

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_RutinaId_Orden",
                table: "Ejercicios",
                columns: new[] { "RutinaId", "Orden" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ejercicios_OrdenValido",
                table: "Ejercicios",
                sql: "[Orden] >= 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ejercicios_RepeticionesValidas",
                table: "Ejercicios",
                sql: "[Repeticiones] >= 1 AND [Repeticiones] <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ejercicios_SeriesValidas",
                table: "Ejercicios",
                sql: "[Series] >= 1 AND [Series] <= 10");

            migrationBuilder.AddForeignKey(
                name: "FK_Membresias_AspNetUsers_UsuarioId",
                table: "Membresias",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PerfilesUsuarios_AspNetUsers_UsuarioId",
                table: "PerfilesUsuarios",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rutinas_AspNetUsers_UsuarioId",
                table: "Rutinas",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Membresias_AspNetUsers_UsuarioId",
                table: "Membresias");

            migrationBuilder.DropForeignKey(
                name: "FK_PerfilesUsuarios_AspNetUsers_UsuarioId",
                table: "PerfilesUsuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Rutinas_AspNetUsers_UsuarioId",
                table: "Rutinas");

            migrationBuilder.DropIndex(
                name: "IX_Rutinas_Usuario_Fecha_Estado",
                table: "Rutinas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Rutinas_AlturaValida",
                table: "Rutinas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Rutinas_EdadValida",
                table: "Rutinas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Rutinas_PesoValido",
                table: "Rutinas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PerfilesUsuarios_AlturaValida",
                table: "PerfilesUsuarios");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PerfilesUsuarios_DiasValidos",
                table: "PerfilesUsuarios");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PerfilesUsuarios_PesoValido",
                table: "PerfilesUsuarios");

            migrationBuilder.DropIndex(
                name: "IX_Membresias_Usuario_Activa_Fin",
                table: "Membresias");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Membresias_FechaFinMayor",
                table: "Membresias");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Membresias_PrecioPositivo",
                table: "Membresias");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Membresias_TipoValido",
                table: "Membresias");

            migrationBuilder.DropIndex(
                name: "IX_Ejercicios_RutinaId_Orden",
                table: "Ejercicios");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ejercicios_OrdenValido",
                table: "Ejercicios");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ejercicios_RepeticionesValidas",
                table: "Ejercicios");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ejercicios_SeriesValidas",
                table: "Ejercicios");

            migrationBuilder.DropColumn(
                name: "CreadoPor",
                table: "Rutinas");

            migrationBuilder.DropColumn(
                name: "FechaModificacion",
                table: "Rutinas");

            migrationBuilder.DropColumn(
                name: "ModificadoPor",
                table: "Rutinas");

            migrationBuilder.RenameIndex(
                name: "IX_PerfilesUsuarios_UsuarioId_Unique",
                table: "PerfilesUsuarios",
                newName: "IX_PerfilesUsuarios_UsuarioId");

            migrationBuilder.AlterColumn<string>(
                name: "RutinaGenerada",
                table: "Rutinas",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)");

            migrationBuilder.AlterColumn<string>(
                name: "Objetivo",
                table: "Rutinas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "LugarEntrenamiento",
                table: "Rutinas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Genero",
                table: "Rutinas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Experiencia",
                table: "Rutinas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "EstadoIMC",
                table: "Rutinas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "DiasEntrenamiento",
                table: "Rutinas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
