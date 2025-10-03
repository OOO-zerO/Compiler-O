public class Array<T> : AnyRef where T : AnyValue
{
    private T[] _items;
    
    // Constructor
    public Array(Integer length)
    {
        _items = new T[length.GetValue()];
    }
    
    // Conversion
    public List<T> toList()
    {
        var list = new List<T>();
        foreach (var item in _items)
        {
            list.append(item);
        }
        return list;
    }
    
    // Features
    public Integer Length() => new Integer(_items.Length);
    
    // Access to elements
    public T get(Integer index) => _items[index.GetValue()];
    
    public void set(Integer index, T value)
    {
        _items[index.GetValue()] = value;
    }
}