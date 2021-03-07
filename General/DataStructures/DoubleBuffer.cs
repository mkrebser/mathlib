using System.Collections;
using System.Collections.Generic;

public struct DoubleBuffer<T>
{
    public T _buffer_1;
    public T _buffer_2;
    private bool _buffer_1_active;
    public bool Buffer1Active() { return _buffer_1_active; }

    public DoubleBuffer(T val1, T val2) { _buffer_1 = val1; _buffer_2 = val2; _buffer_1_active = false; }

    public T GetActive()
    {
        if (_buffer_1_active)
            return _buffer_1;
        else
            return _buffer_2;
    }
    public void Swap()
    {
        _buffer_1_active = !_buffer_1_active;
    }
}
