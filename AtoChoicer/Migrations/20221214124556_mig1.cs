using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AtoChoicer.Migrations
{
    public partial class mig1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "basicInfo",
                columns: table => new
                {
                    해당시간 = table.Column<DateTime>(nullable: false),
                    종목코드 = table.Column<string>(nullable: false),
                    종목명 = table.Column<string>(nullable: false),
                    상장주식 = table.Column<long>(nullable: false),
                    연중최고 = table.Column<int>(nullable: false),
                    연중최저 = table.Column<int>(nullable: false),
                    시가총액 = table.Column<long>(nullable: false),
                    외인소진률 = table.Column<double>(nullable: false),
                    최고250 = table.Column<int>(nullable: false),
                    최저250 = table.Column<int>(nullable: false),
                    시가 = table.Column<int>(nullable: false),
                    고가 = table.Column<int>(nullable: false),
                    저가 = table.Column<int>(nullable: false),
                    상한가 = table.Column<int>(nullable: false),
                    하한가 = table.Column<int>(nullable: false),
                    최고가250일 = table.Column<string>(nullable: true),
                    최저가250일 = table.Column<string>(nullable: true),
                    최고가250대비율 = table.Column<double>(nullable: false),
                    최저가250대비율 = table.Column<double>(nullable: false),
                    현재가 = table.Column<int>(nullable: false),
                    전일대비 = table.Column<int>(nullable: false),
                    등락율 = table.Column<double>(nullable: false),
                    거래량 = table.Column<int>(nullable: false),
                    유통주식 = table.Column<long>(nullable: false),
                    유통비율 = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "basicInfo");
        }
    }
}
