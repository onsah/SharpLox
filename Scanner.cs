namespace Lox 
{
    using System.Collections.Generic;
    using static TokenType;
    
    public class Scanner
    {
        private readonly string source;
        private readonly List<Token> tokens = new List<Token>();
        private int start = 0;
        private int current = 0;
        private int line = 1;

        private static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
        {
            {"and", AND},
            {"class", CLASS},
            {"else", ELSE},
            {"false", FALSE},
            {"for", FOR},
            {"fun", FUN},
            {"if", IF},
            {"nil", NIL},
            {"or", OR},
            {"print", PRINT},
            {"return", RETURN},
            {"super", SUPER},
            {"this", THIS},
            {"true", TRUE},
            {"var", VAR},
            {"while", WHILE},
            {"static", STATIC}
        };

        public Scanner(string source)
        {
            this.source = source;
        }

        internal IEnumerable<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                start = current;
                ScanToken();
            }
            tokens.Add(new Token(EOF, "", null, line));
            return tokens;
        }

        private bool IsAtEnd(int offset = 0) => (current + offset) >= source.Length;

        private void ScanToken()
        {
            var c = Advance();
            switch (c)
            {
                case '(': AddToken(LEFT_PAREN); break;
                case ')': AddToken(RIGHT_PAREN); break;
                case '{': AddToken(LEFT_BRACE); break;     
                case '}': AddToken(RIGHT_BRACE); break;    
                case ',': AddToken(COMMA); break;          
                case '.': AddToken(DOT); break;            
                case '-': AddToken(MINUS); break;          
                case '+': AddToken(PLUS); break;           
                case ';': AddToken(SEMICOLON); break;      
                case '*': AddToken(STAR); break;
                case ':': AddToken(COLON); break;
                case '!': AddToken(Match('=') ? BANG_EQUAL : BANG); break;
                case '=': AddToken(Match('=') ? EQUAL_EQUAL : EQUAL); break;
                case '<': AddToken(Match('=') ? LESS_EQUAL : LESS); break;
                case '>': AddToken(Match('=') ? GREATER_EQUAL : GREATER); break;
                // Single question mark currently is not used
                case '?': AddToken(Match('?') ? NULL_COALESCING : QUESTION_MARK); break;
                case '/':
                    if (Match('/'))
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    else
                        AddToken(SLASH);
                    break;
                case '"':
                    StringLiteral();
                    break;
                case ' ':                                    
                case '\r':                                   
                case '\t':                                     
                    break;
                case '\n':                                   
                    line += 1;                                    
                    break;  
                default:
                    if (IsDigit(c))
                        Number();
                    else if (IsAlpha(c))
                        Identifier();
                    else 
                        Lox.Program.Error(line, "Unexpected character: " + c);
                    break;
            }
        }

        private void Identifier()
        {
            while (IsAlpha(Peek()))
                Advance();
            var text = source.Substring(start, current - start);
            TokenType? type = null;
            if (keywords.ContainsKey(text))
                type = keywords[text];
            AddToken(type ?? IDENTIFIER);
        }

        private bool IsAlpha(char c) => 
            (c >= 'a' && c <= 'z') ||      
            (c >= 'A' && c <= 'Z') ||      
            c == '_'; 

        private bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

        private char Advance() => source[current++];

        private void AddToken(TokenType type, object literal = null)
        {
            var text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }

        private bool Match(char literal)
        {
            if (IsAtEnd())
                return false;
            var matched = source[current] == literal;
            if (matched)
                current += 1;
            return matched;
        }

        private char Peek(int offset = 0) => IsAtEnd(offset) ? '\0' : source[current + offset];

        private void StringLiteral()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n')
                    line += 1;
                Advance();
            }
            if (IsAtEnd())
            {
                Program.Error(line, "Unterminated string");
            }
            Advance();
            var value = source.Substring(start + 1, current - start - 2);
            AddToken(STRING, value);
        }

        private bool IsDigit(char c) => c >= '0' && c <= '9'; 

        private void Number() 
        {
            while (IsDigit(Peek()))
                Advance();

            if (Peek() == '.' && IsDigit(Peek(1))) 
            {
                Advance();
                while (IsDigit(Peek()))
                    Advance();
            }
            AddToken(NUMBER, double.Parse(source.Substring(start, current - start)));
        }
    }
}