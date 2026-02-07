namespace Forest.Api.Domain.Exceptions
{
    public sealed class DomainException : ApplicationException
    {
        public DomainException(string message) : base(message) { }
    }
}
