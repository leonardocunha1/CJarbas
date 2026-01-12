using Bogus;
using CashFlow.Communication.Enums;
using CashFlow.Communication.Requests;

namespace CommonTestUtilities.Requests;

public class RequestRegisterExpenseJsonBuilder
{
    public static RequestRegisterExpenseJson Build()
    {
        /*
         * Maneira mais verbosa de fazer a mesma coisa que o código abaixo:
         * var faker = new Faker();
        var request = new RequestRegisterExpenseJson
        {
            Title = faker.Commerce.ProductName(),
            Description = faker.Commerce.ProductDescription(),
            Amount = faker.Finance.Amount(),
            Date = faker.Date.Past(),
            PaymentType = faker.PickRandom<PaymentType>()
        };
        */

        return new Faker<RequestRegisterExpenseJson>()
            .RuleFor(r => r.Title, faker => faker.Commerce.ProductName())
            .RuleFor(r => r.Description, faker => faker.Commerce.ProductDescription())
            .RuleFor(r => r.Amount, faker => faker.Finance.Amount())
            .RuleFor(r => r.Date, faker => faker.Date.Past())
            .RuleFor(r => r.PaymentType, faker => faker.PickRandom<PaymentType>());
    }
}
