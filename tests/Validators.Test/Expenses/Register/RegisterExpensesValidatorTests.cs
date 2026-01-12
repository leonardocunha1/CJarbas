using CashFlow.Application.UseCases.Expenses.Register;
using CashFlow.Communication.Requests;

namespace Validators.Test.Expenses.Register;

public class RegisterExpensesValidatorTests
{
    [Fact]
    public void Success()
    {
        // Arrange
        var validator = new RegisterExpenseValidator();
        var request = new RequestRegisterExpenseJson
        {
            Amount = 100,
            Description = "Office supplies",
            Date = DateTime.UtcNow.AddDays(-1),
            Title = "Purchase of office supplies",
            PaymentType = CashFlow.Communication.Enums.PaymentType.CreditCard
        };
        // Act
        var result = validator.Validate(request);
        // Assert
        Assert.True(result.IsValid);
    }
}
