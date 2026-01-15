namespace DnsResolver.Domain.Exceptions;

public class InvalidDomainException : DomainException
{
    public InvalidDomainException(string message) : base(message) { }
}
