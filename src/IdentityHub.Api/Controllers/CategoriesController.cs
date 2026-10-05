using IdentityHub.Api.Contracts;
using IdentityHub.Api.Authorization;
using IdentityHub.Application.Common.Models;
using IdentityHub.Application.Features.Categories.Commands.CreateCategory;
using IdentityHub.Application.Features.Categories.Commands.DeleteCategory;
using IdentityHub.Application.Features.Categories.Commands.UpdateCategory;
using IdentityHub.Application.Features.Categories.Queries.GetCategories;
using IdentityHub.Application.Features.Categories.Queries.GetCategoryById;
using IdentityHub.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CategoriesController(ISender sender) : ControllerBase
{
    /// <summary>Lists all categories. Public/anonymous access for catalog browsing.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken ct)
        => Ok(await sender.Send(new GetCategoriesQuery(), ct));

    /// <summary>Lists categories with search, active status filter, and pagination. Public/anonymous access for catalog browsing.</summary>
    [HttpGet("paged")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<CategoryDto>>> GetPaged(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetCategoriesPagedQuery(search, isActive, page, pageSize), ct));

    /// <summary>Gets a single category by id. Public/anonymous access for catalog browsing.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken ct)
    {
        var category = await sender.Send(new GetCategoryByIdQuery(id), ct);
        return category is null ? NotFound() : Ok(category);
    }

    /// <summary>Creates a new category master with unit of measurement and variant definitions.</summary>
    [HttpPost]
    [RequireSection("section-categories-manage")]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request, CancellationToken ct)
    {
        var variantInputs = (request.VariantDefinitions ?? [])
            .Select(v => new CategoryVariantDefinitionInput(v.Id, v.Name, v.Type, v.Values, v.IsRequired))
            .ToList();

        var result = await sender.Send(new CreateCategoryCommand(
            request.Name,
            request.Description,
            request.IsActive,
            request.UnitOfMeasurement,
            variantInputs), ct);

        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data)
            : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Updates an existing category.</summary>
    [HttpPut("{id:guid}")]
    [RequireSection("section-categories-manage")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request, CancellationToken ct)
    {
        var variantInputs = (request.VariantDefinitions ?? [])
            .Select(v => new CategoryVariantDefinitionInput(v.Id, v.Name, v.Type, v.Values, v.IsRequired))
            .ToList();

        var result = await sender.Send(new UpdateCategoryCommand(
            id,
            request.Name,
            request.Description,
            request.IsActive,
            request.UnitOfMeasurement,
            variantInputs), ct);

        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Soft-deletes a category.</summary>
    [HttpDelete("{id:guid}")]
    [RequireSection("section-categories-manage")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteCategoryCommand(id), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }
}
