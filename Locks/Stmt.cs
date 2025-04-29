using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locks
{
    abstract class Stmt
    {
        public abstract void Accept<T>(Visitor<T> visitor);
    }
    class BlockStmt : Stmt
    {
        public List<Stmt> statements { get; private set; }
        public BlockStmt(List<Stmt> statements)
        {
            this.statements = statements;
        }
        public override void Accept<T>(Visitor<T> visitor)
        {
            visitor.VisitBlockStmt(this);
        }
    }
    class IfStmt : Stmt
    {
        public Expr condition { get; private set; }
        public Stmt thenBranch { get; private set; }
        public Stmt elseBranch { get; private set; }
        public IfStmt(Expr condition, Stmt thenBranch, Stmt elseBranch)
        {
            this.condition = condition;
            this.thenBranch = thenBranch;
            this.elseBranch = elseBranch;
        }
        public override void Accept<T>(Visitor<T> visitor)
        {
            visitor.VisitIfStmt(this);
        }
    }
    class WhileStmt : Stmt
    {
        public Expr condition { get; private set; }
        public Stmt body { get; private set; }
        public WhileStmt(Expr condition, Stmt body)
        {
            this.condition = condition;
            this.body = body;
        }
        public override void Accept<T>(Visitor<T> visitor)
        {
            visitor.VisitWhileStmt(this);
        }
    }
    class FunctionStmt : Stmt
    {
        public Token name { get; private set; }
        public List<Token> parameters { get; private set; }
        public List<Stmt> body;
        public FunctionStmt(Token name, List<Token> parameters, List<Stmt> body)
        {
            this.name = name;
            this.parameters = parameters;
            this.body = body;
        }
        public override void Accept<T>(Visitor<T> visitor)
        {
            visitor.VisitFunctionStmt(this);
        }
    }
    class ExpressionStmt : Stmt
    {
        public Expr expr { get; private set; }
        public ExpressionStmt(Expr expr)
        {
            this.expr = expr;
        }
        public override void Accept<T>(Visitor<T> visitor)
        {
            visitor.VisitExpressionStmt(this);
        }
    }
    class PrintStmt : Stmt
    {
        public Expr expr { get; private set; }
        public PrintStmt(Expr expr)
        {
            this.expr = expr;
        }
        public override void Accept<T>(Visitor<T> visitor)
        {
            visitor.VisitPrintStmt(this);
        }
    }
    class ReturnStmt : Stmt
    {
        public Token keyword { get; private set; }
        public Expr value { get; private set; }
        public ReturnStmt(Token keyword, Expr value)
        {
            this.value = value;
            this.keyword = keyword;
        }
        public override void Accept<T>(Visitor<T> visitor)
        {
            visitor.VisitReturnStmt(this);
        }
    }
    class VarStmt : Stmt
    {
        public Token name { get; private set; }
        public Expr expr { get; private set; }
        public VarStmt(Token name, Expr expr)
        {
            this.name = name;
            this.expr = expr;
        }
        public override void Accept<T>(Visitor<T> visitor)
        {
            visitor.VisitVarStmt(this);
        }
    }
    class ClassStmt : Stmt
    {
        public Token name { get; private set; }
        public Variable superclass;
        public List<FunctionStmt> methods { get; private set; }
        public ClassStmt(Token name, Variable superclass, List<FunctionStmt> methods)
        {
            this.name = name;
            this.superclass = superclass;
            this.methods = methods;
        }

        public override void Accept<T>(Visitor<T> visitor)
        {
            visitor.VisitClassStmt(this);
        }
    }
}
