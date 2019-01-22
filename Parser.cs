namespace Lox
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static TokenType;
    class Parser
    {
        internal class ParseError: System.Exception { }
        private readonly List<Token> tokens;
        private int current = 0;

        internal Parser(List<Token> tokens)
        {
            this.tokens = tokens.ToList();
            tokens.Add(new Token(EOF, "", null, tokens.Last().Line));
        }

        internal IEnumerable<Stmt> Parse()
        {
            var statements = new List<Stmt>();
            while (!IsAtEnd())
                statements.Add(Declaration());
            return statements;
        }

        private Expr Expression() => Assignment();

        private Stmt Declaration()
        {
            try
            {
                if (Match(CLASS))
                    return ClassDeclaration();
                if (Match(FUN))
                    return Function("function");
                if (Match(VAR))
                    return VarDeclaration();
                return Statement();
            }
            catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt ClassDeclaration()
        {
            var name = Consume(IDENTIFIER, "Expected class name.");
            Expr.Variable superClass = null;
            if (Match(LESS))
            {
                Consume(IDENTIFIER, "Expected superclass name");
                superClass = new Expr.Variable { Name = Prev() };
            }
            Consume(LEFT_BRACE, Expected('{', "class name"));
            var methods = new List<Stmt.Function>();
            var statics = new List<Stmt.Function>();
            while (!Check(RIGHT_BRACE) && !IsAtEnd())
            {
                if (Match(STATIC))
                    statics.Add(Function("method"));
                else
                    methods.Add(Function("method"));
            }
            Consume(RIGHT_BRACE, Expected('}', "class body"));
            return new Stmt.Class { name = name, superClass = superClass, methods = methods, statics = statics };
        }

        private Stmt Statement()
        {
            if (Match(FOR))
                return ForStatement();
            if (Match(IF))
                return IfStatement();
            if (Match(PRINT))
                return PrintStatement();
            if (Match(RETURN))
                return ReturnStatement();
            if (Match(WHILE))
                return WhileStatement();
            if (Match(LEFT_BRACE))
                return new Stmt.Block { statements = Block() };
            return ExpressionStatement();
        }

        private Stmt ForStatement()
        {
            Consume(LEFT_PAREN, "Expected '(' after 'for'.");

            var initializer = Match(SEMICOLON) ? null : 
                (Match(VAR) ? VarDeclaration() : ExpressionStatement());
            var condition = !Check(SEMICOLON) ? Expression() : null;
            Consume(SEMICOLON, Expected(';', "loop condition"));
            var increment = !Check(RIGHT_PAREN) ? Expression() : null;
            Consume(RIGHT_PAREN, Expected(')', "for clauses"));
            var body = Statement();
            if (increment != null)
                body = new Stmt.Block { statements = new List<Stmt>() {body, new Stmt.Expression { expression = increment }} };
            if (condition == null)
                condition = new Expr.Literal { Value = true };
            body = new Stmt.While { condition = condition, body = body };
            if (initializer != null)
                body = new Stmt.Block { statements = new List<Stmt>() {initializer, body} };
            return body;
        }

        private Stmt IfStatement()
        {
            Consume(LEFT_PAREN, "Expected '(' after 'if'.");
            var condition = Expression();
            Consume(RIGHT_PAREN, "Expected ')' after 'if'.");
            
            var thenBranch = Statement();
            Stmt elseBranch = null;
            if (Match(ELSE))
                elseBranch = Statement();
            
            return new Stmt.If { condition = condition, thenBranch = thenBranch, elseBranch = elseBranch };
        }

        private Stmt PrintStatement()
        {
            var value = Expression();
            Consume(SEMICOLON, "Expected ';' after value.");
            return new Stmt.Print { expression = value };
        }

        private Stmt ReturnStatement()
        {
            var keyword = Prev();
            Expr value = !Check(SEMICOLON) ? Expression() : null;
            Consume(SEMICOLON, Expected(';', "return statement"));
            return new Stmt.Return { keyword = keyword, value = value };
        }

        private Stmt VarDeclaration()
        {
            var ident = Consume(IDENTIFIER, "Expected an identifier");
            Expr initializer = null;
            if (Match(EQUAL))
                initializer = Expression();
            Consume(SEMICOLON, "Expected ';' after variable declaration");
            return new Stmt.Var { Name = ident, Initializer = initializer };
        }

        private Stmt WhileStatement()
        {
            Consume(LEFT_PAREN, "Expected '(' after while.");
            var cond = Expression();
            Consume(RIGHT_PAREN, "Expected ')' after while.");
            var body = Statement();
            return new Stmt.While { condition = cond, body = body };
        }

        private Stmt ExpressionStatement()
        {
            var value = Expression();
            Consume(SEMICOLON, "Expected ';' after value.");
            return new Stmt.Expression { expression = value };
        }

        private Stmt.Function Function(string kind)
        {
            var name = Consume(IDENTIFIER, string.Format("Expect " + kind + " name."));
            Consume(LEFT_PAREN, Expected('(', kind + " name"));
            var parameters = new List<Token>();
            if (!Check(RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 8)
                        Error(Peek(), "Can't have more than 8 parameters");
                    parameters.Add(Consume(IDENTIFIER, "Expect parameter name"));
                } while (Match(COMMA));
            }
            Consume(RIGHT_PAREN, Expected(')', "parameters"));
            Consume(LEFT_BRACE, Expected('{', "arguments"));
            var body = Block();
            return new Stmt.Function { name = name, parameters = parameters, body = body };
        }

        private List<Stmt> Block()
        {
            var statements = new List<Stmt>();
            while (!Check(RIGHT_BRACE) && !IsAtEnd())
                statements.Add(Declaration());
            Consume(RIGHT_BRACE, Expected('}', "block"));
            return statements;
        }

        private Expr Assignment()
        {
            var expr = NullCoalescing();
            if (Match(EQUAL))
            {
                var equals = Prev();
                var value = Assignment();
                if (expr is Expr.Variable)
                {
                    var name = ((Expr.Variable) expr).Name;
                    return new Expr.Assign { Name = name, Value = value};
                }
                else if (expr is Expr.Get)
                {
                    var get = (Expr.Get) expr;
                    return new Expr.Set { Object = get.Object, Name = get.Name, Value = value };
                }
                Program.Error(equals, "Invalid assignment target.");
            }
            return expr;
        }

        private Expr NullCoalescing()
        {
            var expr = Ternary();
            if (Match(NULL_COALESCING))
            {
                var right = Expression();
                expr = new Expr.Ternary 
                {
                    Condition = new Expr.Binary 
                    { 
                        Left = expr, 
                        Op = new Token(BANG_EQUAL, "!=", null, Prev().Line),  
                        Right = new Expr.Literal { Value = null }
                    },
                    Left = expr,
                    Right = right
                };
            }
            return expr;
        }

        private Expr Ternary()
        {
            var expr = Or();
            if (Match(QUESTION_MARK))
            {
                var left = Expression();
                Consume(COLON, Expected(':', "Left expression"));
                var right = Expression();
                return new Expr.Ternary { Condition = expr, Left = left, Right = right };
            }   
            return expr;
        }

        private Expr Or()
        {
            var expr = And();
            while (Match(OR))
            {
                var op = Prev();
                var right = And();
                expr = new Expr.Logical { Left = expr, Op = op, Right = right };
            }
            return expr;
        }

        private Expr And()
        {
            var expr = Equality();
            while (Match(AND))
            {
                var op = Prev();
                var right = Equality();
                expr = new Expr.Logical { Left = expr, Op = op, Right = right };
            }
            return expr;
        }

        private Expr Equality() 
        {
            var expr = Comparasion();
            while (Match(BANG_EQUAL, EQUAL_EQUAL))
            {
                var op = Prev();
                var right = Comparasion();
                expr = new Expr.Binary 
                {
                    Left = expr,
                    Op = op,
                    Right = right
                };
            }
            return expr;
        }

        private Expr Comparasion()
        {
            var expr = Addition();
            while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
            {
                var op = Prev();
                var right = Addition();
                expr = new Expr.Binary 
                {
                    Left = expr,
                    Op = op,
                    Right = right
                };
            }
            return expr;
        }

        private Expr Addition()
        {
            var expr = Multiplication();
            while (Match(MINUS, PLUS))
            {
                var op = Prev();
                var right = Multiplication();
                expr = new Expr.Binary 
                {
                    Left = expr,
                    Op = op,
                    Right = right
                };
            }
            return expr;
        }

        private Expr Multiplication()
        {
            var expr = Unary();
            while (Match(SLASH, STAR))
            {
                var op = Prev();
                var right = Unary();
                expr = new Expr.Binary 
                {
                    Left = expr,
                    Op = op,
                    Right = right
                };
            }
            return expr;
        }

        private Expr Unary()
        {
            if (Match(BANG, MINUS))
            {
                var op = Prev();
                var right = Unary();
                return new Expr.Unary
                {
                    Op = op,
                    Right = right
                };
            }
            return Call();
        }

        private Expr Call()
        {
            var expr = Primary();
            while (true)
            {
                if (Match(LEFT_PAREN))
                    expr = FinishCall(expr);
                else if (Match(DOT))
                {
                    var name = Consume(IDENTIFIER, "Expected property name after '.'");
                    expr = new Expr.Get { Object = expr, Name = name };
                }
                else
                    break;
            }   
            return expr;
        }

        private Expr FinishCall(Expr calee)
        {
            var arguments = new List<Expr>();
            if (!Check(RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count >= 8)
                        Error(Peek(), $"Cannot have more than {8} arguments.");
                    arguments.Add(Expression());
                } while (Match(COMMA));
            }
            var paren = Consume(RIGHT_PAREN, Expected(')', "arguments"));
            return new Expr.Call { Calee = calee, Paren = paren, Arguments = arguments };
        }

        private Expr Primary()
        {
            if (Match(FALSE)) return new Expr.Literal { Value = false };
            if (Match(TRUE)) return new Expr.Literal { Value = true };
            if (Match(NIL)) return new Expr.Literal { Value = null };

            if (Match(NUMBER, STRING))
                return new Expr.Literal { Value = Prev().Literal };
            if (Match(SUPER))
            {
                var keyword = Prev();
                Consume(DOT, "Expect '.' after 'super'.");
                var method = Consume(IDENTIFIER, "Expect superclass method name.");
                return new Expr.Super { Keyword = keyword, Method = method };
            }
            if (Match(THIS))
                return new Expr.This { Keyword = Prev() };
            if (Match(IDENTIFIER))
                return new Expr.Variable { Name = Prev() };
            if (Match(LEFT_PAREN))
            {
                var expr = Expression();
                Consume(RIGHT_PAREN, "Expected )");
                return new Expr.Grouping { Expression = expr };
            }

            throw Error(Peek(), "Expected expression");
        }

        

        private bool Match(params TokenType[] types)
        {
            var matched = types.Any(t => Check(t));
            if (matched)
                Advance();
            return matched;
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type))
                return Advance();
            throw Error(Peek(), message);   
        }

        private bool Check(TokenType type) => IsAtEnd() ? false : (Peek().Type == type);

        private Token Advance()
        {
            if (!IsAtEnd())
                current += 1;
            return Prev();
        }

        private bool IsAtEnd() => Peek().Type == EOF;

        private Token Peek() => tokens[current];

        private Token Prev() => tokens[current - 1];

        internal static ParseError Error(Token token, string message)
        {
            Program.Error(token, message);
            return new ParseError { };
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd()) 
            {
                if (Prev().Type == SEMICOLON)
                    return;
                
                switch (Peek().Type) 
                {
                    case CLASS:                            
                    case FUN:                              
                    case VAR:                              
                    case FOR:                              
                    case IF:                               
                    case WHILE:                            
                    case PRINT:                            
                    case RETURN:                           
                        return; 
                }

                Advance();
            }
        }

        private string Expected(char expectedChar, string after) => $"Expected '{expectedChar}' after {after}.";

        /* private static void Main(string[] args)
        {
            var tokens = new Token[] 
            {
                new Token(TokenType.NUMBER, "5", 5, 1),
                new Token(TokenType.STAR, "*", null, 1),
                new Token(TokenType.NUMBER, "5", 5, 1),
                new Token(TokenType.EOF, null, null, 1)
            };
            var parser = new Parser(tokens.ToList());
            var expr = parser.Parse();

            Console.WriteLine(new AstPrinter().Print(expr));
        } */
    }
}