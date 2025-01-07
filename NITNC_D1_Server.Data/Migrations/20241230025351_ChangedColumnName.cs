using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NITNC_D1_Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangedColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "MatsudairaRoles",
                newName: "DiscordRoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DiscordRoleId",
                table: "MatsudairaRoles",
                newName: "RoleId");
        }
    }
}
