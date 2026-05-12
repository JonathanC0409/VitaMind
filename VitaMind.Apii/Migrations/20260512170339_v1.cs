using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace VitaMind.Apii.Migrations
{
    /// <inheritdoc />
    public partial class v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "MetricaSistemas",
                columns: table => new
                {
                    IdMetrica = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreMetrica = table.Column<string>(type: "text", nullable: false),
                    ValorNumerico = table.Column<decimal>(type: "numeric", nullable: false),
                    Unidad = table.Column<string>(type: "text", nullable: true),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    FechaCalculo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricaSistemas", x => x.IdMetrica);
                });

            migrationBuilder.CreateTable(
                name: "ServicioEstados",
                columns: table => new
                {
                    IdServicio = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreServicio = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    UltimoChequeo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Mensaje = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicioEstados", x => x.IdServicio);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Correo = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Rol = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "BusquedaSemanticas",
                columns: table => new
                {
                    IdBusqueda = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    Consulta = table.Column<string>(type: "text", nullable: false),
                    EmbeddingConsulta = table.Column<Vector>(type: "vector(1536)", nullable: true),
                    TiempoBusquedaMs = table.Column<int>(type: "integer", nullable: false),
                    TotalResultados = table.Column<int>(type: "integer", nullable: false),
                    SimilitudMaxima = table.Column<decimal>(type: "numeric", nullable: false),
                    SimilitudPromedio = table.Column<decimal>(type: "numeric", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusquedaSemanticas", x => x.IdBusqueda);
                    table.ForeignKey(
                        name: "FK_BusquedaSemanticas_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsultaAgentes",
                columns: table => new
                {
                    IdConsulta = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    Pregunta = table.Column<string>(type: "text", nullable: false),
                    Respuesta = table.Column<string>(type: "text", nullable: true),
                    AccionDetectada = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    TiempoConsultaMs = table.Column<int>(type: "integer", nullable: false),
                    RegistrosRecuperados = table.Column<int>(type: "integer", nullable: false),
                    SimilitudPromedio = table.Column<decimal>(type: "numeric", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultaAgentes", x => x.IdConsulta);
                    table.ForeignKey(
                        name: "FK_ConsultaAgentes_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErrorIngresos",
                columns: table => new
                {
                    IdError = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: true),
                    CampoAfectado = table.Column<string>(type: "text", nullable: true),
                    TipoError = table.Column<string>(type: "text", nullable: false),
                    MensajeError = table.Column<string>(type: "text", nullable: false),
                    DatosIntentados = table.Column<string>(type: "text", nullable: true),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorIngresos", x => x.IdError);
                    table.ForeignKey(
                        name: "FK_ErrorIngresos_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LogOperaciones",
                columns: table => new
                {
                    IdLog = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: true),
                    TipoOperacion = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    TiempoRespuestaMs = table.Column<int>(type: "integer", nullable: false),
                    Origen = table.Column<string>(type: "text", nullable: true),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogOperaciones", x => x.IdLog);
                    table.ForeignKey(
                        name: "FK_LogOperaciones_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosSalud",
                columns: table => new
                {
                    IdRegistro = table.Column<Guid>(type: "uuid", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uuid", nullable: false),
                    Categoria = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Intensidad = table.Column<string>(type: "text", nullable: true),
                    Observacion = table.Column<string>(type: "text", nullable: true),
                    TipoEntrada = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HoraRegistro = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EstadoInsercion = table.Column<string>(type: "text", nullable: false),
                    LatenciaMs = table.Column<int>(type: "integer", nullable: false),
                    TiempoEmbeddingMs = table.Column<int>(type: "integer", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: true),
                    DimensionEmbedding = table.Column<int>(type: "integer", nullable: false),
                    EsSimilar = table.Column<bool>(type: "boolean", nullable: false),
                    SimilitudDetectada = table.Column<decimal>(type: "numeric", nullable: false),
                    IdRegistroSimilar = table.Column<Guid>(type: "uuid", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosSalud", x => x.IdRegistro);
                    table.ForeignKey(
                        name: "FK_RegistrosSalud_RegistrosSalud_IdRegistroSimilar",
                        column: x => x.IdRegistroSimilar,
                        principalTable: "RegistrosSalud",
                        principalColumn: "IdRegistro",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RegistrosSalud_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResultadoBusquedas",
                columns: table => new
                {
                    IdResultado = table.Column<Guid>(type: "uuid", nullable: false),
                    IdBusqueda = table.Column<Guid>(type: "uuid", nullable: false),
                    IdRegistro = table.Column<Guid>(type: "uuid", nullable: false),
                    NivelSimilitud = table.Column<decimal>(type: "numeric", nullable: false),
                    PosicionResultado = table.Column<int>(type: "integer", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultadoBusquedas", x => x.IdResultado);
                    table.ForeignKey(
                        name: "FK_ResultadoBusquedas_BusquedaSemanticas_IdBusqueda",
                        column: x => x.IdBusqueda,
                        principalTable: "BusquedaSemanticas",
                        principalColumn: "IdBusqueda",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResultadoBusquedas_RegistrosSalud_IdRegistro",
                        column: x => x.IdRegistro,
                        principalTable: "RegistrosSalud",
                        principalColumn: "IdRegistro",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusquedaSemanticas_IdUsuario",
                table: "BusquedaSemanticas",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultaAgentes_IdUsuario",
                table: "ConsultaAgentes",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorIngresos_IdUsuario",
                table: "ErrorIngresos",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_LogOperaciones_IdUsuario",
                table: "LogOperaciones",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosSalud_IdRegistroSimilar",
                table: "RegistrosSalud",
                column: "IdRegistroSimilar");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosSalud_IdUsuario",
                table: "RegistrosSalud",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_ResultadoBusquedas_IdBusqueda",
                table: "ResultadoBusquedas",
                column: "IdBusqueda");

            migrationBuilder.CreateIndex(
                name: "IX_ResultadoBusquedas_IdRegistro",
                table: "ResultadoBusquedas",
                column: "IdRegistro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultaAgentes");

            migrationBuilder.DropTable(
                name: "ErrorIngresos");

            migrationBuilder.DropTable(
                name: "LogOperaciones");

            migrationBuilder.DropTable(
                name: "MetricaSistemas");

            migrationBuilder.DropTable(
                name: "ResultadoBusquedas");

            migrationBuilder.DropTable(
                name: "ServicioEstados");

            migrationBuilder.DropTable(
                name: "BusquedaSemanticas");

            migrationBuilder.DropTable(
                name: "RegistrosSalud");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
