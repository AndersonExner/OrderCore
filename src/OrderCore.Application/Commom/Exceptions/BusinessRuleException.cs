using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCore.Application.Commom.Exceptions
{
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message)
        {
        }
    }
}
