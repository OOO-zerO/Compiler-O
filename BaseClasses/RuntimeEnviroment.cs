public class RuntimeEnvironment
{
    private Dictionary<string, Type> _builtInTypes;
    
    public RuntimeEnvironment()
    {
        _builtInTypes = new Dictionary<string, Type>
        {
            { "Integer", typeof(Integer) },
            { "Real", typeof(Real) },
            { "Boolean", typeof(Boolean) },
            { "Array", typeof(Array<>) },
            { "List", typeof(List<>) },
            { "AnyValue", typeof(AnyValue) },
            { "AnyRef", typeof(AnyRef) },
            { "Class", typeof(Class) }
        };
    }
    
    public object CreateInstance(string typeName, params object[] args)
    {
        if (_builtInTypes.TryGetValue(typeName, out Type type))
        {
            return Activator.CreateInstance(type, args);
        }
        throw new Exception($"Unknown built-in type: {typeName}");
    }
    
    public Type GetGenericTypeDefinition(string typeName)
    {
        if (_builtInTypes.TryGetValue(typeName, out Type type))
        {
            return type;
        }
        throw new Exception($"Unknown built-in type: {typeName}");
    }
}