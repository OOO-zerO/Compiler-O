public class Integer : AnyValue
{
    private int _value;
    
    public static Integer Min => new Integer(int.MinValue);
    public static Integer Max => new Integer(int.MaxValue);
    
    // Constructors
    public Integer(Integer p) => _value = p._value;
    public Integer(Real p) => _value = (int)p.GetValue();
    
    public Integer(int value) => _value = value;
    
    public int GetValue() => _value;
    
    // Conversions
    public Real toReal() => new Real(_value);
    public Boolean toBoolean() => new Boolean(_value != 0);
    
    // Unary operators
    public Integer UnaryMinus() => new Integer(-_value);
    
    // Arithmetic operations
    public Integer Plus(Integer p) => new Integer(_value + p._value);
    public Real Plus(Real p) => new Real(_value + p.GetValue());
    
    public Integer Minus(Integer p) => new Integer(_value - p._value);
    public Real Minus(Real p) => new Real(_value - p.GetValue());
    
    public Integer Mult(Integer p) => new Integer(_value * p._value);
    public Real Mult(Real p) => new Real(_value * p.GetValue());
    
    public Integer Div(Integer p) => new Integer(_value / p._value);
    public Real Div(Real p) => new Real(_value / p.GetValue());
    
    public Integer Rem(Integer p) => new Integer(_value % p._value);
    
    // Comparison operations
    public Boolean Less(Integer p) => new Boolean(_value < p._value);
    public Boolean Less(Real p) => new Boolean(_value < p.GetValue());
    
    public Boolean LessEqual(Integer p) => new Boolean(_value <= p._value);
    public Boolean LessEqual(Real p) => new Boolean(_value <= p.GetValue());
    
    public Boolean Greater(Integer p) => new Boolean(_value > p._value);
    public Boolean Greater(Real p) => new Boolean(_value > p.GetValue());
    
    public Boolean GreaterEqual(Integer p) => new Boolean(_value >= p._value);
    public Boolean GreaterEqual(Real p) => new Boolean(_value >= p.GetValue());
    
    public Boolean Equal(Integer p) => new Boolean(_value == p._value);
    public Boolean Equal(Real p) => new Boolean(_value == p.GetValue());
    
    public override string ToString() => _value.ToString();
    public override bool Equals(object obj) => obj is Integer other && _value == other._value;
    public override int GetHashCode() => _value.GetHashCode();
}