using Microsoft.EntityFrameworkCore.Migrations;

namespace ShoppingCart.DurableFunction.Migrations
{
    public partial class AddStatusAndEmailToCart : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Carts",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Carts",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Carts");
        }
    }
}
