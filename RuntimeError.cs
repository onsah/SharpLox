namespace Lox
{
    internal class RuntimeError: System.Exception
    {
        internal readonly Token token;

        internal RuntimeError(Token token, string message): base(message)
        {
            this.token = token;
        }
    }
}