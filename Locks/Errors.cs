using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locks
{
    class ParseError : SystemException
    {

    }
    class RuntimeError : SystemException
    {
        public Token token { get; private set; }
        public RuntimeError(Token token, string message) : base(message)
        {
            this.token = token;
        }
    }
    class Return : SystemException
    {
        public object value { get; private set; }
        public Return(object value)
        {
            this.value = value;
        }
    }
}
