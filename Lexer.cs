public interface ILexer
{
    Token GetNextToken();
    int CurrentLine { get; }
    int CurrentColumn { get; }
}

public class Lexer : ILexer
{
    private readonly string _input;
    private int _position;
    private int _line;
    private int _column;
    private char _currentChar;

    public int CurrentLine => _line;
    public int CurrentColumn => _column;

    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        // Class declaration keywords
        {"class", TokenType.CLASS},
        {"extends", TokenType.EXTENDS},
        {"is", TokenType.IS},
        {"end", TokenType.END},
        
        // Member declaration keywords
        {"var", TokenType.VAR},
        {"method", TokenType.METHOD},
        {"this", TokenType.THIS},
        
        // Statement keywords
        {"if", TokenType.IF},
        {"then", TokenType.THEN},
        {"else", TokenType.ELSE},
        {"while", TokenType.WHILE},
        {"loop", TokenType.LOOP},
        {"return", TokenType.RETURN},
        
        // Predefined class names
        {"Integer", TokenType.INTEGER},
        {"Real", TokenType.REAL},
        {"Boolean", TokenType.BOOLEAN},
        {"Array", TokenType.ARRAY},
        {"List", TokenType.LIST},
        {"AnyValue", TokenType.ANYVALUE},
        {"AnyRef", TokenType.ANYREF},
        
