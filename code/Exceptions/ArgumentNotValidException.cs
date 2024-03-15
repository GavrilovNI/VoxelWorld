using Sandbox;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace VoxelWorld.Exceptions;

internal class ArgumentNotValidException : ArgumentException
{
    public ArgumentNotValidException() : this(null)
    {
    }

    public ArgumentNotValidException(string? paramName) : base("Value should be valid", paramName)
    {
    }

    public ArgumentNotValidException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public ArgumentNotValidException(string? paramName, string? message) : base(message, paramName)
    {
    }

    public static void ThrowIfNotValid([NotNull] IValid argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if(!argument.IsValid())
            throw new ArgumentNullException(paramName);
    }
}
