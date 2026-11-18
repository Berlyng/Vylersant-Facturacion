using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Vylersant_Facturacion.Infrastructure.Persistence;

#nullable disable

namespace Vylersant_Facturacion.Infrastructure.Persistence.Migrations;

[DbContext(typeof(VylersantFacturacionDbContext))]
[Migration("20261118000000_AddInvoiceIssuer")]
public partial class AddInvoiceIssuer : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "IssuedByUserId",
            table: "Invoices",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Invoices_IssuedByUserId",
            table: "Invoices",
            column: "IssuedByUserId");

        migrationBuilder.AddForeignKey(
            name: "FK_Invoices_Users_IssuedByUserId",
            table: "Invoices",
            column: "IssuedByUserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Invoices_Users_IssuedByUserId",
            table: "Invoices");

        migrationBuilder.DropIndex(
            name: "IX_Invoices_IssuedByUserId",
            table: "Invoices");

        migrationBuilder.DropColumn(
            name: "IssuedByUserId",
            table: "Invoices");
    }
}

