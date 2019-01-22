namespace Lox
{
    using System;
    using System.Text;

    class AstPrinter : Expr.Visitor<string>
    {
        internal string Print(Expr expr) => expr.Accept(this);

        public string VisitBinaryExpr(Expr.Binary expr) => Pharanthesize(expr.Op.Lexeme, expr.Left, expr.Right);
        public string VisitCallExpr(Expr.Call expr) => throw new NotImplementedException();
        public string VisitGroupingExpr(Expr.Grouping expr) => Pharanthesize("group", expr.Expression);

        public string VisitLiteralExpr(Expr.Literal expr) => (expr.Value ?? "nil").ToString();

        public string VisitLogicalExpr(Expr.Logical expr) => throw new NotImplementedException();

        public string VisitUnaryExpr(Expr.Unary expr) => Pharanthesize(expr.Op.Lexeme, expr.Right);
        // TODO: format
        public string VisitVariableExpr(Expr.Variable expr) => Pharanthesize("var", expr);
        
        public string VisitAssignExpr(Expr.Assign expr)
        {
            throw new NotImplementedException();
        }

        public string VisitTernaryExpr(Expr.Ternary expr)
        {
            throw new NotImplementedException();
        }

        private string Pharanthesize(string name, params Expr[] exprs) 
        {
            var builder = new StringBuilder();
            builder.Append(string.Format("({0}", name));
            foreach (var expr in exprs) 
                builder.Append(string.Format(" {0}", expr.Accept(this)));
            builder.Append(')');
            return builder.ToString();
        }

        public string VisitGetExpr(Expr.Get expr)
        {
            throw new NotImplementedException();
        }

        public string VisitSetExpr(Expr.Set expr)
        {
            throw new NotImplementedException();
        }

        public string VisitThisExpr(Expr.This expr)
        {
            throw new NotImplementedException();
        }

        public string VisitSuperExpr(Expr.Super expr)
        {
            throw new NotImplementedException();
        }

        /* static void Main(string[] args)
        {
            var expression = new Expr.Binary
            {
                Op = new Token(TokenType.STAR, "*", null, 1),
                Left = new Expr.Unary 
                { 
                    Op = new Token(TokenType.MINUS, "-", null, 1), Right = new Expr.Literal { Value = 123 }
                },
                Right = new Expr.Grouping 
                {
                    Expression = new Expr.Literal { Value = 45.67 }
                }
            };
            
            Console.WriteLine(new AstPrinter().Print(expression));
        } */
    }
}