using Microsoft.EntityFrameworkCore;
using Vylersant_Facturacion.Application.Authentication;
using Vylersant_Facturacion.Domain.Authentication;
using Vylersant_Facturacion.Infrastructure.Persistence;

namespace Vylersant_Facturacion.Infrastructure.Repositories;

public sealed class RefreshTokenRepository
    : IRefreshTokenRepository
{
    private readonly VylersantFacturacionDbContext _context;

    public RefreshTokenRepository(
        VylersantFacturacionDbContext context)
    {
        _context = context;
    }

    public Task<RefreshToken?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return _context.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);
    }

    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);
    }
}