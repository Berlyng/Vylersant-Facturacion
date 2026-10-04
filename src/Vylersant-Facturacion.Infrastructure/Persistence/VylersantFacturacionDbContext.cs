using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Infrastructure.Persistence
{
    public sealed class VylersantFacturacionDbContext : DbContext
    {
        public VylersantFacturacionDbContext(DbContextOptions<VylersantFacturacionDbContext> options)
            : base(options)
        {

        }
    }
}
