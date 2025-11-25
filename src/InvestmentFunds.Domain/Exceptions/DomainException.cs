namespace InvestmentFunds.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class InsufficientBalanceException : DomainException
{
    public InsufficientBalanceException(string fundName) 
        : base($"No tiene saldo disponible para vincularse al fondo {fundName}") { }
}

public class FundNotFoundException : DomainException
{
    public FundNotFoundException(string fundId) 
        : base($"El fondo con ID {fundId} no existe") { }
}

public class SubscriptionNotFoundException : DomainException
{
    public SubscriptionNotFoundException() 
        : base("No se encontró la suscripción") { }
}

public class CustomerNotFoundException : DomainException
{
    public CustomerNotFoundException() 
        : base("Cliente no encontrado") { }
}
