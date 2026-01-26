using CashFlow.Domain.Enums;

namespace CashFlow.Domain.Extensions;

public static class PaymentTypeExtensions
{
    public static string PaymentTypeToString(this PaymentType paymentType)
    {
         return paymentType switch
         {
             PaymentType.Cash => "Dinheiro",
             PaymentType.CreditCard => "Cartão de Crédio",
             PaymentType.DebitCard => "Cartão de Débito",
             PaymentType.EletronicTransfer => "Transferencia Bancaria",
             _ => string.Empty
         };
    }
}
