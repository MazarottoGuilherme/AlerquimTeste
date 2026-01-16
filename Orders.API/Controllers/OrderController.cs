using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.DTOs;
using Orders.Application.Services;
using Orders.Domain;
using Orders.Domain.Orders;

namespace Orders.API.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController  : ControllerBase
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [Authorize]
    [HttpPost("CreateOrder")]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        try
        {
            var userDto = await _orderService.CreateOrderAsync(request);
            return Ok(userDto);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("GetAllOrders")]
    public IActionResult GetAllOrders()
    {
        try
        {
            var userDto = _orderService.GetAllOrders();
            return Ok(userDto);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public IActionResult GetOrderById(Guid id)
    {
        try
        {
            var orderId = new OrderId(id);

            var userDto = _orderService.GetOrderById(orderId);
            return Ok(userDto);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        return Ok();
    }

}