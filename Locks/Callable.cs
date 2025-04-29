using System.Reflection.Metadata;

namespace Locks
{
    interface Callable
    {
        int Arity();
        public object Call(Interpreter interpreter, List<object> arguments);
    }
    class FunctionCallable : Callable
    {
        private Token name;
        private List<Token> parameters;
        private List<Stmt> statements;
        private FunctionStmt declaration;
        private Env closure;
        private bool isInitializer;
        public FunctionCallable(FunctionStmt functionStmt, Env closure, bool isInitializer)
        {
            this.name = functionStmt.name;
            this.parameters = functionStmt.parameters;
            this.statements = functionStmt.body;
            this.declaration = functionStmt;
            this.closure = closure;
            this.isInitializer = isInitializer;
        }
        int Callable.Arity()
        {
            return parameters.Count;
        }
        public object Call(Interpreter interpreter, List<object> arguments)
        {
            Env env = new Env(closure);
            for(int i = 0; i < parameters.Count; i++)
            {
                env.Define(parameters[i].lexeme, arguments[i]);
            }
            try
            {
                interpreter.ExecuteBlock(statements, env);
                if (isInitializer)
                    return closure.GetAt(0, "this");
            }
            catch(Return r)
            {
                if (isInitializer) return closure.GetAt(0, "this");

                return r.value;
            }


            return null;
        }
        public FunctionCallable Bind(Instance instance)
        {
            Env env = new Env(closure);
            env.Define("this", instance);
            return new FunctionCallable(declaration, env, isInitializer);
        }
        public override string ToString()
        {
            return $"<fn {name.lexeme}>";
        }
    }
    class ClockCallable : Callable
    {
        int Callable.Arity()
        {
            return 0;
        }

        object Callable.Call(Interpreter interpreter, List<object> arguments)
        {
            return (double)(DateTime.UnixEpoch.Ticks);
        }
    }
}
