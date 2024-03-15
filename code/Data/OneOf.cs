

namespace VoxelWorld.Data;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
public class OneOf<T0, T1> : OneOfBase
{
    private OneOf(int index, object? value) : base(index, value)
    {

    }

    public static implicit operator OneOf<T0, T1>(T0 t) => new(0, t);
    public static implicit operator OneOf<T0, T1>(T1 t) => new(1, t);

    public static bool operator==(OneOf<T0, T1> left, OneOf<T0, T1> right) => left.Equals(right);
    public static bool operator!=(OneOf<T0, T1> left, OneOf<T0, T1> right) => !left.Equals(right);
}

public class OneOf<T0, T1, T2> : OneOfBase
{
    private OneOf(int index, object? value) : base(index, value)
    {
    }

    public static implicit operator OneOf<T0, T1, T2>(T0 t) => new(0, t);
    public static implicit operator OneOf<T0, T1, T2>(T1 t) => new(1, t);
    public static implicit operator OneOf<T0, T1, T2>(T2 t) => new(2, t);

    public static bool operator ==(OneOf<T0, T1, T2> left, OneOf<T0, T1, T2> right) => left.Equals(right);
    public static bool operator !=(OneOf<T0, T1, T2> left, OneOf<T0, T1, T2> right) => !left.Equals(right);
}

public class OneOf<T0, T1, T2, T3> : OneOfBase
{
    private OneOf(int index, object? value) : base(index, value)
    {
    }

    public static implicit operator OneOf<T0, T1, T2, T3>(T0 t) => new(0, t);
    public static implicit operator OneOf<T0, T1, T2, T3>(T1 t) => new(1, t);
    public static implicit operator OneOf<T0, T1, T2, T3>(T2 t) => new(2, t);
    public static implicit operator OneOf<T0, T1, T2, T3>(T3 t) => new(3, t);

    public static bool operator ==(OneOf<T0, T1, T2, T3> left, OneOf<T0, T1, T2, T3> right) => left.Equals(right);
    public static bool operator !=(OneOf<T0, T1, T2, T3> left, OneOf<T0, T1, T2, T3> right) => !left.Equals(right);
}

public class OneOf<T0, T1, T2, T3, T4> : OneOfBase
{
    private OneOf(int index, object? value) : base(index, value)
    {
    }

    public static implicit operator OneOf<T0, T1, T2, T3, T4>(T0 t) => new(0, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4>(T1 t) => new(1, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4>(T2 t) => new(2, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4>(T3 t) => new(3, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4>(T4 t) => new(4, t);

    public static bool operator ==(OneOf<T0, T1, T2, T3, T4> left, OneOf<T0, T1, T2, T3, T4> right) => left.Equals(right);
    public static bool operator !=(OneOf<T0, T1, T2, T3, T4> left, OneOf<T0, T1, T2, T3, T4> right) => !left.Equals(right);
}

public class OneOf<T0, T1, T2, T3, T4, T5> : OneOfBase
{
    private OneOf(int index, object? value) : base(index, value)
    {
    }

    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5>(T0 t) => new(0, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5>(T1 t) => new(1, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5>(T2 t) => new(2, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5>(T3 t) => new(3, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5>(T4 t) => new(4, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5>(T5 t) => new(5, t);

    public static bool operator ==(OneOf<T0, T1, T2, T3, T4, T5> left, OneOf<T0, T1, T2, T3, T4, T5> right) => left.Equals(right);
    public static bool operator !=(OneOf<T0, T1, T2, T3, T4, T5> left, OneOf<T0, T1, T2, T3, T4, T5> right) => !left.Equals(right);
}

public class OneOf<T0, T1, T2, T3, T4, T5, T6> : OneOfBase
{
    private OneOf(int index, object? value) : base(index, value)
    {
    }

    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5, T6>(T0 t) => new(0, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5, T6>(T1 t) => new(1, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5, T6>(T2 t) => new(2, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5, T6>(T3 t) => new(3, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5, T6>(T4 t) => new(4, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5, T6>(T5 t) => new(5, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5, T6>(T6 t) => new(6, t);

    public static bool operator ==(OneOf<T0, T1, T2, T3, T4, T5, T6> left, OneOf<T0, T1, T2, T3, T4, T5, T6> right) => left.Equals(right);
    public static bool operator !=(OneOf<T0, T1, T2, T3, T4, T5, T6> left, OneOf<T0, T1, T2, T3, T4, T5, T6> right) => !left.Equals(right);
}

public class OneOf<T0, T1, T2, T3, T4, T5, T6, T7> : OneOfBase
{
    private OneOf(int index, object? value) : base(index, value)
    {
    }

    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5, T6, T7>(T0 t) => new(0, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5, T6, T7>(T1 t) => new(1, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5, T6, T7>(T2 t) => new(2, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5, T6, T7>(T3 t) => new(3, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5, T6, T7>(T4 t) => new(4, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5, T6, T7>(T5 t) => new(5, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5, T6, T7>(T6 t) => new(6, t);
    public static implicit operator OneOf<T0, T1, T2, T3, T4, T5, T6, T7>(T7 t) => new(7, t);

    public static bool operator ==(OneOf<T0, T1, T2, T3, T4, T5, T6, T7> left, OneOf<T0, T1, T2, T3, T4, T5, T6, T7> right) => left.Equals(right);
    public static bool operator !=(OneOf<T0, T1, T2, T3, T4, T5, T6, T7> left, OneOf<T0, T1, T2, T3, T4, T5, T6, T7> right) => !left.Equals(right);
}

#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
