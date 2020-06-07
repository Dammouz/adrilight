using System;

namespace adrilight.ViewModel
{
    public interface ISelectableViewPart
    {
        int Order { get; }
        string ViewPartName { get; }
        object Content { get; }
    }
}
