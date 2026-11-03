using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ezekiel.TimpugClinicAppointmentQueueSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddContactInfoToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactInfo",
                table: "Appointments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactInfo",
                table: "Appointments");
        }
    }
}
