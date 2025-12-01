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

    public ProgramNode Parse()
    {
        var program = ParseProgram();
        Expect(TokenType.EOF);
        return program;
    }

    private ProgramNode ParseProgram()
    {
        // program := classDecl+
        var classes = new System.Collections.Generic.List<ClassDeclNode>();
        while (Check(TokenType.CLASS))
        {
            classes.Add(ParseClassDecl());
        }
        return new ProgramNode(classes);
    }

    private ClassDeclNode ParseClassDecl()
    {
        var classTok = ExpectWithReturn(TokenType.CLASS);
        var nameTok = ExpectWithReturn(TokenType.IDENTIFIER);
        if (TryMatch(TokenType.EXTENDS))
        {
            var baseTok = ExpectWithReturn(TokenType.IDENTIFIER);
            Expect(TokenType.IS);

            var thisStmts = new System.Collections.Generic.List<StatementNode>();
            if (TryMatch(TokenType.THIS))
            {
                Expect(TokenType.IS);
                while (!Check(TokenType.END))
                {
                    thisStmts.Add(ParseStatement());
                }
                Expect(TokenType.END);
            }

            var members = new System.Collections.Generic.List<MemberDeclNode>();
            while (!Check(TokenType.END))
            {
                if (Check(TokenType.VAR))
                {
                    members.Add(ParseVarDecl());
                }
                else if (Check(TokenType.METHOD))
                {
                    members.Add(ParseMethodDecl());
                }
                else
                {
                    throw Error($"Unexpected token '{_current.Type}' in class body");
                }
            }

            Expect(TokenType.END);
            return new ClassDeclNode(nameTok.Value, baseTok.Value, thisStmts, members, classTok.Line, classTok.Column);
        }
        Expect(TokenType.IS);

        var thisStatements = new System.Collections.Generic.List<StatementNode>();
        // optional 'this' block
        if (TryMatch(TokenType.THIS))
        {
            Expect(TokenType.IS);
            while (!Check(TokenType.END))
            {
                thisStatements.Add(ParseStatement());
            }
            Expect(TokenType.END);
        }

        var memberDecls = new System.Collections.Generic.List<MemberDeclNode>();
        while (!Check(TokenType.END))
        {
            if (Check(TokenType.VAR))
            {
                memberDecls.Add(ParseVarDecl());
            }
            else if (Check(TokenType.METHOD))
            {
                memberDecls.Add(ParseMethodDecl());
            }
            else
            {
                throw Error($"Unexpected token '{_current.Type}' in class body");
            }
        }

        Expect(TokenType.END);
        return new ClassDeclNode(nameTok.Value, null, thisStatements, memberDecls, classTok.Line, classTok.Column);
    }

    private VarDeclNode ParseVarDecl()
    {
        // var identifier : expression
        var varTok = ExpectWithReturn(TokenType.VAR);
        var nameTok = ExpectWithReturn(TokenType.IDENTIFIER);
        Expect(TokenType.COLON);
        var init = ParseExpression();
        return new VarDeclNode(nameTok.Value, init, varTok.Line, varTok.Column);
    }

    private LocalVarDeclStmtNode ParseLocalVarDecl()
    {
        var varTok = ExpectWithReturn(TokenType.VAR);
        var nameTok = ExpectWithReturn(TokenType.IDENTIFIER);
        Expect(TokenType.COLON);

        if (_current.Type == TokenType.SEMICOLON || _current.Type == TokenType.EOF)
        {
            throw Error("Incomplete variable declaration: expected expression after ':'");
        }

        var init = ParseExpression();
        return new LocalVarDeclStmtNode(nameTok.Value, init, varTok.Line, varTok.Column);
    }

    private MethodDeclNode ParseMethodDecl()
    {
        // method name [(params)] [: Type]? is statements end
        var methodTok = ExpectWithReturn(TokenType.METHOD);
        var nameTok = ExpectWithReturn(TokenType.IDENTIFIER);
        var parameters = new System.Collections.Generic.List<ParamNode>();
        if (TryMatch(TokenType.LEFT_PAREN))
        {
            if (!Check(TokenType.RIGHT_PAREN))
            {
                parameters.Add(ParseParam());
                while (TryMatch(TokenType.COMMA))
                {
                    parameters.Add(ParseParam());
                }
            }
            Expect(TokenType.RIGHT_PAREN);
        }

        TypeRefNode? returnType = null;
        if (TryMatch(TokenType.COLON))
        {
            // return type is an identifier (predefined class names) possibly with generic arguments, e.g. Array[Integer]
            returnType = ParseTypeRef();
        }

        Expect(TokenType.IS);
        var body = new System.Collections.Generic.List<StatementNode>();
        while (!Check(TokenType.END))
        {
            body.Add(ParseStatement());
        }
        Expect(TokenType.END);
        return new MethodDeclNode(nameTok.Value, parameters, returnType, body, methodTok.Line, methodTok.Column);
    }

    private ParamNode ParseParam()
    {
        // name : Type
        var nameTok = ExpectWithReturn(TokenType.IDENTIFIER);
        Expect(TokenType.COLON);
        var typeRef = ParseTypeRef();
        return new ParamNode(nameTok.Value, typeRef, nameTok.Line, nameTok.Column);
    }

    private TypeRefNode ParseTypeRef()
    {
        // Base type name: predefined or user-defined identifier
        var typeTok = ExpectOneOfWithReturn(
            TokenType.INTEGER,
            TokenType.REAL,
            TokenType.BOOLEAN,
            TokenType.ARRAY,
            TokenType.LIST,
            TokenType.ANYVALUE,
            TokenType.ANYREF,
            TokenType.IDENTIFIER);

        var typeName = typeTok.Value;

        // Optional generic arguments in square brackets, e.g. Array[Integer]
        if (TryMatch(TokenType.LEFT_BRACKET))
        {
            var genericArgs = new System.Collections.Generic.List<TypeRefNode>();

            // At least one type argument
            genericArgs.Add(ParseTypeRef());
            while (TryMatch(TokenType.COMMA))
            {
                genericArgs.Add(ParseTypeRef());
            }

            Expect(TokenType.RIGHT_BRACKET);
            return new TypeRefNode(typeName, typeTok.Line, typeTok.Column, genericArgs);
        }

        return new TypeRefNode(typeName, typeTok.Line, typeTok.Column);
    }

    private StatementNode ParseStatement()
    {
        if (Check(TokenType.VAR))
        {
            return ParseLocalVarDecl();
        }
        // Assignment: identifier := expression OR general expression statement
        if (Check(TokenType.IDENTIFIER))
        {
            // Peek by consuming identifier, then if ASSIGN treat as assignment
            var idTok = ExpectWithReturn(TokenType.IDENTIFIER);
            if (TryMatch(TokenType.ASSIGN))
            {
                var value = ParseExpression();
                return new AssignStmtNode(new IdentifierExprNode(idTok.Value, idTok.Line, idTok.Column), value, idTok.Line, idTok.Column);
            }
            // Otherwise, continue parsing full expression starting with this identifier
            var start = new IdentifierExprNode(idTok.Value, idTok.Line, idTok.Column);
            var expr = ParseExpressionStartingWith(start);
            return new ExprStmtNode(expr, idTok.Line, idTok.Column);
        }
        if (TryMatch(TokenType.IF))
        {
            var cond = ParseExpression();
            Expect(TokenType.THEN);
            var thenStmts = new System.Collections.Generic.List<StatementNode>();
            while (!Check(TokenType.END) && !Check(TokenType.ELSE))
            {
                thenStmts.Add(ParseStatement());
            }
            if (TryMatch(TokenType.ELSE))
            {
                var elseStmts = new System.Collections.Generic.List<StatementNode>();
                while (!Check(TokenType.END))
                {
                    elseStmts.Add(ParseStatement());
                }
                Expect(TokenType.END);
                return new IfStmtNode(cond, thenStmts, elseStmts, cond.Line, cond.Column);
            }
            Expect(TokenType.END);
            return new IfStmtNode(cond, thenStmts, null, cond.Line, cond.Column);
        }
        if (TryMatch(TokenType.WHILE))
        {
            var cond = ParseExpression();
            Expect(TokenType.LOOP);
            var body = new System.Collections.Generic.List<StatementNode>();
            while (!Check(TokenType.END))
            {
                body.Add(ParseStatement());
            }
            Expect(TokenType.END);
            return new WhileStmtNode(cond, body, cond.Line, cond.Column);
        }
        if (TryMatch(TokenType.RETURN))
        {
            var expr = ParseExpression();
            return new ReturnStmtNode(expr, expr.Line, expr.Column);
        }

        // expression statement (e.g., method call)
        var e = ParseExpression();
        return new ExprStmtNode(e, e.Line, e.Column);
    }

    private ExprNode ParseExpression()
    {
        var primary = ParsePrimary();
        var withPostfix = ParsePostfixChain(primary);
        return ParseBinaryRhs(0, withPostfix);
    }

    private ExprNode ParsePostfixChain(ExprNode start)
    {
        var expr = start;
        while (true)
        {
            if (TryMatch(TokenType.LEFT_PAREN))
            {
                var args = new System.Collections.Generic.List<ExprNode>();
                if (!Check(TokenType.RIGHT_PAREN))
                {
                    args.Add(ParseExpression());
                    while (TryMatch(TokenType.COMMA))
                    {
                        args.Add(ParseExpression());
                    }
                }
                Expect(TokenType.RIGHT_PAREN);
                expr = new CallExprNode(expr, args, expr.Line, expr.Column);
                continue;
            }
            if (TryMatch(TokenType.DOT))
            {
                var memberTok = ExpectWithReturn(TokenType.IDENTIFIER);
                expr = new MemberAccessExprNode(expr, memberTok.Value, memberTok.Line, memberTok.Column);
                continue;
            }
            break;
        }
        return expr;
    }

    private int GetPrecedence(TokenType type)
    {
        return type switch
        {
            TokenType.STAR or TokenType.SLASH => 3,
            TokenType.PLUS or TokenType.MINUS => 2,
            TokenType.GT or TokenType.LT or TokenType.GE or TokenType.LE or TokenType.EQEQ or TokenType.NEQ => 1,
            _ => -1
        };
    }

    private BinaryOperator TokenTypeToBinaryOp(TokenType type)
    {
        return type switch
        {
            TokenType.PLUS => BinaryOperator.Add,
            TokenType.MINUS => BinaryOperator.Subtract,
            TokenType.STAR => BinaryOperator.Multiply,
            TokenType.SLASH => BinaryOperator.Divide,
            TokenType.GT => BinaryOperator.GreaterThan,
            TokenType.LT => BinaryOperator.LessThan,
            TokenType.GE => BinaryOperator.GreaterThanOrEqual,
            TokenType.LE => BinaryOperator.LessThanOrEqual,
            TokenType.EQEQ => BinaryOperator.Equal,
            TokenType.NEQ => BinaryOperator.NotEqual,
            _ => throw Error($"Unexpected binary operator '{type}'")
        };
    }

    private ExprNode ParseBinaryRhs(int minPrecedence, ExprNode left)
    {
        while (true)
        {
            int precedence = GetPrecedence(_current.Type);
            if (precedence < minPrecedence) break;
            
            var opTok = _current; 
            _current = _lexer.GetNextToken();

            if (_current.Line > opTok.Line)
            {
                throw Error($"Incomplete binary expression: expected right operand after '{opTok.Value}'");
            }
            
            if (_current.Type == TokenType.EOF || 
                _current.Type == TokenType.SEMICOLON || 
                _current.Type == TokenType.END ||
                _current.Type == TokenType.RIGHT_PAREN ||
                GetPrecedence(_current.Type) >= 0)
            {
                throw Error($"Incomplete binary expression: expected right operand after '{opTok.Value}'");
            }
            
            var right = ParsePostfixChain(ParsePrimary());
            
            while (true)
            {
                int nextPrec = GetPrecedence(_current.Type);
                if (nextPrec > precedence)
                {
                    right = ParseBinaryRhs(precedence + 1, right);
                }
                else break;
            }
            left = new BinaryExprNode(left, TokenTypeToBinaryOp(opTok.Type), right, opTok.Line, opTok.Column);
        }
        return left;
    }

    private ExprNode ParseExpressionStartingWith(ExprNode start)
    {
        var withPostfix = ParsePostfixChain(start);
        return ParseBinaryRhs(0, withPostfix);
    }

    private ExprNode ParsePrimary()
    {
        if (Check(TokenType.IDENTIFIER))
        {
            var t = ExpectWithReturn(TokenType.IDENTIFIER);

            // Support generic type usage in expressions, e.g. Array[Integer](10)
            // For now we only need to consume the [TypeArgs] part so that examples
            // like Array[Integer](10) parse; semantic analysis can ignore the
            // generic arguments at expression level if not needed yet.
            if (TryMatch(TokenType.LEFT_BRACKET))
            {
                // Consume one or more type names separated by commas and a closing ']'
                do
                {
                    ExpectOneOf(TokenType.INTEGER, TokenType.REAL, TokenType.BOOLEAN, TokenType.ARRAY, TokenType.LIST, TokenType.ANYVALUE, TokenType.ANYREF, TokenType.IDENTIFIER);
                }
                while (TryMatch(TokenType.COMMA));

                Expect(TokenType.RIGHT_BRACKET);
            }

            return new IdentifierExprNode(t.Value, t.Line, t.Column);
        }
        if (Check(TokenType.INTEGER) || Check(TokenType.REAL) || Check(TokenType.BOOLEAN))
        {
            var t = ExpectOneOfWithReturn(TokenType.INTEGER, TokenType.REAL, TokenType.BOOLEAN);
            return new IdentifierExprNode(t.Value, t.Line, t.Column);
        }
        if (Check(TokenType.THIS))
        {
            var t = ExpectWithReturn(TokenType.THIS);
            return new ThisExprNode(t.Line, t.Column);
        }
        if (Check(TokenType.INT_LITERAL))
        {
            var t = ExpectWithReturn(TokenType.INT_LITERAL);
            return new IntLiteralExprNode(t.Value, t.Line, t.Column);
        }
        if (Check(TokenType.REAL_LITERAL))
        {
            var t = ExpectWithReturn(TokenType.REAL_LITERAL);
            return new RealLiteralExprNode(t.Value, t.Line, t.Column);
        }
        if (Check(TokenType.BOOL_LITERAL))
        {
            var t = ExpectWithReturn(TokenType.BOOL_LITERAL);
            return new BoolLiteralExprNode(t.Value == "true", t.Line, t.Column);
        }
        if (TryMatch(TokenType.LEFT_PAREN))
        {
            var inner = ParseExpression();
            Expect(TokenType.RIGHT_PAREN);
            return inner;
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
    private Token ExpectWithReturn(TokenType type)
    {
        if (Check(type))
        {
            var t = _current;
            _current = _lexer.GetNextToken();
            return t;
        }
        throw Error($"Expected {type} but found {_current.Type}");
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
    private Token ExpectOneOfWithReturn(params TokenType[] types)
    {
        foreach (var t in types)
        {
            if (Check(t))
            {
                var tok = _current;
                _current = _lexer.GetNextToken();
                return tok;
            }
        }
        throw Error($"Expected one of [{string.Join(", ", types)}] but found {_current.Type}");
    }

    private Exception Error(string message)
    {
        return new Exception($"{message} at {_current.Line}:{_current.Column}");
    }
}


