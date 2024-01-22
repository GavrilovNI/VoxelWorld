using Sandcube.Mth.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandcube.Blocks.States.Properties;

public class FilteredBlockProperty<T> : BlockProperty<T> where T : CustomEnum<T>, ICustomEnum<T>
{
    public readonly Func<T, bool> Filter;

    public FilteredBlockProperty(string name, Func<T, bool> filter) : base(name)
    {
        Filter = filter;
    }

    public override IEnumerable<CustomEnum> GetAllValues() => base.GetAllValues().Cast<T>().Where(Filter);
}
