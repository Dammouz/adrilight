using System;
using adrilight.Resources;

namespace adrilight.Fakes
{
    internal class ContextFake : IContext
    {
        public bool IsSynchronized => true;

        public void BeginInvoke(Action action)
        {
        }

        public void Invoke(Action action)
        {
        }
    }
}
