using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Application.Services;
using Catalog.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController  : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("CreateProduct")]
    public IActionResult CreateProduct(CreateProductRequest request)
    {
        try
        {
            var userDto = _productService.CreateProduct(request);
            return Ok(userDto);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("AddStock")]
    public IActionResult AddStock(Guid productId, int quantity, string invoiceNumber)
    {
        try
        {
            _productService.AddStock(productId, quantity, invoiceNumber);
            return Ok();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    [Authorize]
    [HttpGet("GetAll")]
    public IActionResult GetAll()
    {
        try
        {
            var userDto = _productService.GetAll();
            return Ok(userDto);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("{id}")]
    public IActionResult GetProductById(Guid id)
    {
        try
        {
            var productDto = _productService.GetProductById(id);
            return Ok(productDto);
        }
        catch (DomainException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public IActionResult UpdateProduct(Guid id, UpdateProductRequest request)
    {
        try
        {
            var productDto = _productService.UpdateProduct(id, request);
            return Ok(productDto);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(Guid id)
    {
        try
        {
            _productService.DeleteProduct(id);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}