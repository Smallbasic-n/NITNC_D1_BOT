using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NITNC_D1_Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangedTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MatsudairaROles",
                table: "MatsudairaROles");

            migrationBuilder.RenameTable(
                name: "MatsudairaROles",
                newName: "MatsudairaRoles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MatsudairaRoles",
                table: "MatsudairaRoles",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MatsudairaRoles",
                table: "MatsudairaRoles");

            migrationBuilder.RenameTable(
                name: "MatsudairaRoles",
                newName: "MatsudairaROles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MatsudairaROles",
                table: "MatsudairaROles",
                column: "Id");
        }
    }
}
