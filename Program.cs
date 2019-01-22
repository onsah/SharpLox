namespace Lox
{

    using System;
    using System.IO;
    using System.Linq;
    class Program
    {
        static Interpreter interpreter = new Interpreter();
        static bool HadError { get; set; } = false;
        static bool HadRuntimeError { get; set; } = false;
        
        static void Main(string[] args)
        {
            if (args.Length > 1) 
            {
                Console.WriteLine("Usage: Lox [script]");
                System.Environment.Exit(64);
            } 
            else if (args.Length == 1) 
                RunFile(args[0]);
            else
                RunPrompt();
        }

        private static void RunFile(string path)
        {
            var bytes = File.ReadAllBytes(path);
            Run(System.Text.Encoding.Default.GetString(bytes));
            if (HadError) 
                System.Environment.Exit(65); 
            if (HadRuntimeError)
                System.Environment.Exit(70);
        }

        private static void RunPrompt()
        {
            while (true) 
            {
                Console.Write("> ");
                Run(Console.ReadLine());
                HadError = false;
            }
        }

        private static void Run(string source)
        {
            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();
            var parser = new Parser(tokens.ToList());
            var statements = parser.Parse();
            if (HadError)
                return;
            var resolver = new Resolver(interpreter);
            resolver.Resolve(statements);
            if (HadError) // Check after resolution
                return;
            interpreter.Interpret(statements);
            //Console.WriteLine(new AstPrinter().Print(expr));
        }

        internal static void Error(int line, string message) => 
            Report(line, "", message);

        internal static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
                Report(token.Line, " at end", message);
            else 
                Report(token.Line, String.Format(" At'{0}' ", token.Lexeme), message);
        }

        internal static void RuntimeError(RuntimeError error)
        {
            //Console.WriteLine(error.ToString());
            Console.WriteLine(String.Format("{0}\n[line {1}]", error.Message, error.token.Line));
            HadRuntimeError = true;
        }

        private static void Report(int line, string where, string message)
        {
            HadError = true;
            Console.Error.WriteLine("[line " + line + "] Error" + where + ": " + message);
        }
    }
}
