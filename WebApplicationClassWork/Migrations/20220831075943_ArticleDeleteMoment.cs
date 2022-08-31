using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApplicationClassWork.Migrations
{
    public partial class ArticleDeleteMoment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c15bad12-1159-4d7d-ae25-11d43be2c12f"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteMoment",
                table: "Articles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "Email", "LogMoment", "Login", "PassHash", "PassSalt", "RealName", "RegMoment" },
                values: new object[] { new Guid("209286d1-2f97-4ae2-8ba6-d53c4e3626f8"), "", "", null, "Admin", "", "", "Корневой администратор", new DateTime(2022, 8, 31, 10, 59, 43, 428, DateTimeKind.Local).AddTicks(5949) });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_ReplyId",
                table: "Articles",
                column: "ReplyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Articles_ReplyId",
                table: "Articles",
                column: "ReplyId",
                principalTable: "Articles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Articles_ReplyId",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_ReplyId",
                table: "Articles");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("209286d1-2f97-4ae2-8ba6-d53c4e3626f8"));

            migrationBuilder.DropColumn(
                name: "DeleteMoment",
                table: "Articles");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "Email", "LogMoment", "Login", "PassHash", "PassSalt", "RealName", "RegMoment" },
                values: new object[] { new Guid("c15bad12-1159-4d7d-ae25-11d43be2c12f"), "", "", null, "Admin", "", "", "Корневой администратор", new DateTime(2022, 8, 22, 18, 31, 34, 406, DateTimeKind.Local).AddTicks(6569) });
        }
    }
}
