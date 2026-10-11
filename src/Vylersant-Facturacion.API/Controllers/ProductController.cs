using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vylersant_Facturacion.Application.Products;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Contracts.Products;

using CreateProductApplicationRequest =
    Vylersant_Facturacion.Application.Products.CreateProductRequest;

using UpdateProductApplicationRequest =
    Vylersant_Facturacion.Application.Products.UpdateProductRequest;

using CreateProductContractRequest =
    Vylersant_Facturacion.Contracts.Products.CreateProductRequest;

using UpdateProductContractRequest =
    Vylersant_Facturacion.Contracts.Products.UpdateProductRequest;

namespace Vylersant_Facturacion.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly CreateProductService _createProductService;
    private readonly GetProductService _getProductsService;
    private readonly GetProductByIdService _getProductByIdService;
    private readonly UpdateProductService _updateProductService;
    private readonly ChangeProductStatusService _changeProductStatusService;

    public ProductsController(
        CreateProductService createProductService,
        GetProductService getProductsService,
        GetProductByIdService getProductByIdService,
        UpdateProductService updateProductService,
        ChangeProductStatusService changeProductStatusService)
    {
        _createProductService = createProductService;
        _getProductsService = getProductsService;
        _getProductByIdService = getProductByIdService;
        _updateProductService = updateProductService;
        _changeProductStatusService = changeProductStatusService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateProductResponse>> Create(
        CreateProductContractRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        if (businessId is null)
        {
            return Unauthorized();
        }

        var applicationRequest =
            new CreateProductApplicationRequest(
                request.Name,
                request.Price,
                request.Sku,
                request.Description);

        var result =
            await _createProductService.ExecuteAsync(
                businessId.Value,
                applicationRequest,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.ProductId },
            new CreateProductResponse(
                result.ProductId));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        if (businessId is null)
        {
            return Unauthorized();
        }

        var products =
            await _getProductsService.ExecuteAsync(
                businessId.Value,
                cancellationToken);

        var response =
            products
                .Select(product =>
                    new ProductResponse(
                        product.Id,
                        product.Name,
                        product.Price,
                        product.Sku,
                        product.Description,
                        product.IsActive))
                .ToList();

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        if (businessId is null)
        {
            return Unauthorized();
        }

        var product =
            await _getProductByIdService.ExecuteAsync(
                id,
                businessId.Value,
                cancellationToken);

        return Ok(
            new ProductResponse(
                product.Id,
                product.Name,
                product.Price,
                product.Sku,
                product.Description,
                product.IsActive));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateProductContractRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        if (businessId is null)
        {
            return Unauthorized();
        }

        var applicationRequest =
            new UpdateProductApplicationRequest(
                request.Name,
                request.Price,
                request.Sku,
                request.Description);

        await _updateProductService.ExecuteAsync(
            id,
            businessId.Value,
            applicationRequest,
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        ChangeProductStatusRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = GetBusinessId();

        if (businessId is null)
        {
            return Unauthorized();
        }

        await _changeProductStatusService.ExecuteAsync(
            id,
            businessId.Value,
            request.IsActive,
            cancellationToken);

        return NoContent();
    }

    private Guid? GetBusinessId()
    {
        var businessIdClaim =
            User.FindFirst(
                TokenClaimNames.BusinessId)?.Value;

        if (!Guid.TryParse(
                businessIdClaim,
                out var businessId))
        {
            return null;
        }

        return businessId;
    }
}