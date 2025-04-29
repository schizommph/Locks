using System.Net.Http.Headers;

namespace Locks
{
    class Program
    {
        private static Interpreter interpreter = new Interpreter();
        private static bool hadError = false;
        private static bool hadRuntimeError = false;
        static void Main(string[] args)
        {
            if(args.Length > 1)
            {
                Console.WriteLine($"Usage: Locks <script>");
                Environment.Exit(0);
            }
            else if(args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }
        private static void RunFile(string path)
        {
            string source = "";
            try
            {
                source = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with reading \"{path}\": {ex.Message}");
            }
            Run(source);
            if (hadError)
                Environment.Exit(65);
            if (hadRuntimeError)
                Environment.Exit(70);
        }
        private static void RunPrompt()
        {
            string line;
            while(true)
            {
                Console.Write("$ ");
                line = Console.ReadLine();
                if(line == null)
                    break;
                if(line.Trim() != "")
                    Run(line);
                hadError = false;
                hadRuntimeError = false;
            }
        }
        // 107
        public static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();
            Parser parser = new Parser(tokens);
            List<Stmt> statements = parser.Parse();
            if (hadError)
                return;
            if (hadRuntimeError)
                return;

            Resolver resolver = new Resolver(interpreter);
            resolver.Resolve(statements);

            if (hadError)
                return;

            interpreter.Interpret(statements);
        }
        public static void Error(Token token, string message)
        {
            if(token.type == TokenType.EOF)
            {
                Report(token.line, " at end", message);
            }
            else
            {
                Report(token.line, $" at '{token.lexeme}'", message);
            }
        }
        public static void RuntimeError(RuntimeError ex)
        {
            Console.WriteLine($"At line {ex.token.line}.\n\tRuntime Error: {ex.Message}");
            hadRuntimeError = true;
        }
        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }
        public static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"At line {line}.\n\tError{where}: {message}");
            hadError = true;
        }
        public static void Warning(int line, string message)
        {
            Warning(line, "", message);
        }
        public static void Warning(int line, string where, string message)
        {
            Console.Error.WriteLine($"At line {line}.\nWarning{where}: {message}");
        }
    }
}