        // Boolean literals
        {"true", TokenType.BOOL_LITERAL},
        {"false", TokenType.BOOL_LITERAL}
    };

    public Lexer(string input)
    {
        _input = input;
        _position = 0;
        _line = 1;
        _column = 1;
        _currentChar = _input.Length > 0 ? _input[0] : '\0';
    }

    private void NextChar()
    {
        if (_currentChar == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }

        _position++;
        if (_position >= _input.Length)
            _currentChar = '\0'; // End of input
        else
            _currentChar = _input[_position];
    }

    private void SkipWhitespace()
    {
        while (_currentChar != '\0' && char.IsWhiteSpace(_currentChar))
        {
            NextChar();
        }
    }

    private void SkipComment()
    {
        // Skip single line comments (//)
        while (_currentChar != '\0' && _currentChar != '\n' && _currentChar != '\r')
        {
            NextChar();
        }

        // Skip the newline character if present
        if (_currentChar == '\n' || _currentChar == '\r')
        {
            NextChar();
        }
    }

    private Token ParseIdentifierOrKeyword()
    {
        int start = _position;
        int line = _line;
        int column = _column;

        while (_currentChar != '\0' && (char.IsLetterOrDigit(_currentChar) || _currentChar == '_'))
        {
            NextChar();
        }

        string result = _input.Substring(start, _position - start);

        if (Keywords.TryGetValue(result, out TokenType keywordType))
        {
            return new Token(keywordType, result, line, column);
        }
        return new Token(TokenType.IDENTIFIER, result, line, column);
    }

    private Token ParseNumber()
    {
        int start = _position;
        int line = _line;
        int column = _column;
        bool hasDot = false;
        bool isReal = false;

        while (_currentChar != '\0' && (char.IsDigit(_currentChar) || _currentChar == '.' || _currentChar == 'e' || _currentChar == 'E' || _currentChar == '-' || _currentChar == '+'))
        {
            if (_currentChar == '.')
            {
                if (hasDot)
                {
                    // Second dot encountered - return error
                    NextChar();
                    return new Token(TokenType.ERROR, "Multiple decimal points in number", line, column);
                }
                hasDot = true;
                isReal = true;
            }
            else if (_currentChar == 'e' || _currentChar == 'E')
            {
                isReal = true;
                NextChar();

                // Handle optional sign after exponent
                if (_currentChar == '+' || _currentChar == '-')
                {
                    NextChar();
                }

                // Must have digits after exponent
                if (_currentChar == '\0' || !char.IsDigit(_currentChar))
                {
                    return new Token(TokenType.ERROR, "Invalid exponent in number", line, column);
                }
                continue;
            }

            NextChar();
        }

        string numberValue = _input.Substring(start, _position - start);

        if (isReal)
            return new Token(TokenType.REAL_LITERAL, numberValue, line, column);
        else
            return new Token(TokenType.INT_LITERAL, numberValue, line, column);
    }

    private Token ParseString()
    {
        int line = _line;
        int column = _column;
        NextChar();
        var buffer = new System.Text.StringBuilder();
        while (_currentChar != '\0' && _currentChar != '"' && _currentChar != '\n' && _currentChar != '\r')
        {
            if (_currentChar == '\\')
            {
                NextChar();
                if (_currentChar == 'n') { buffer.Append('\n'); NextChar(); continue; }
                if (_currentChar == 't') { buffer.Append('\t'); NextChar(); continue; }
                if (_currentChar == '"') { buffer.Append('"'); NextChar(); continue; }
                if (_currentChar == '\\') { buffer.Append('\\'); NextChar(); continue; }
                // Unknown escape, keep literal char
            }
            else
            {
                buffer.Append(_currentChar);
                NextChar();
            }
        }
        if (_currentChar != '"')
        {
            return new Token(TokenType.ERROR, "Unterminated string literal", line, column);
        }
        // consume closing quote
        NextChar();
        return new Token(TokenType.STRING_LITERAL, buffer.ToString(), line, column);
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

            // Handle comments
            if (_currentChar == '/' && _position + 1 < _input.Length && _input[_position + 1] == '/')
            {
                NextChar(); // Skip first '/'
                NextChar(); // Skip second '/'
                SkipComment();
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

            if (_currentChar == '"')
            {
                return ParseString();
            }

            int currentLine = _line;
            int currentColumn = _column;

            switch (_currentChar)
            {
                case ':':
                    NextChar();
                    if (_currentChar == '=')
                    {
                        NextChar();
                        return new Token(TokenType.ASSIGN, ":=", currentLine, currentColumn);
                    }
                    return new Token(TokenType.COLON, ":", currentLine, currentColumn);

                case '=':
                    NextChar();
                    if (_currentChar == '>')
                    {
                        NextChar();
                        return new Token(TokenType.ARROW, "=>", currentLine, currentColumn);
                    }
                    if (_currentChar == '=')
                    {
                        NextChar();
                        return new Token(TokenType.EQEQ, "==", currentLine, currentColumn);
                    }
                    // Single '=' is not used in Project O, treat as error
                    return new Token(TokenType.ERROR, "Unexpected character: =", currentLine, currentColumn);

                case '!':
                    NextChar();
                    if (_currentChar == '=')
                    {
                        NextChar();
                        return new Token(TokenType.NEQ, "!=", currentLine, currentColumn);
                    }
                    return new Token(TokenType.ERROR, "Unexpected character: !", currentLine, currentColumn);

                case '.':
                    NextChar();
                    return new Token(TokenType.DOT, ".", currentLine, currentColumn);

                case ',':
                    NextChar();
                    return new Token(TokenType.COMMA, ",", currentLine, currentColumn);

                case ';':
                    NextChar();
                    return new Token(TokenType.SEMICOLON, ";", currentLine, currentColumn);

                case '(':
                    NextChar();
                    return new Token(TokenType.LEFT_PAREN, "(", currentLine, currentColumn);

                case ')':
                    NextChar();
                    return new Token(TokenType.RIGHT_PAREN, ")", currentLine, currentColumn);

                case '+':
                    NextChar();
                    return new Token(TokenType.PLUS, "+", currentLine, currentColumn);

                case '-':
                    NextChar();
                    return new Token(TokenType.MINUS, "-", currentLine, currentColumn);

                case '*':
                    NextChar();
                    return new Token(TokenType.STAR, "*", currentLine, currentColumn);

                case '/':
                    // Could be comment start handled above; otherwise operator
                    NextChar();
                    return new Token(TokenType.SLASH, "/", currentLine, currentColumn);

                case '>':
                    NextChar();
                    if (_currentChar == '=')
                    {
                        NextChar();
                        return new Token(TokenType.GE, ">=", currentLine, currentColumn);
                    }
                    return new Token(TokenType.GT, ">", currentLine, currentColumn);

                case '<':
                    NextChar();
                    if (_currentChar == '=')
                    {
                        NextChar();
                        return new Token(TokenType.LE, "<=", currentLine, currentColumn);
                    }
                    return new Token(TokenType.LT, "<", currentLine, currentColumn);

                case '{':
                    NextChar();
                    return new Token(TokenType.LEFT_BRACE, "{", currentLine, currentColumn);

                case '}':
                    NextChar();
                    return new Token(TokenType.RIGHT_BRACE, "}", currentLine, currentColumn);

                case '[':
                    NextChar();
                    return new Token(TokenType.LEFT_BRACKET, "[", currentLine, currentColumn);

                case ']':
                    NextChar();
                    return new Token(TokenType.RIGHT_BRACKET, "]", currentLine, currentColumn);

                default:
                    var errChar = _currentChar;
                    NextChar();
                    return new Token(TokenType.ERROR, $"Unexpected character: {errChar}", currentLine, currentColumn);
            }
        }
        return new Token(TokenType.EOF, string.Empty, _line, _column);
    }
}