using System;

namespace Sandcube.Data;

public interface IOneOf
{
    object? Value { get; }
    int Index { get; }
}
