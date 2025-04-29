using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Locks
{
    class Parser
    {
        private List<Token> tokens;
        private int current;
        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
            this.current = 0;
        }
        // 111
        public List<Stmt> Parse()
        {
            List<Stmt> statements = new List<Stmt>();
            try
            {
                while (!AtEnd())
                {
                    statements.Add(Declaration());
                }
            }
            catch(ParseError)
            {
                Synchronize();
                return null;
            }

            return statements;
        }
        private Stmt Declaration()
        {
            if (Match(TokenType.VAR))
                return VarDeclaration();

            return Statement();
        }
        private Stmt Statement()
        {
            if (Match(TokenType.PRINT)) return PrintStatement();
            if (Match(TokenType.LEFT_BRACE)) return BlockStatement();
            if (Match(TokenType.IF)) return IfStatement();
            if (Match(TokenType.WHILE)) return WhileStatement();
            if (Match(TokenType.FOR)) return ForStatement();
            if (Match(TokenType.FUN)) return FunctionStatement("function");
            if (Match(TokenType.CLASS)) return ClassStatement();
            if (Match(TokenType.RETURN)) return ReturnStatement();

            return ExpressionStatement();
        }
        private ClassStmt ClassStatement()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expected class name.");

            Variable superclass = null;
            if (Match(TokenType.LESS))
            {
                Consume(TokenType.IDENTIFIER, "Expected superclass name.");
                superclass = new Variable(Previous());
            }
            
            Consume(TokenType.LEFT_BRACE, "Expected '{' before class body.");
            List<FunctionStmt> methods = new List<FunctionStmt>();
            while(!Check(TokenType.RIGHT_BRACE) && !AtEnd())
            {
                methods.Add(FunctionStatement("method"));
            }
            Consume(TokenType.RIGHT_BRACE, "Expected '}' after class body.");

            return new ClassStmt(name, superclass, methods);
        }
        private FunctionStmt FunctionStatement(string type)
        {
            Token name = Consume(TokenType.IDENTIFIER, $"Expected {type} name.");
            Consume(TokenType.LEFT_PAREN, $"Expected '(' after {type} name.");
            List<Token> parameters = new List<Token>();
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 parameters.");
                    }
                    parameters.Add(Consume(TokenType.IDENTIFIER,
                        "Expected parameter name."));
                } while (Match(TokenType.COMMA));
            }
            Consume(TokenType.RIGHT_PAREN, $"Expected '(' after parameters.");
            Consume(TokenType.LEFT_BRACE, $"Expected '{{' before {type} body.");
            List<Stmt> body = BlockStatement().statements;
            return new FunctionStmt(name, parameters, body);
        }
        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(TokenType.SEMICOLON, "Expected ';' at the end of statement.");
            return new ExpressionStmt(expr);
        }
        private Stmt VarDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expected variable name.");
            Expr initializer = null;
            if (Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expected ';' at the end of statement.");
            return new VarStmt(name, initializer);
        }
        private Stmt ReturnStatement()
        {
            Token keyword = Previous();
            Expr value = null;
            if (!Check(TokenType.SEMICOLON))
            {
                value = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expected ';' after return value.");
            return new ReturnStmt(keyword, value);
        }
        private Stmt PrintStatement()
        {
            Expr expr = Expression();
            Consume(TokenType.SEMICOLON, "Expected ';' at the end of statement.");
            return new PrintStmt(expr);
        }
        private Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expected '(' after declaring if statement.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after declaring if statement.");
            Stmt thenBranch = Statement();
            Stmt elseBranch = null;
            if (Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new IfStmt(condition, thenBranch, elseBranch);
        }
        private Stmt ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expected '(' after declaring for statement.");

            Stmt initializer;
            if (Match(TokenType.SEMICOLON))
                initializer = null;
            else if (Check(TokenType.VAR))
                initializer = Declaration();
            else
                initializer = ExpressionStatement();


            Expr condition = null;
            if (!Check(TokenType.SEMICOLON))
                condition = Expression();
            

            Consume(TokenType.SEMICOLON, "Expected ';' after loop condition");

            Expr increment = null;
            if (!Check(TokenType.RIGHT_PAREN))
                increment = Expression();


            Consume(TokenType.RIGHT_PAREN, "Expected ')' after declaring for statement.");


            Stmt body = Statement();

            if(increment != null)
                body = new BlockStmt(
                    new List<Stmt>()
                    {
                        body,
                        new ExpressionStmt(increment)
                    });
            if (condition == null) condition = new Literal(true);
            body = new WhileStmt(condition, body);

            if(initializer != null)
                body = new BlockStmt(
                    new List<Stmt>()
                    {
                        initializer,
                        body
                    });


            return body;
        }
        private Stmt WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expected '(' after declaring while statement.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after declaring while statement.");
            Stmt thenBranch = Statement();

            return new WhileStmt(condition, thenBranch);
        }
        private BlockStmt BlockStatement()
        {
            Token leftBrace = Previous();
            List<Stmt> statements = new List<Stmt>();
            while(!Check(TokenType.RIGHT_BRACE) && !AtEnd())
            {
                statements.Add(Declaration());
            }
            Consume(TokenType.RIGHT_BRACE, "Expected '}' after block.");
            return new BlockStmt(statements);
        }
        private Expr Expression()
        {
            return Assignment();
        }
        private Expr Assignment()
        {
            Expr expr = EqualityExpr();

            if (Match(TokenType.EQUAL))
            {
                Token equals = Previous();
                Expr right = Expression();
                if(expr is Variable var)
                {
                    return new Assign(var.name, right);
                }
                else if(expr is Get get)
                {
                    return new Set(get.name, get.obj, right);
                }
                Error(equals, "Invalid assignment target.");
            }
            return expr;
        }
        private Expr EqualityExpr()
        {
            Expr expr = ComparisonExpr();
            while(Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token op = Previous();
                Expr right = ComparisonExpr();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }
        private Expr ComparisonExpr()
        {
            Expr expr = TermExpr();
            while (Match(TokenType.LESS, TokenType.LESS_EQUAL,
                TokenType.GREATER, TokenType.GREATER_EQUAL))
            {
                Token op = Previous();
                Expr right = TermExpr();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }
        private Expr TermExpr()
        {
            Expr expr = FactorExpr();
            while (Match(TokenType.PLUS, TokenType.MINUS))
            {
                Token op = Previous();
                Expr right = FactorExpr();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }
        private Expr FactorExpr()
        {
            Expr expr = UnaryExpr();
            while (Match(TokenType.STAR, TokenType.SLASH))
            {
                Token op = Previous();
                Expr right = UnaryExpr();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }
        private Expr UnaryExpr()
        {
            while (Match(TokenType.MINUS, TokenType.BANG))
            {
                Token op = Previous();
                Expr right = UnaryExpr();
                return new Unary(op, right);
            }
            return CallExpr();
        }
        private Expr CallExpr()
        {
            Expr expr = LiteralExpr();

            while (true)
            {
                if (Match(TokenType.LEFT_PAREN))
                {
                    expr = FinishCallExpr(expr);
                }
                else if (Match(TokenType.PERIOD))
                {
                    Token name = Consume(TokenType.IDENTIFIER, "Expected" +
                        "property name after '.'.");
                    expr = new Get(name, expr);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }
        private Expr FinishCallExpr(Expr expr)
        {
            List<Expr> args = new List<Expr>();
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if(args.Count >= 255)
                        Error(Peek(), "Can't have more than 255 arguments.");
                    args.Add(Expression());
                } while (Match(TokenType.COMMA));
            }
            Token paren = Consume(TokenType.RIGHT_PAREN,
                "Expected ')' after arguments.");

            return new Call(expr, paren, args);
        }
        private Expr LiteralExpr()
        {
            if (Match(TokenType.FALSE)) return new Literal(false);
            if (Match(TokenType.TRUE)) return new Literal(true);
            if (Match(TokenType.NIL)) return new Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
                return new Literal(Previous().literal);
            if (Match(TokenType.IDENTIFIER))
                return new Variable(Previous());
            if (Match(TokenType.THIS))
                return new This(Previous());
            if (Match(TokenType.SUPER))
            {
                Token keyword = Previous();
                Consume(TokenType.PERIOD, "Expected '.' after 'super'.");
                Token method = Consume(TokenType.IDENTIFIER, "Expected superclass method name.");
                return new Super(keyword, method);
            }
            if (Match(TokenType.LEFT_PAREN, TokenType.STRING))
            {
                Expr expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Grouping(expr);
            }
            throw Error(Peek(), "Expected expression.");
        }
        private void Synchronize()
        {
            Advance();
            while (!AtEnd())
            {
                if (Previous().type == TokenType.SEMICOLON) return;
                switch(Previous().type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }
                Advance();
            }
        }
        private bool Match(params TokenType[] types)
        {
            foreach(TokenType tokenType in types)
            {
                if (Check(tokenType))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }
        private Token Consume(TokenType tokenType, string message)
        {
            if (Check(tokenType))
            {
                Advance();
                return Previous();
            }
            throw Error(Peek(), message);
        }
        private ParseError Error(Token token, string message)
        {
            Program.Error(token, message);
            return new ParseError();
        }
        private Token Advance()
        {
            if (!AtEnd())
                current++;
            return Previous();
        }
        private Token Previous()
        {
            return tokens[current - 1];
        }
        private bool Check(TokenType tokenType)
        {
            if (AtEnd())
                return false;
            return Peek().type == tokenType;
        }
        private Token Peek()
        {
            return tokens[current];
        }
        private bool AtEnd()
        {
            return Peek().type == TokenType.EOF;
        }
    }
}
