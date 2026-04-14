using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCore.Application.Commom.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {
        }
    }
}
