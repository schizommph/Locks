using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locks
{
    static class ReservedKeywords
    {
        public static Dictionary<string, TokenType> reservedKeywords { get; private set; } = new Dictionary<string, TokenType>()
        {
            ["and"] = TokenType.AND,
            ["class"] = TokenType.CLASS,
            ["else"] = TokenType.ELSE,
            ["false"] = TokenType.FALSE,
            ["for"] = TokenType.FOR,
            ["fun"] = TokenType.FUN,
            ["if"] = TokenType.IF,
            ["nil"] = TokenType.NIL,
            ["or"] = TokenType.OR,
            ["print"] = TokenType.PRINT,
            ["return"] = TokenType.RETURN,
            ["super"] = TokenType.SUPER,
            ["this"] = TokenType.THIS,
            ["true"] = TokenType.TRUE,
            ["var"] = TokenType.VAR,
            ["while"] = TokenType.WHILE,
        };
    }
}
