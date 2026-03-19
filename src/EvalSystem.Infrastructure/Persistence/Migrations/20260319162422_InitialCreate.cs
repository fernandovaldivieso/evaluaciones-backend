using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EvalSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tecnologias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tecnologias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Rol = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Evaluaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Nivel = table.Column<int>(type: "int", nullable: false),
                    TiempoLimiteMinutos = table.Column<int>(type: "int", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    TecnologiaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Evaluaciones_Tecnologias_TecnologiaId",
                        column: x => x.TecnologiaId,
                        principalTable: "Tecnologias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcesosSeleccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Puesto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaLimite = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcesosSeleccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcesosSeleccion_Usuarios_CreadorId",
                        column: x => x.CreadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Expira = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Revocado = table.Column<bool>(type: "bit", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluacionSecciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    EvaluacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluacionSecciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluacionSecciones_Evaluaciones_EvaluacionId",
                        column: x => x.EvaluacionId,
                        principalTable: "Evaluaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcesoCandidatos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcesoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidatoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcesoCandidatos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcesoCandidatos_ProcesosSeleccion_ProcesoId",
                        column: x => x.ProcesoId,
                        principalTable: "ProcesosSeleccion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcesoCandidatos_Usuarios_CandidatoId",
                        column: x => x.CandidatoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcesoEvaluaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcesoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcesoEvaluaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcesoEvaluaciones_Evaluaciones_EvaluacionId",
                        column: x => x.EvaluacionId,
                        principalTable: "Evaluaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcesoEvaluaciones_ProcesosSeleccion_ProcesoId",
                        column: x => x.ProcesoId,
                        principalTable: "ProcesosSeleccion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SesionesEvaluacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScoreObtenido = table.Column<int>(type: "int", nullable: true),
                    ScoreMaximo = table.Column<int>(type: "int", nullable: false),
                    CandidatoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcesoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionesEvaluacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SesionesEvaluacion_Evaluaciones_EvaluacionId",
                        column: x => x.EvaluacionId,
                        principalTable: "Evaluaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SesionesEvaluacion_ProcesosSeleccion_ProcesoId",
                        column: x => x.ProcesoId,
                        principalTable: "ProcesosSeleccion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SesionesEvaluacion_Usuarios_CandidatoId",
                        column: x => x.CandidatoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Preguntas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Puntaje = table.Column<int>(type: "int", nullable: false),
                    TiempoSegundos = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Explicacion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SeccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preguntas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Preguntas_EvaluacionSecciones_SeccionId",
                        column: x => x.SeccionId,
                        principalTable: "EvaluacionSecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResultadosEvaluacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScoreTotal = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ScorePorSeccion = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    BrechasIdentificadas = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RecomendacionIA = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    FortalezasIdentificadas = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    FechaAnalisis = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SesionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultadosEvaluacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultadosEvaluacion_SesionesEvaluacion_SesionId",
                        column: x => x.SesionId,
                        principalTable: "SesionesEvaluacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpcionesRespuesta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EsCorrecta = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    PreguntaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpcionesRespuesta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpcionesRespuesta_Preguntas_PreguntaId",
                        column: x => x.PreguntaId,
                        principalTable: "Preguntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RespuestasCandidato",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Respuesta = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    TiempoRespuestaSegundos = table.Column<int>(type: "int", nullable: false),
                    EsCorrecta = table.Column<bool>(type: "bit", nullable: true),
                    PuntajeObtenido = table.Column<int>(type: "int", nullable: true),
                    SesionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreguntaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpcionSeleccionadaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespuestasCandidato", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RespuestasCandidato_OpcionesRespuesta_OpcionSeleccionadaId",
                        column: x => x.OpcionSeleccionadaId,
                        principalTable: "OpcionesRespuesta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RespuestasCandidato_Preguntas_PreguntaId",
                        column: x => x.PreguntaId,
                        principalTable: "Preguntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RespuestasCandidato_SesionesEvaluacion_SesionId",
                        column: x => x.SesionId,
                        principalTable: "SesionesEvaluacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Evaluaciones_TecnologiaId",
                table: "Evaluaciones",
                column: "TecnologiaId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionSecciones_EvaluacionId",
                table: "EvaluacionSecciones",
                column: "EvaluacionId");

            migrationBuilder.CreateIndex(
                name: "IX_OpcionesRespuesta_PreguntaId",
                table: "OpcionesRespuesta",
                column: "PreguntaId");

            migrationBuilder.CreateIndex(
                name: "IX_Preguntas_SeccionId",
                table: "Preguntas",
                column: "SeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcesoCandidatos_CandidatoId",
                table: "ProcesoCandidatos",
                column: "CandidatoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcesoCandidatos_ProcesoId_CandidatoId",
                table: "ProcesoCandidatos",
                columns: new[] { "ProcesoId", "CandidatoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcesoEvaluaciones_EvaluacionId",
                table: "ProcesoEvaluaciones",
                column: "EvaluacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcesoEvaluaciones_ProcesoId_EvaluacionId",
                table: "ProcesoEvaluaciones",
                columns: new[] { "ProcesoId", "EvaluacionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcesosSeleccion_CreadorId",
                table: "ProcesosSeleccion",
                column: "CreadorId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UsuarioId",
                table: "RefreshTokens",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasCandidato_OpcionSeleccionadaId",
                table: "RespuestasCandidato",
                column: "OpcionSeleccionadaId");

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasCandidato_PreguntaId",
                table: "RespuestasCandidato",
                column: "PreguntaId");

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasCandidato_SesionId_PreguntaId",
                table: "RespuestasCandidato",
                columns: new[] { "SesionId", "PreguntaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResultadosEvaluacion_SesionId",
                table: "ResultadosEvaluacion",
                column: "SesionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SesionesEvaluacion_CandidatoId",
                table: "SesionesEvaluacion",
                column: "CandidatoId");

            migrationBuilder.CreateIndex(
                name: "IX_SesionesEvaluacion_EvaluacionId",
                table: "SesionesEvaluacion",
                column: "EvaluacionId");

            migrationBuilder.CreateIndex(
                name: "IX_SesionesEvaluacion_ProcesoId",
                table: "SesionesEvaluacion",
                column: "ProcesoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tecnologias_Nombre",
                table: "Tecnologias",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcesoCandidatos");

            migrationBuilder.DropTable(
                name: "ProcesoEvaluaciones");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RespuestasCandidato");

            migrationBuilder.DropTable(
                name: "ResultadosEvaluacion");

            migrationBuilder.DropTable(
                name: "OpcionesRespuesta");

            migrationBuilder.DropTable(
                name: "SesionesEvaluacion");

            migrationBuilder.DropTable(
                name: "Preguntas");

            migrationBuilder.DropTable(
                name: "ProcesosSeleccion");

            migrationBuilder.DropTable(
                name: "EvaluacionSecciones");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Evaluaciones");

            migrationBuilder.DropTable(
                name: "Tecnologias");
        }
    }
}
