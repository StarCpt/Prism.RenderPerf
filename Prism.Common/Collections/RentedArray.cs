using System;
using System.Buffers;

namespace Prism.Common.Collections;

public readonly struct RentedArray<T> : IDisposable
{
    public int Length => _length;
    public Span<T> Data => new Span<T>(_array, 0, _length);

    private readonly int _length;
    private readonly T[] _array;

    public RentedArray(int length, bool clear = true)
    {
        _length = length;
        _array = ArrayPool<T>.Shared.Rent(_length);

        if (clear)
        {
            Array.Clear(_array, 0, _length);
        }
    }

    public void Dispose()
    {
        // TODO: don't clear array when T is unmanaged (how?)
        ArrayPool<T>.Shared.Return(_array, true);
    }

    public ArraySegment<T> GetArraySegment() => new ArraySegment<T>(_array, 0, _length);
    public Memory<T> AsMemory() => new Memory<T>(_array, 0, _length);
}
