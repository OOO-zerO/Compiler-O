public interface ILexer
{
    Token GetNextToken();
}

public class Lexer : ILexer
{
    private readonly string _input;
    private int _position;
    private char _currentChar;
    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        {"class", TokenType.CLASS},
        {"if", TokenType.IF},
        {"else", TokenType.ELSE},
        {"while", TokenType.WHILE},
        {"return", TokenType.RETURN},
        {"void", TokenType.VOID},
        {"int", TokenType.INT},
        {"float", TokenType.FLOAT},
        {"read", TokenType.READ},
        {"write", TokenType.WRITE},
        {"true", TokenType.BOOL_LITERAL},
        {"false", TokenType.BOOL_LITERAL},
        {"null", TokenType.NULL_LITERAL}
    };

    public Lexer(string input)
    {
        _input = input;
        _position = 0;
        _currentChar = _input.Length > 0 ? _input[0] : '\0';
    }

    private void NextChar()
    {
        _position++;
        if (_position >= _input.Length)
            _currentChar = '\0'; // End of input
        else
            _currentChar = _input[_position];
    }

    private void SkipWhitespace()
    {
        while (_currentChar != '\0' && char.IsWhiteSpace(_currentChar))
            NextChar();
    }

    private Token ParseIdentifierOrKeyword()
    {
        int start = _position;
        while (_currentChar != '\0' && (char.IsLetterOrDigit(_currentChar) || _currentChar == '_'))
        {
            NextChar();
        }

        string result = _input.Substring(start, _position - start);

        if (Keywords.TryGetValue(result, out TokenType keywordType))
        {
            // For boolean literals, we need to preserve the actual value
            if (keywordType == TokenType.BOOL_LITERAL)
                return new Token(keywordType, result);
            return new Token(keywordType, result);
        }
        return new Token(TokenType.IDENTIFIER, result);
    }

    private Token ParseNumber()
    {
        int start = _position;
        bool hasDot = false;
        bool isFloat = false;

        while (_currentChar != '\0' && (char.IsDigit(_currentChar) || _currentChar == '.'))
        {
            if (_currentChar == '.')
            {
                if (hasDot)
                {
                    // Second dot encountered - return error
                    NextChar();
                    return new Token(TokenType.ERROR, "Multiple decimal points in number");
                }
                hasDot = true;
                isFloat = true;
            }
            NextChar();
        }

        string numberValue = _input.Substring(start, _position - start);

        if (isFloat)
            return new Token(TokenType.FLOAT_LITERAL, numberValue);
        else
            return new Token(TokenType.INT_LITERAL, numberValue);
    }

    public Token GetNextToken()
    {
        while (_currentChar != '\0')
        {
            if (char.IsWhiteSpace(_currentChar))
            {
                SkipWhitespace();
                continue;
            }

            if (char.IsLetter(_currentChar) || _currentChar == '_')
            {
                return ParseIdentifierOrKeyword();
            }

            if (char.IsDigit(_currentChar))
            {
                return ParseNumber();
            }

            switch (_currentChar)
            {
                case '=':
                    NextChar();
                    if (_currentChar == '=')
                    {
                        NextChar();
                        return new Token(TokenType.EQUALS, "==");
                    }
                    return new Token(TokenType.ASSIGN, "=");

                case '!':
                    NextChar();
                    if (_currentChar == '=')
                    {
                        NextChar();
                        return new Token(TokenType.NOT_EQUALS, "!=");
                    }
                    return new Token(TokenType.ERROR, "Unexpected character: !");

                case '>':
                    NextChar();
                    return new Token(TokenType.GREATER_THAN, ">");

                case '<':
                    NextChar();
                    return new Token(TokenType.LESS_THAN, "<");

                case '+':
                    NextChar();
                    return new Token(TokenType.PLUS, "+");

                case '-':
                    NextChar();
                    return new Token(TokenType.MINUS, "-");

                case '*':
                    NextChar();
                    return new Token(TokenType.MULTIPLY, "*");

                case '/':
                    NextChar();
                    // Handle single-line comments
                    if (_currentChar == '/')
                    {
                        while (_currentChar != '\0' && _currentChar != '\n' && _currentChar != '\r')
                            NextChar();
                        continue; // Continue to get next token after comment
                    }
                    return new Token(TokenType.DIVIDE, "/");

                case '(':
                    NextChar();
                    return new Token(TokenType.LEFT_PAREN, "(");

                case ')':
                    NextChar();
                    return new Token(TokenType.RIGHT_PAREN, ")");

                case '{':
                    NextChar();
                    return new Token(TokenType.LEFT_BRACE, "{");

                case '}':
                    NextChar();
                    return new Token(TokenType.RIGHT_BRACE, "}");

                case ';':
                    NextChar();
                    return new Token(TokenType.SEMICOLON, ";");

                case ',':
                    NextChar();
                    return new Token(TokenType.COMMA, ",");

                case '"':
                    return ParseStringLiteral();

                default:
                    var errChar = _currentChar;
                    NextChar();
                    return new Token(TokenType.ERROR, $"Unexpected character: {errChar}");
            }
        }
        return new Token(TokenType.EOF, string.Empty);
    }

    private Token ParseStringLiteral()
    {
        NextChar(); // Skip opening quote
        int start = _position;

        while (_currentChar != '\0' && _currentChar != '"')
        {
            NextChar();
        }

        if (_currentChar == '"')
        {
            string strValue = _input.Substring(start, _position - start);
            NextChar(); // Skip closing quote
            return new Token(TokenType.STRING_LITERAL, strValue);
        }

        return new Token(TokenType.ERROR, "Unterminated string literal");
    }
}