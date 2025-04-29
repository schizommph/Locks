namespace Locks
{
    class Resolver : Visitor<object>
    {
        // 190
        private Interpreter interpreter;
        private FunctionType currentFunction = FunctionType.NONE;
        private ClassType currentClass = ClassType.NONE;
        private List<Dictionary<string, bool>> scopes;

        public void Resolve(List<Stmt> statements)
        {
            foreach(Stmt statement in statements)
            {
                Resolve(statement);
            }
        }
        public void Resolve(Stmt statement)
        {
            statement.Accept(this);
        }
        public void Resolve(Expr expr)
        {
            expr.Accept(this);
        }
        void BeginScope()
        {
            scopes.Add(new Dictionary<string, bool>());
        }
        void EndScope()
        {
            scopes.RemoveAt(scopes.Count - 1);
        }

        void Declare(Token name)
        {
            if (scopes.Count == 0) return;

            if(scopes[scopes.Count - 1].ContainsKey(name.lexeme))
            {
                Program.Error(name, 
                    "Already variable with this name is in this scope.");
            }

            scopes[scopes.Count - 1][name.lexeme] = false;
        }
        void Define(Token name)
        {
            if (scopes.Count == 0) return;
            scopes[scopes.Count - 1][name.lexeme] = true;
        }
        void ResolveLocal(Expr expr, Token name)
        {
            for(int i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes[i].ContainsKey(name.lexeme))
                {
                    interpreter.Resolve(expr, scopes.Count - 1 - i);
                    return;
                }
            }
        }
        void ResolveFunction(FunctionStmt functionStmt, FunctionType functionType)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = functionType;
            BeginScope();
            foreach (Token parameter in functionStmt.parameters)
            {
                Declare(parameter);
                Define(parameter);
            }
            Resolve(functionStmt.body);
            EndScope();
            currentFunction = enclosingFunction;
        }

        // 180
        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
            this.scopes = new List<Dictionary<string, bool>>();
        }
        public object VisitAssign(Assign assign)
        {
            Resolve(assign.expr);
            ResolveLocal(assign, assign.name);
            return null;
        }
        public object VisitBinary(Binary binary)
        {
            Resolve(binary.right);
            Resolve(binary.left);
            return null;
        }
        public void VisitBlockStmt(BlockStmt blockStmt)
        {
            BeginScope();
            Resolve(blockStmt.statements);
            EndScope();
        }
        public object VisitCall(Call call)
        {
            Resolve(call.callee);

            foreach(Expr arg in call.args)
            {
                Resolve(arg);
            }
            return null;
        }
        public void VisitExpressionStmt(ExpressionStmt exprStmt)
        {
            Resolve(exprStmt.expr);
        }
        public void VisitFunctionStmt(FunctionStmt functionStmt)
        {
            Declare(functionStmt.name);
            Define(functionStmt.name);

            ResolveFunction(functionStmt, FunctionType.FUNCTION);
        }
        public object VisitGrouping(Grouping grouping)
        {
            Resolve(grouping.expr);
            return null;
        }
        public void VisitIfStmt(IfStmt ifStmt)
        {
            Resolve(ifStmt.condition);
            Resolve(ifStmt.thenBranch);
            if(ifStmt.elseBranch != null)
                Resolve(ifStmt.elseBranch);

        }
        public object VisitLiteral(Literal literal)
        {
            return null;
        }
        public void VisitPrintStmt(PrintStmt printStmt)
        {
            Resolve(printStmt.expr);
        }
        public void VisitReturnStmt(ReturnStmt returnStmt)
        {
            if (currentFunction == FunctionType.NONE)
                Program.Error(returnStmt.keyword, "Can't return function from" +
                    " top-level code.");
            if(returnStmt.value != null)
                if (currentFunction == FunctionType.INITIALIZER)
                    Program.Error(returnStmt.keyword, "Can't return function from" +
                        " initializer.");

            if (returnStmt.value != null)
                Resolve(returnStmt.value);
        }
        public object VisitUnary(Unary unary)
        {
            Resolve(unary.right);
            return null;
        }
        public object VisitVariable(Variable variable)
        {
            if (scopes.Count != 0 &&
                scopes[scopes.Count - 1].TryGetValue(variable.name.lexeme,
                out bool defined) && !defined)
            {
                Program.Error(variable.name, "Can't read local variable" +
                    "in its own initializer.");
            }
            ResolveLocal(variable, variable.name);
            return null;
        }
        public void VisitVarStmt(VarStmt varStmt)
        {
            Declare(varStmt.name);
            if(varStmt.expr != null)
            {
                Resolve(varStmt.expr);
            }
            Define(varStmt.name);
        }
        public void VisitWhileStmt(WhileStmt whileStmt)
        {
            Resolve(whileStmt.condition);
            Resolve(whileStmt.body);
        }

        public void VisitClassStmt(ClassStmt classStmt)
        {
            Declare(classStmt.name);
            Define(classStmt.name);
            ClassType enclosing = currentClass;
            currentClass = ClassType.CLASS;

            if(classStmt.superclass != null &&
                classStmt.superclass.name.lexeme == classStmt.name.lexeme)
            {
                Program.Error(classStmt.superclass.name, "A class cannot inherit " +
                    "from itself.");
            }

            if(classStmt.superclass != null)
            {
                currentClass = ClassType.SUBCLASS;
                Resolve(classStmt.superclass);

                BeginScope();
                scopes[scopes.Count - 1]["super"] = true;
            }

            BeginScope();
            scopes[scopes.Count - 1]["this"] = true;

            foreach(FunctionStmt functionStmt in classStmt.methods)
            {
                FunctionType declaration = FunctionType.METHOD;
                if (functionStmt.name.lexeme == "init")
                    declaration = FunctionType.INITIALIZER;
                ResolveFunction(functionStmt, declaration);
            }

            EndScope();
            if (classStmt.superclass != null)
            {
                EndScope();
            }

            currentClass = enclosing;
        }

        public object VisitGet(Get get)
        {
            Resolve(get.obj);
            return null;
        }

        public object VisitSet(Set set)
        {
            Resolve(set.value);
            Resolve(set.obj);
            return null;
        }

        public object VisitThis(This ths)
        {
            if (currentClass == ClassType.NONE)
                Program.Error(ths.keyword, "Can't use 'this' outside of" +
                    " a class.");

            ResolveLocal(ths, ths.keyword);
            return null;
        }

        public object VisitSuper(Super super)
        {
            if (currentClass == ClassType.NONE)
                Program.Error(super.keyword, "Can't use 'super' outside of a class.");
            if (currentClass != ClassType.SUBCLASS)
                Program.Error(super.keyword, "Can't use 'super' outside of a subclass.");
            ResolveLocal(super, super.keyword);
            return null;
        }
    }
}
