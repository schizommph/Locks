using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locks
{
    abstract class Expr
    {
        public abstract T Accept<T>(Visitor<T> visitor);
    }
    class Binary : Expr
    {
        public Expr left { get; private set; }
        public Token op { get; private set; }
        public Expr right { get; private set; }
        public Binary(Expr left, Token op, Expr right)
        {
            this.left = left;
            this.op = op;
            this.right = right;
        }
        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitBinary(this);
        }
    }
    class Grouping : Expr
    {
        public Expr expr { get; private set; }
        public Grouping(Expr expr)
        {
            this.expr = expr;
        }
        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitGrouping(this);
        }
    }
    class Literal : Expr
    {
        public object value { get; private set; }
        public Literal(object value)
        {
            this.value = value;
        }
        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitLiteral(this);
        }
    }
    class Unary : Expr
    {
        public Token op { get; private set; }
        public Expr right { get; private set; }
        public Unary(Token op, Expr right)
        {
            this.op = op;
            this.right = right;
        }
        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitUnary(this);
        }
    }
    class Variable : Expr
    {
        public Token name { get; private set; }
        public Variable(Token name)
        {
            this.name = name;
        }
        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitVariable(this);
        }
    }
    class Assign : Expr
    {
        public Token name { get; private set; }
        public Expr expr { get; private set; }
        public Assign(Token name, Expr expr)
        {
            this.name = name;
            this.expr = expr;
        }
        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitAssign(this);
        }
    }
    class Call : Expr
    {
        public Expr callee { get; private set; }
        public Token paren { get; private set; }
        public List<Expr> args { get; private set; }
        public Call(Expr callee, Token paren, List<Expr> args)
        {
            this.callee = callee;
            this.paren = paren;
            this.args = args;
        }
        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitCall(this);
        }
    }
    class Get : Expr
    {
        public Token name { get; private set; }
        public Expr obj { get; private set; }
        public Get(Token name, Expr obj)
        {
            this.name = name;
            this.obj = obj;
        }
        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitGet(this);
        }
    }
    class Set : Expr
    {
        public Token name { get; private set; }
        public Expr obj { get; private set; }
        public Expr value { get; private set; }
        public Set(Token name, Expr obj, Expr value)
        {
            this.name = name;
            this.obj = obj;
            this.value = value;
        }
        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitSet(this);
        }
    }
    class This : Expr
    {
        public Token keyword { get; private set; }
        public This(Token keyword)
        {
            this.keyword = keyword;
        }
        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitThis(this);
        }
    }
    class Super : Expr
    {
        public Token keyword { get; private set; }
        public Token method { get; private set; }
        public Super(Token keyword, Token method)
        {
            this.keyword = keyword;
            this.method = method;
        }
        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitSuper(this);
        }
    }
}
