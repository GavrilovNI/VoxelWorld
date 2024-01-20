using System;

namespace Sandcube.Data;

public class OneOfBase : IOneOf
{
    public int Index { get; }
    public object? Value { get; }

    public OneOfBase(int index, object? value)
    {
        Index = index;
        Value = value;
    }

    protected object? Get(int index) => index == Index ? Value : null;

    public T? As<T>() => Value is T t ? t : default;
    public bool Is<T>() => Value is T;
    public bool Is<T>(out T t)
    {
        if(Value is T t2)
        {
            t = t2;
            return true;
        }
        t = default!;
        return false;
    }

    public virtual bool ValueEquals(OneOfBase oneOfBase) => object.Equals(Value, oneOfBase.Value);
    public virtual bool Equals(OneOfBase oneOfBase) => object.ReferenceEquals(this, oneOfBase) ||
        Index == oneOfBase.Index && ValueEquals(oneOfBase);
    public override bool Equals(object? obj) => obj is OneOfBase oneOfBase && Equals(oneOfBase);
    public override int GetHashCode() => HashCode.Combine(Index, Value);
}
