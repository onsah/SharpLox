namespace Lox
{
    internal class Token 
    {
        readonly TokenType type;
        readonly string lexeme;
        readonly object literal;
        readonly int line;

        public Token(TokenType type, string lexeme, object literal, int line) 
        {
            this.type = type;                                             
            this.lexeme = lexeme;                                         
            this.literal = literal;                                       
            this.line = line;                                             
        }

        internal string Lexeme => lexeme;

        internal object Literal => literal;

        internal int Line => line;

        internal TokenType Type => type;

        public override string ToString() => 
            string.Format("Type: {0}, lexeme: {1}, literal: {2}", Type, Lexeme, Literal);
    }

    internal enum TokenType 
    {
        // Single-character tokens.                      
        LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
        COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR, COLON,

        // One or two character tokens.                  
        BANG, BANG_EQUAL,                                
        EQUAL, EQUAL_EQUAL,                              
        GREATER, GREATER_EQUAL,                          
        LESS, LESS_EQUAL,
        QUESTION_MARK, NULL_COALESCING,                                

        // Literals.                                     
        IDENTIFIER, STRING, NUMBER,                      

        // Keywords.                                     
        AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR,  
        PRINT, RETURN, SUPER, THIS, TRUE, VAR, WHILE, STATIC,

        EOF 
    }
}