using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Vylersant_Facturacion.Infrastructure.Persistence;

#nullable disable

namespace Vylersant_Facturacion.Infrastructure.Persistence.Migrations;

[DbContext(typeof(VylersantFacturacionDbContext))]
[Migration("20261120000000_AddPayments")]
public sealed class AddPayments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Payments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                Method = table.Column<int>(type: "int", nullable: false),
                Reference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Status = table.Column<int>(type: "int", nullable: false),
                VoidedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Payments", x => x.Id);
                table.ForeignKey("FK_Payments_Businesses_BusinessId", x => x.BusinessId, "Businesses", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Payments_Invoices_InvoiceId", x => x.InvoiceId, "Invoices", "Id", onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex("IX_Payments_BusinessId_InvoiceId_Status", "Payments", new[] { "BusinessId", "InvoiceId", "Status" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("Payments");
}

