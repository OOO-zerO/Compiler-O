public class Real : AnyValue
{
    private double _value;
    
    public static Real Min => new Real(double.MinValue);
    public static Real Max => new Real(double.MaxValue);
    public static Real Epsilon => new Real(double.Epsilon);
    
    // Constructors
    public Real(Real p) => _value = p._value;
    public Real(Integer p) => _value = p.GetValue();
    
    public Real(double value) => _value = value;
    
    public double GetValue() => _value;
    
    // Conversions
    public Integer toInteger() => new Integer((int)_value);
    
    // Unary operators
    public Real UnaryMinus() => new Real(-_value);
    
    // Arithmetic operations
    public Real Plus(Real p) => new Real(_value + p._value);
    public Real Plus(Integer p) => new Real(_value + p.GetValue());
    
    public Real Minus(Real p) => new Real(_value - p._value);
    public Real Minus(Integer p) => new Real(_value - p.GetValue());
    
    public Real Mult(Real p) => new Real(_value * p._value);
    public Real Mult(Integer p) => new Real(_value * p.GetValue());
    
    public Real Div(Integer p) => new Real(_value / p.GetValue());
    public Real Div(Real p) => new Real(_value / p._value);
    
    public Real Rem(Integer p) => new Real(_value % p.GetValue());
    
    // Comparison operations
    public Boolean Less(Real p) => new Boolean(_value < p._value);
    public Boolean Less(Integer p) => new Boolean(_value < p.GetValue());
    
    public Boolean LessEqual(Real p) => new Boolean(_value <= p._value);
    public Boolean LessEqual(Integer p) => new Boolean(_value <= p.GetValue());
    
    public Boolean Greater(Real p) => new Boolean(_value > p._value);
    public Boolean Greater(Integer p) => new Boolean(_value > p.GetValue());
    
    public Boolean GreaterEqual(Real p) => new Boolean(_value >= p._value);
    public Boolean GreaterEqual(Integer p) => new Boolean(_value >= p.GetValue());
    
    public Boolean Equal(Real p) => new Boolean(_value == p._value);
    public Boolean Equal(Integer p) => new Boolean(_value == p.GetValue());
    
    public override string ToString() => _value.ToString();
    public override bool Equals(object obj) => obj is Real other && _value == other._value;
    public override int GetHashCode() => _value.GetHashCode();
}