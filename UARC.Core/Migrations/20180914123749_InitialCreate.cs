using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Uarc.Core.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpcUaServer",
                columns: table => new
                {
                    OpcUaServerId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ServerLabel = table.Column<string>(nullable: false),
                    OpcUaUrl = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpcUaServer", x => x.OpcUaServerId);
                });

            migrationBuilder.CreateTable(
                name: "OpcUaVariable",
                columns: table => new
                {
                    OpcUaVariableId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    VarLabel = table.Column<string>(nullable: false),
                    NodeID = table.Column<string>(nullable: false),
                    OpcUaServerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpcUaVariable", x => x.OpcUaVariableId);
                    table.ForeignKey(
                        name: "FK_OpcUaVariable_OpcUaServer_OpcUaServerId",
                        column: x => x.OpcUaServerId,
                        principalTable: "OpcUaServer",
                        principalColumn: "OpcUaServerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpcUaWert",
                columns: table => new
                {
                    WertId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Zeitstempel = table.Column<DateTime>(nullable: false),
                    DisplayName = table.Column<string>(nullable: true),
                    Wert = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    SqlOpcUaVariableOpcUaVariableId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpcUaWert", x => x.WertId);
                    table.ForeignKey(
                        name: "FK_OpcUaWert_OpcUaVariable_SqlOpcUaVariableOpcUaVariableId",
                        column: x => x.SqlOpcUaVariableOpcUaVariableId,
                        principalTable: "OpcUaVariable",
                        principalColumn: "OpcUaVariableId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpcUaVariable_OpcUaServerId",
                table: "OpcUaVariable",
                column: "OpcUaServerId");

            migrationBuilder.CreateIndex(
                name: "IX_OpcUaWert_SqlOpcUaVariableOpcUaVariableId",
                table: "OpcUaWert",
                column: "SqlOpcUaVariableOpcUaVariableId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpcUaWert");

            migrationBuilder.DropTable(
                name: "OpcUaVariable");

            migrationBuilder.DropTable(
                name: "OpcUaServer");
        }
    }
}
