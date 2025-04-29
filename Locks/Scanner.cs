using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locks
{
    class Scanner
    {
        private string source;
        private List<Token> tokens;
        private int start, current;
        private int line;

        public Scanner(string source)
        {
            this.source = source;
            this.tokens = new List<Token>();
            this.start = 0;
            this.current = 0;
            this.line = 1;
        }
        public List<Token> ScanTokens()
        {
            while(!AtEnd())
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }
        // 46
        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.PERIOD); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case '*': AddToken(TokenType.STAR); break;
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && !AtEnd()) Advance();
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    line++;
                    break;
                case '"': GetString(); break;
                default:
                    if (char.IsDigit(c))
                    {
                        GetNumber();
                    }
                    else if (char.IsAsciiLetter(c) || c == '_')
                    {
                        GetIdentifier();
                    }
                    else
                    {
                        Program.Error(line, $"Unexpected character.");
                    }
                    break;
            }
        }
        // 57
        private void GetIdentifier()
        {
            while (char.IsAsciiLetterOrDigit(Peek()) || Peek() == '_')
                Advance();


            string value = source.Substring(start, current - start);
            TokenType type = TokenType.IDENTIFIER;
            if (ReservedKeywords.reservedKeywords.ContainsKey(value))
                type = ReservedKeywords.reservedKeywords[value];

            AddToken(type, value);
        }
        private void GetNumber()
        {
            while (char.IsDigit(Peek())) Advance();

            if(Peek() == '.' && char.IsDigit(PeekNext()))
            {
                Advance();

                while (char.IsDigit(Peek())) Advance();
            }

            string value = source.Substring(start, current - start);
            AddToken(TokenType.NUMBER, Double.Parse(value));
        }
        private void GetString()
        {
            while(Peek() != '"' && !AtEnd())
            {
                if (Peek() == '\n') line++;
                Advance();
            }
            if (AtEnd())
            {
                Program.Error(line, "Unterminated string");
                return;
            }
            Advance();

            string value = source.Substring(start + 1, current - start - 2);
            AddToken(TokenType.STRING, value);
        }
        private char Advance()
        {
            if (AtEnd())
                return '\0';
            current++;
            return source[current - 1];
        }
        private char Peek()
        {
            if (AtEnd())
                return '\0';
            return source[current];
        }
        private char PeekNext()
        {
            if (current + 1 >= source.Length)
                return '\0';
            return source[current + 1];
        }
        private bool Match(char c)
        {
            if (AtEnd() || c != source[current])
                return false;
            current++;
            return true;
        }
        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }
        private void AddToken(TokenType type, object literal)
        {
            string lexeme = source.Substring(start, current - start);
            tokens.Add(new Token(type, lexeme, literal, line));
        }
        private bool AtEnd()
        {
            return current >= source.Length;
        }
    }
}
