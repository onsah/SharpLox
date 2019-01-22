namespace Lox
{
    public class Return: System.Exception
    {
        internal readonly object value;

        public Return(object value): base()
        {
            this.value = value;
        }
    }
}