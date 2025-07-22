namespace SectigoCertificateManager.Utilities;

using System;
using System.Buffers;

/// <summary>
/// Provides a minimal growable string builder that uses <see cref="ArrayPool{T}"/>.
/// </summary>
internal ref struct ValueStringBuilder {
    private char[]? _array;
    private Span<char> _span;
    private int _pos;

    /// <summary>Initializes a new instance with the given initial buffer.</summary>
    /// <param name="initialBuffer">Buffer used until growth is required.</param>
    public ValueStringBuilder(Span<char> initialBuffer) {
        _array = null;
        _span = initialBuffer;
        _pos = 0;
    }

    /// <summary>Gets the current length.</summary>
    public int Length => _pos;

    /// <summary>Appends a single character to the builder.</summary>
    /// <param name="c">Character to append.</param>
    public void Append(char c) {
        if (_pos >= _span.Length) {
            Grow(1);
        }

        _span[_pos] = c;
        _pos++;
    }

    /// <summary>Appends the provided span to the builder.</summary>
    /// <param name="value">Characters to append.</param>
    public void Append(ReadOnlySpan<char> value) {
        if (_pos > _span.Length - value.Length) {
            Grow(value.Length);
        }

        value.CopyTo(_span.Slice(_pos));
        _pos += value.Length;
    }

    /// <summary>Clears the builder.</summary>
    public void Clear() => _pos = 0;

    private void Grow(int additionalCapacity) {
        var newSize = Math.Max(_pos + additionalCapacity, _span.Length * 2);
        var newArray = ArrayPool<char>.Shared.Rent(newSize);
        _span.Slice(0, _pos).CopyTo(newArray);
        var toReturn = _array;
        _span = _array = newArray;
        if (toReturn is not null) {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }

    /// <summary>Returns the accumulated string.</summary>
    public override string ToString() {
#if NETSTANDARD2_0 || NET472
        return new string(_span.Slice(0, _pos).ToArray());
#else
        return new string(_span.Slice(0, _pos));
#endif
    }

    /// <summary>Returns the rented array to the pool.</summary>
    public void Dispose() {
        var toReturn = _array;
        this = default;
        if (toReturn is not null) {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }
}
