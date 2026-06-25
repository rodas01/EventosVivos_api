using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventosVivos_Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    cliente_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    correo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.cliente_id);
                });

            migrationBuilder.CreateTable(
                name: "estados_eventos",
                columns: table => new
                {
                    estado_evento_id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados_eventos", x => x.estado_evento_id);
                });

            migrationBuilder.CreateTable(
                name: "estados_reservas",
                columns: table => new
                {
                    estado_reserva_id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados_reservas", x => x.estado_reserva_id);
                });

            migrationBuilder.CreateTable(
                name: "tipos_eventos",
                columns: table => new
                {
                    tipo_evento_id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_eventos", x => x.tipo_evento_id);
                });

            migrationBuilder.CreateTable(
                name: "venues",
                columns: table => new
                {
                    venue_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    capacidad_maxima = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venues", x => x.venue_id);
                });

            migrationBuilder.CreateTable(
                name: "eventos",
                columns: table => new
                {
                    evento_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_evento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    id_venues = table.Column<int>(type: "int", nullable: false),
                    capacidad = table.Column<int>(type: "int", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    precio = table.Column<float>(type: "real", nullable: false),
                    id_tipo_evento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    id_estado_evento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos", x => x.evento_id);
                    table.ForeignKey(
                        name: "FK_eventos_estados_eventos_id_estado_evento",
                        column: x => x.id_estado_evento,
                        principalTable: "estados_eventos",
                        principalColumn: "estado_evento_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_eventos_tipos_eventos_id_tipo_evento",
                        column: x => x.id_tipo_evento,
                        principalTable: "tipos_eventos",
                        principalColumn: "tipo_evento_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_eventos_venues_id_venues",
                        column: x => x.id_venues,
                        principalTable: "venues",
                        principalColumn: "venue_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reservas",
                columns: table => new
                {
                    reserva_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo_reserva = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_evento = table.Column<int>(type: "int", nullable: false),
                    fecha_reserva = table.Column<DateTime>(type: "datetime2", nullable: false),
                    cantidad_entradas = table.Column<int>(type: "int", nullable: false),
                    precio_reserva = table.Column<float>(type: "real", nullable: false),
                    id_estado_reserva = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservas", x => x.reserva_id);
                    table.ForeignKey(
                        name: "FK_reservas_clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "clientes",
                        principalColumn: "cliente_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reservas_estados_reservas_id_estado_reserva",
                        column: x => x.id_estado_reserva,
                        principalTable: "estados_reservas",
                        principalColumn: "estado_reserva_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reservas_eventos_id_evento",
                        column: x => x.id_evento,
                        principalTable: "eventos",
                        principalColumn: "evento_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clientes_correo",
                table: "clientes",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_eventos_id_estado_evento",
                table: "eventos",
                column: "id_estado_evento");

            migrationBuilder.CreateIndex(
                name: "IX_eventos_id_tipo_evento",
                table: "eventos",
                column: "id_tipo_evento");

            migrationBuilder.CreateIndex(
                name: "IX_eventos_id_venues",
                table: "eventos",
                column: "id_venues");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_id_cliente",
                table: "reservas",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_id_estado_reserva",
                table: "reservas",
                column: "id_estado_reserva");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_id_evento",
                table: "reservas",
                column: "id_evento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reservas");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "estados_reservas");

            migrationBuilder.DropTable(
                name: "eventos");

            migrationBuilder.DropTable(
                name: "estados_eventos");

            migrationBuilder.DropTable(
                name: "tipos_eventos");

            migrationBuilder.DropTable(
                name: "venues");
        }
    }
}
