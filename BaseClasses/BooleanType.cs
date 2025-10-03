public class Boolean : AnyValue
{
    private bool _value;
    
    // Constructor
    public Boolean(Boolean p) => _value = p._value;
    public Boolean(bool value) => _value = value;
    
    public bool GetValue() => _value;
    
    // Conversion
    public Integer toInteger() => new Integer(_value ? 1 : 0);
    
    // Boolean operations
    public Boolean Or(Boolean p) => new Boolean(_value || p._value);
    public Boolean And(Boolean p) => new Boolean(_value && p._value);
    public Boolean Xor(Boolean p) => new Boolean(_value ^ p._value);
    public Boolean Not() => new Boolean(!_value);
    
    public override string ToString() => _value.ToString();
    public override bool Equals(object obj) => obj is Boolean other && _value == other._value;
    public override int GetHashCode() => _value.GetHashCode();
}