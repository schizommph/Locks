using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Locks
{
    class Interpreter : Visitor<object>
    {
        public Env globals { get; private set; }
        public Dictionary<Expr, int> locals { get; private set; }

        private Env env;

        public Interpreter()
        {
            globals = new Env();

            /*globals.Define(new Token(TokenType.IDENTIFIER, "clock", null, -1),
                new ClockCallable());*/

            env = globals;
            locals = new Dictionary<Expr, int>();
        }

        public void Interpret(List<Stmt> statements)
        {
            ExecuteBlock(statements, env);
        }
        public void Resolve(Expr expr, int depth)
        {
            locals[expr] = depth;
        }
        private string Stringify(object obj)
        {
            if(obj == null)
            {
                return "nil";
            }
            else if(obj is double)
            {
                string str = obj.ToString();
                if (str.EndsWith(".0"))
                    str = str.Substring(0, str.Length - 2);
                return str;
            }
            else if (obj is bool)
            {
                return obj.ToString().ToLower();
            }
            else if (obj is Instance instance)
            {
                FunctionCallable tostr = instance.cls.FindMethod("tostr");
                if(tostr != null)
                {
                    object str = tostr.Bind(instance).Call(this, new List<object>());
                    if (str == null)
                        return "nil";
                    return $"{str}";
                }
                
            }
            return obj.ToString();
        }
        public object VisitBinary(Binary binary)
        {
            object left = Evaluate(binary.left);
            object right = Evaluate(binary.right);

            switch (binary.op.type)
            {
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.LESS:
                    CheckNumberOperands(binary.op, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(binary.op, left, right);
                    return (double)left <= (double)right;
                case TokenType.GREATER:
                    CheckNumberOperands(binary.op, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(binary.op, left, right);
                    return (double)left >= (double)right;
                case TokenType.MINUS:
                    CheckNumberOperands(binary.op, left, right);
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    CheckNumberOperands(binary.op, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(binary.op, left, right);
                    return (double)left * (double)right;
                case TokenType.PLUS:
                    if(left is double && right is double)
                        return (double)left + (double)right;
                    if (left is string)
                        return (string)left + Stringify(right);

                    throw new RuntimeError(binary.op, "Operands must be number and strings only.");
            }

            return null;
        }

        public object VisitGrouping(Grouping grouping)
        {
            return Evaluate(grouping.expr);
        }

        public object VisitLiteral(Literal literal)
        {
            return literal.value;
        }
        public object VisitVariable(Variable variable)
        {
            return LookUpVariable(variable.name, variable);
        }
        object LookUpVariable(Token name, Expr variable)
        {
            if(!locals.ContainsKey(variable))
            {
                return globals.Get(name);
            }
            else
            {
                int distance = locals[variable];
                return env.GetAt(distance, name.lexeme);
            }
        }

        public object VisitUnary(Unary unary)
        {
            object right = Evaluate(unary.right);

            switch (unary.op.type)
            {
                case TokenType.MINUS:
                    CheckNumberOperand(unary.op, right);
                    return -(double)right;
                case TokenType.BANG:
                    return !IsTruthish(right);
            }

            return null;
        }
        public object VisitAssign(Assign assign)
        {
            object value = Evaluate(assign.expr);

            if(!locals.ContainsKey(assign))
            {
                globals.Assign(assign.name, value);
            }
            else
            {
                int distance = locals[assign];
                env.AssignAt(distance, assign.name, value);
            }

            return value;
        }
        public object VisitCall(Call call)
        {
            object callee = Evaluate(call.callee);

            List<object> args = new List<object>();
            foreach(Expr expr in call.args)
            {
                args.Add(Evaluate(expr));
            }
            if (!(callee is Callable))
            {
                throw new RuntimeError(call.paren, "Can only call functions.");
            }
            Callable function = (Callable)callee;
            if(args.Count != function.Arity())
            {
                throw new RuntimeError(call.paren,
                    $"Expected {function.Arity()} arguments, but got " +
                    $"{args.Count} instead.");
            }
            try
            {
                return function.Call(this, args);
            }
            catch(Return ex)
            {
                return ex.value;
            }
        }
        public void VisitIfStmt(IfStmt ifStmt)
        {
            if (IsTruthish(Evaluate(ifStmt.condition)))
                Execute(ifStmt.thenBranch);
            else if(ifStmt.elseBranch != null)
                Execute(ifStmt.elseBranch);
        }
        public void VisitReturnStmt(ReturnStmt returnStmt)
        {
            object value = null;
            if (returnStmt.value != null)
                value = Evaluate(returnStmt.value);
            throw new Return(value);
        }
        public void VisitFunctionStmt(FunctionStmt functionStmt)
        {
            FunctionCallable function = new FunctionCallable(
                functionStmt, this.env, false);
            env.Define(functionStmt.name.lexeme, function);
        }
        public void VisitWhileStmt(WhileStmt whileStmt)
        {
            bool condition = IsTruthish(Evaluate(whileStmt.condition));
            while (condition)
            {
                Execute(whileStmt.body);
                condition = IsTruthish(Evaluate(whileStmt.condition));
            }
        }
        public void VisitBlockStmt(BlockStmt blockStmt)
        {
            ExecuteBlock(blockStmt.statements, new Env(env));
        }
        public void VisitExpressionStmt(ExpressionStmt expressionStmt)
        {
            Evaluate(expressionStmt.expr);
        }
        public void VisitPrintStmt(PrintStmt printStmt)
        {
            object value = Evaluate(printStmt.expr);
            Console.WriteLine(Stringify(value));
        }
        public void VisitVarStmt(VarStmt varStmt)
        {
            object value = null;
            if(varStmt.expr != null)
                value = Evaluate(varStmt.expr);

            env.Define(varStmt.name.lexeme, value);
        }
        private void CheckNumberOperands(Token op, object left, object right)
        {
            if (left is double && right is double) return;
            throw new RuntimeError(op, "Operand must be a number.");
        }
        private void CheckNumberOperand(Token op, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(op, "Operand must be a number.");
        }
        public bool IsEqual(object a, object b)
        {
            if(a == null && a == b)
                return true;
            if(a == null) return false;
            return a.Equals(b);
        }
        public bool IsTruthish(object expr)
        {
            if (expr == null) return false;
            if (expr is bool boolean) return boolean;
            if (expr is double num) return num != 0;
            return true;
        }
        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }
        private void Execute(Stmt statement)
        {
            statement.Accept(this);
        }
        public void ExecuteBlock(List<Stmt> statements, Env env)
        {
            Env previous = this.env;
            try
            {
                this.env = env;
                foreach(Stmt statement in statements)
                {
                    try
                    {
                        Execute(statement);
                    }
                    catch (RuntimeError ex)
                    {
                        Program.RuntimeError(ex);
                    }
                }
            }
            finally
            {
                this.env = previous;
            }
        }

        public void VisitClassStmt(ClassStmt classStmt)
        {
            object superclass = null;
            if(classStmt.superclass != null)
            {
                superclass = Evaluate(classStmt.superclass);
                if(!(superclass is Class))
                {
                    throw new RuntimeError(classStmt.superclass.name, "Superclass must be a class.");
                }
            }

            env.Define(classStmt.name.lexeme, null);

            if(classStmt.superclass != null)
            {
                env = new Env(env);
                env.Define("super", superclass);
            }

            Dictionary<string, FunctionCallable> methods = new Dictionary<string, FunctionCallable>();
            foreach(FunctionStmt functionStmt in classStmt.methods)
            {
                FunctionCallable function = new FunctionCallable(functionStmt, env,
                    functionStmt.name.lexeme == "init");
                methods[functionStmt.name.lexeme] = function;
            }

            Class cls = new Class(classStmt.name.lexeme, (Class)superclass, methods);

            if (classStmt.superclass != null)
            {
                env = env.enclosing;
            }
            env.Assign(classStmt.name, cls);
        }

        public object VisitGet(Get get)
        {
            object obj = Evaluate(get.obj);
            if(obj is Instance instance)
            {
                return instance.Get(get.name);
            }
            throw new RuntimeError(get.name, "Only instances can " +
                "have properties.");
        }

        public object VisitSet(Set set)
        {
            object obj = Evaluate(set.obj);

            if(!(obj is Instance instance))
                throw new RuntimeError(set.name, "Only instances can " +
                    "have fields.");
            object value = Evaluate(set.value);
            instance.Set(set.name, value);
            return value;
        }

        public object VisitThis(This ths)
        {
            return LookUpVariable(ths.keyword, ths);
        }

        public object VisitSuper(Super super)
        {
            int distance = locals[super];
            Class superclass = (Class)env.GetAt(distance, "super");
            Instance obj = (Instance)env.GetAt(distance - 1, "this");
            FunctionCallable method = superclass.FindMethod(super.method.lexeme);

            if(method == null)
            {
                throw new RuntimeError(super.keyword, $"Undefined property '{super.method.lexeme}'.");
            }

            return method.Bind(obj);
        }
    }
}
