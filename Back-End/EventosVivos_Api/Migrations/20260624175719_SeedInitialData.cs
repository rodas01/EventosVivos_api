using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventosVivos_Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "estados_eventos",
                columns: new[] { "estado_evento_id", "descripcion" },
                values: new object[,]
                {
                    { "ACTIVO", "Evento activo y disponible para reservas" },
                    { "CANCELADO", "Evento cancelado y no disponible para reservas" }
                });

            migrationBuilder.InsertData(
                table: "estados_reservas",
                columns: new[] { "estado_reserva_id", "descripcion" },
                values: new object[,]
                {
                    { "CANCELADA", "Reserva cancelada y no disponible" },
                    { "COMFIRMADA", "Reserva pagada y confirmada" },
                    { "PAGO_PENDIENTE", "Reserva pendiente de pago y confirmación" },
                    { "PERDIDA", "Reserva perdida" }
                });

            migrationBuilder.InsertData(
                table: "tipos_eventos",
                columns: new[] { "tipo_evento_id", "descripcion" },
                values: new object[,]
                {
                    { "CONCIERTO", "Evento musical en vivo" },
                    { "CONFERENCIA", "Evento de presentación o charla" },
                    { "TALLER", "Evento de taller o workshop" }
                });

            migrationBuilder.InsertData(
                table: "venues",
                columns: new[] { "venue_id", "capacidad_maxima", "nombre" },
                values: new object[,]
                {
                    { 1, 500, "Teatro principal" },
                    { 2, 1200, "Centro Cultural Norte" },
                    { 3, 50, "Aula de arte 04" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "estados_eventos",
                keyColumn: "estado_evento_id",
                keyValue: "ACTIVO");

            migrationBuilder.DeleteData(
                table: "estados_eventos",
                keyColumn: "estado_evento_id",
                keyValue: "CANCELADO");

            migrationBuilder.DeleteData(
                table: "estados_eventos",
                keyColumn: "estado_evento_id",
                keyValue: "COMPLETADO");

            migrationBuilder.DeleteData(
                table: "estados_reservas",
                keyColumn: "estado_reserva_id",
                keyValue: "CANCELADA");

            migrationBuilder.DeleteData(
                table: "estados_reservas",
                keyColumn: "estado_reserva_id",
                keyValue: "COMFIRMADA");

            migrationBuilder.DeleteData(
                table: "estados_reservas",
                keyColumn: "estado_reserva_id",
                keyValue: "PAGO_PENDIENTE");

            migrationBuilder.DeleteData(
                table: "tipos_eventos",
                keyColumn: "tipo_evento_id",
                keyValue: "CONCIERTO");

            migrationBuilder.DeleteData(
                table: "tipos_eventos",
                keyColumn: "tipo_evento_id",
                keyValue: "CONFERENCIA");

            migrationBuilder.DeleteData(
                table: "tipos_eventos",
                keyColumn: "tipo_evento_id",
                keyValue: "TALLER");

            migrationBuilder.DeleteData(
                table: "venues",
                keyColumn: "venue_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "venues",
                keyColumn: "venue_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "venues",
                keyColumn: "venue_id",
                keyValue: 3);
        }
    }
}
