using Sandcube.Mth.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandcube.Interactions;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
public sealed class InteractionResult : CustomEnum<InteractionResult>, ICustomEnum<InteractionResult>
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
{
    public static readonly InteractionResult Pass = new(0, "Pass", false); // return if no action attempt was made
    public static readonly InteractionResult Success = new(1, "Success", true); // return if action attempt was made and succeded
    public static readonly InteractionResult Fail = new(2, "Fail", false); // return if action attempt was made but failed
    public static readonly InteractionResult Consume = new(3, "Consume", true); // same as Fail but consumes action

    public bool ConsumesAction { get; init; }

    [Obsolete("For serialization only", true)]
    public InteractionResult()
    {
    }

    private InteractionResult(int ordinal, string name, bool consumesAction) : base(ordinal, name)
    {
        ConsumesAction = consumesAction;
    }

    public static IReadOnlyList<InteractionResult> All { get; } = new List<InteractionResult>() { Pass, Success }.AsReadOnly();

    public static bool TryParse(string name, out InteractionResult value) => TryParse(All, name, out value);

    public static explicit operator InteractionResult(int ordinal) => All[ordinal];

    public static bool operator ==(InteractionResult a, InteractionResult b) => a.Ordinal == b.Ordinal;
    public static bool operator !=(InteractionResult a, InteractionResult b) => a.Ordinal != b.Ordinal;

    public override IEnumerable<CustomEnum> GetAll() => All;
}
