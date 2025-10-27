
// enum with all types of symbols
public enum SymbolType
{
    Class,
    Method,
    Variable,
    Parameter
}

// classs of symbol struct
public class SymbolInfo
{
    public SymbolType Type { get; }
    public AstNode Node { get; }

    public SymbolInfo(SymbolType type, AstNode node)
    {
        Type = type;
        Node = node;
    }
}

public class SymbolTable
{
    // stack with scopes of symbols 
    private readonly Stack<Dictionary<string, SymbolInfo>> _scopes = new Stack<Dictionary<string, SymbolInfo>>();

    public SymbolTable()
    {
        // global vision scope
        EnterScope();
    }

    public void EnterScope()
    {
        _scopes.Push(new Dictionary<string, SymbolInfo>());
    }

    public void ExitScope()
    {
        if (_scopes.Count > 1)
        {
            _scopes.Pop();
        }
    }

    public bool AddSymbol(string name, SymbolInfo info)
    {
        var currentScope = _scopes.Peek();
        if (currentScope.ContainsKey(name))
        {
            return false;
        }
        currentScope[name] = info;
        return true;
    }

    public bool isSymbolDefined(string name)
    {
        foreach (var scope in _scopes)
        {
            if (scope.ContainsKey(name))
            {
                return true;
            }
        }
        return false;
    }
    
    public SymbolInfo? GetSymbol(string name)
    {
        foreach (var scope in _scopes)
        {
            if (scope.ContainsKey(name))
            {
                return scope[name];
            }
        }
        return null;
    }
}
