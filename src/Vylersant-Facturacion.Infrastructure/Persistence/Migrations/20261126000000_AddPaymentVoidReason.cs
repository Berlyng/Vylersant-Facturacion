using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Vylersant_Facturacion.Infrastructure.Persistence;

#nullable disable

namespace Vylersant_Facturacion.Infrastructure.Persistence.Migrations;

[DbContext(typeof(VylersantFacturacionDbContext))]
[Migration("20261126000000_AddPaymentVoidReason")]
public sealed class AddPaymentVoidReason : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<string>(name: "VoidReason", table: "Payments", type: "nvarchar(500)", maxLength: 500, nullable: true);

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(name: "VoidReason", table: "Payments");
}

