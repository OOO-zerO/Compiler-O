
public interface LexerInterface
{
    public Lexer(string input) { }
    private void NextChar() { }
    private void SkipWhitespace() { }

    private Token ParseIdentifierOrKeyword() { }
    private void ParseNumber() { }
    public Token GetNextToken() { }
}

public class Lexer : LexerInterface
{
    private readonly string _input;
    private int _position;
    private char _currentChar;

    public Lexer(string input)
    {
        _input = input;
        _position = 0;
        _currentChar = _input[_position];
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
        while (char.IsWhiteSpace(_currentChar))
            NextChar();
    }

    private Token ParseIdentifierOrKeyword()
    {
        string result = "";
        while (_currentChar != '\0' && (char.IsLetterOrDigit(_currentChar) || _currentChar == '_'))
        {
            result += _currentChar;
            Advance();
        }

        if (Keywords.TryGetValue(result, out TokenType keywordType))
        {
            return new Token(keywordType, result);
        }
        return new Token(TokenType.IDENTIFIER, result);
    }

    private void ParseNumber()
    {
        var start = _position;

        // TODO: Handle second dot error

        while (char.IsDigit(_currentChar))
            NextChar();
        if (_currentChar == '.')
        {
            NextChar();
            while (char.IsDigit(_currentChar))
                NextChar();
            var floatValue = _input[start.._position];
            return new Token(TokenType.FLOAT_LITERAL, floatValue);
        }
        var intValue = _input[start.._position];
        return new Token(TokenType.INT_LITERAL, intValue);
    }

    private Token GetNextToken()
    {
        while (_currentChar != '\0')
        {
            if (char.IsWhiteSpace(_currentChar))
            {
                SkipWhitespace();
                continue;
            }

            if (char.IsLetter(_currentChar))
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
                    return new Token(TokenType.ERROR, "!");
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
                    NextChar();
                    var start = _position;
                    while (_currentChar != '"' && _currentChar != '\0')
                        NextChar();
                    var strValue = _input[start.._position];
                    if (_currentChar == '"')
                    {
                        NextChar();
                        return new Token(TokenType.STRING_LITERAL, strValue);
                    }
                    return new Token(TokenType.ERROR, "Unterminated string literal");
                default:
                    var errChar = _currentChar;
                    NextChar();
                    return new Token(TokenType.ERROR, $"Unexpected character: {errChar}");
            }
        }
        return new Token(TokenType.END_OF_FILE, string.Empty);
    }

}