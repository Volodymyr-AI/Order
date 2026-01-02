using Order.Application.Orders.Commands.CreateOrder;
using FluentValidation.TestHelper;
using Xunit;

namespace xUnitTesting.ApplicationTests;

public sealed class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    [Fact]
    public void Should_have_error_when_items_empty()
    {
        var cmd = new CreateOrderCommand(
            CustomerId: Guid.NewGuid(),
            StoreId: 1,
            Items: new List<CustomerOrderItemDto>());
        
        var result = _validator.TestValidate(cmd);
        
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void Should_have_error_when_quantity_not_positive()
    {
        var cmd = new CreateOrderCommand(
            Guid.NewGuid(),
            1,
            new List<CustomerOrderItemDto>
            {
                new(Guid.NewGuid(), "Item A", 10m, "USD", 0)
            });
        
        var result = _validator.TestValidate(cmd);
         
        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Fact]
    public void Should_be_valid_for_correct_command()
    {
        var cmd = new CreateOrderCommand(
            Guid.NewGuid(),
            1,
            new List<CustomerOrderItemDto>
            {
                new(Guid.NewGuid(), "Item A", 10m, "USD", 2),
                new(Guid.NewGuid(), "Item B", 5m, "USD", 3),
            });
        
        var result = _validator.TestValidate(cmd);
        
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_have_error_when_currencies_different()
    {
        var cmd = new CreateOrderCommand(
            Guid.NewGuid(),
            1,
            new List<CustomerOrderItemDto>
            {
                new(Guid.NewGuid(), "Item A", 10m, "USD", 2),
                new(Guid.NewGuid(), "Item B", 5m, "EUR", 3),
            });
        
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("All items must have the same currency.")
            .WithErrorCode("currency_mismatch");
    }
}