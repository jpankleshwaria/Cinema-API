using System;

namespace ApiApplication.Utility.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException()
        { }

        public DomainException(string message, Exception innerException)
           : base(message, innerException)
        { }
    }
}
