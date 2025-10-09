using System;

public class Parser
{
    private readonly Lexer _lexer;
    private Token _current;

    public Parser(string source)
    {
        _lexer = new Lexer(source);
        _current = _lexer.GetNextToken();
    }

    public bool Parse()
    {
        try
        {
            ParseProgram();
            Expect(TokenType.EOF);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Syntax error: {ex.Message}");
            return false;
        }
    }

    private void ParseProgram()
    {
        // program := classDecl+
        while (Check(TokenType.CLASS))
        {
            ParseClassDecl();
        }
    }

    private void ParseClassDecl()
    {
        Expect(TokenType.CLASS);
        Expect(TokenType.IDENTIFIER);
        if (TryMatch(TokenType.EXTENDS))
        {
            Expect(TokenType.IDENTIFIER);
        }

        Expect(TokenType.IS);

        // optional 'this' block
        if (TryMatch(TokenType.THIS))
        {
            Expect(TokenType.IS);
            while (!Check(TokenType.END))
            {
                ParseStatement();
            }
            Expect(TokenType.END);
        }

        while (!Check(TokenType.END))
        {
            if (Check(TokenType.VAR))
            {
                ParseVarDecl();
            }
            else if (Check(TokenType.METHOD))
            {
                ParseMethodDecl();
            }
            else
            {
                throw Error($"Unexpected token '{_current.Type}' in class body");
            }
        }

        Expect(TokenType.END);
    }

    private void ParseVarDecl()
    {
        // var identifier : expression
        Expect(TokenType.VAR);
        Expect(TokenType.IDENTIFIER);
        Expect(TokenType.COLON);
        ParseExpression();
    }

    private void ParseMethodDecl()
    {
        // method name [(params)] [: Type]? is statements end
        Expect(TokenType.METHOD);
        Expect(TokenType.IDENTIFIER);
        if (TryMatch(TokenType.LEFT_PAREN))
        {
            if (!Check(TokenType.RIGHT_PAREN))
            {
                ParseParam();
                while (TryMatch(TokenType.COMMA))
                {
                    ParseParam();
                }
            }
            Expect(TokenType.RIGHT_PAREN);
        }

        if (TryMatch(TokenType.COLON))
        {
            // return type is an identifier (predefined class names)
            ExpectOneOf(TokenType.INTEGER, TokenType.REAL, TokenType.BOOLEAN, TokenType.ARRAY, TokenType.LIST, TokenType.ANYVALUE, TokenType.ANYREF, TokenType.IDENTIFIER);
        }

        Expect(TokenType.IS);
        while (!Check(TokenType.END))
        {
            ParseStatement();
        }
        Expect(TokenType.END);
    }

    private void ParseParam()
    {
        // name : Type
        Expect(TokenType.IDENTIFIER);
        Expect(TokenType.COLON);
        ExpectOneOf(TokenType.INTEGER, TokenType.REAL, TokenType.BOOLEAN, TokenType.ARRAY, TokenType.LIST, TokenType.ANYVALUE, TokenType.ANYREF, TokenType.IDENTIFIER);
    }

    private void ParseStatement()
    {
        if (Check(TokenType.VAR))
        {
            ParseVarDecl();
            return;
        }
        // Assignment: identifier := expression
        if (Check(TokenType.IDENTIFIER))
        {
            // consume identifier and decide between assignment vs expression statement
            Match(TokenType.IDENTIFIER);
            if (TryMatch(TokenType.ASSIGN))
            {
                ParseExpression();
                return;
            }
            // continue parsing as an expression statement with the already-consumed identifier
            ParsePostfixChain();
            return;
        }
        if (TryMatch(TokenType.IF))
        {
            ParseExpression();
            Expect(TokenType.THEN);
            while (!Check(TokenType.END) && !Check(TokenType.ELSE))
            {
                ParseStatement();
            }
            if (TryMatch(TokenType.ELSE))
            {
                while (!Check(TokenType.END))
                {
                    ParseStatement();
                }
            }
            Expect(TokenType.END);
            return;
        }
        if (TryMatch(TokenType.WHILE))
        {
            ParseExpression();
            Expect(TokenType.LOOP);
            while (!Check(TokenType.END))
            {
                ParseStatement();
            }
            Expect(TokenType.END);
            return;
        }
        if (TryMatch(TokenType.RETURN))
        {
            ParseExpression();
            return;
        }

        // expression statement (e.g., method call)
        ParseExpression();
    }

    private void ParseExpression()
    {
        // Simplified: parse primary with chained .identifier calls and paren calls
        ParsePrimary();
        ParsePostfixChain();
    }

    private void ParsePostfixChain()
    {
        while (TryMatch(TokenType.DOT))
        {
            Expect(TokenType.IDENTIFIER);
            if (TryMatch(TokenType.LEFT_PAREN))
            {
                if (!Check(TokenType.RIGHT_PAREN))
                {
                    ParseExpression();
                    while (TryMatch(TokenType.COMMA))
                    {
                        ParseExpression();
                    }
                }
                Expect(TokenType.RIGHT_PAREN);
            }
        }
    }

    private void ParsePrimary()
    {
        if (TryMatch(TokenType.IDENTIFIER)) return;
        if (TryMatch(TokenType.THIS)) return;
        if (TryMatch(TokenType.INT_LITERAL)) return;
        if (TryMatch(TokenType.REAL_LITERAL)) return;
        if (TryMatch(TokenType.BOOL_LITERAL)) return;
        if (TryMatch(TokenType.LEFT_PAREN))
        {
            ParseExpression();
            Expect(TokenType.RIGHT_PAREN);
            return;
        }
        throw Error($"Unexpected token '{_current.Type}' in expression");
    }

    private bool Check(TokenType type) => _current.Type == type;
    private bool Match(TokenType type)
    {
        if (Check(type))
        {
            _current = _lexer.GetNextToken();
            return true;
        }
        return false;
    }
    private bool TryMatch(TokenType type) => Match(type);

    private void Expect(TokenType type)
    {
        if (!Match(type))
        {
            throw Error($"Expected {type} but found {_current.Type}");
        }
    }
    private void ExpectOneOf(params TokenType[] types)
    {
        foreach (var t in types)
        {
            if (Check(t))
            {
                _current = _lexer.GetNextToken();
                return;
            }
        }
        throw Error($"Expected one of [{string.Join(", ", types)}] but found {_current.Type}");
    }

    private Exception Error(string message)
    {
        return new Exception($"{message} at {_current.Line}:{_current.Column}");
    }
}


