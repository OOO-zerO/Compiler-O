public class List<T> : AnyRef where T : AnyValue
{
    private System.Collections.Generic.List<T> _items;
    
    // Constructors
    public List() => _items = new System.Collections.Generic.List<T>();
    public List(T p) => _items = new System.Collections.Generic.List<T> { p };
    public List(T p, Integer count)
    {
        _items = new System.Collections.Generic.List<T>();
        for (int i = 0; i < count.GetValue(); i++)
        {
            _items.Add(p);
        }
    }
    
    // Operations
    public List<T> append(T v)
    {
        _items.Add(v);
        return this;
    }
    
    public T head() => _items.Count > 0 ? _items[0] : default;
    
    public List<T> tail()
    {
        var newList = new List<T>();
        if (_items.Count > 1)
        {
            for (int i = 1; i < _items.Count; i++)
            {
                newList.append(_items[i]);
            }
        }
        return newList;
    }
    
    public Integer Size() => new Integer(_items.Count);
}