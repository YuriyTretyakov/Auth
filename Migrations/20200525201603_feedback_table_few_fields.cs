using Microsoft.EntityFrameworkCore.Migrations;

namespace Authorization.Migrations
{
    public partial class feedback_table_few_fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Feedback",
                newName: "Picture");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Feedback",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Feedback");

            migrationBuilder.RenameColumn(
                name: "Picture",
                table: "Feedback",
                newName: "UserId");
        }
    }
}
