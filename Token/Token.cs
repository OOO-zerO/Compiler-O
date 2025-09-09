
public class Token
{
    public TokenType Type { get; }
    public string value { get; }

    public Token(TokenType type, string value)
    {
        Type = type;
        this.value = value;
    }
}