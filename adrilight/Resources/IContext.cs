using System;

namespace adrilight.Resources
{
    public interface IContext
    {
        bool IsSynchronized { get; }
        void Invoke(Action action);
        void BeginInvoke(Action action);
    }
}
