using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace adrilight.Resources
{
    internal class WpfContext : IContext
    {
        private readonly Dispatcher _dispatcher;

        public bool IsSynchronized
        {
            get
            {
                return _dispatcher.Thread == Thread.CurrentThread;
            }
        }

        public WpfContext()
            : this(Dispatcher.CurrentDispatcher)
        {
        }

        public WpfContext(Dispatcher dispatcher)
        {
            Debug.Assert(dispatcher != null);

            _dispatcher = dispatcher;
        }

        public void Invoke(Action action)
        {
            Debug.Assert(action != null);

            _dispatcher.Invoke(action);
        }

        public void BeginInvoke(Action action)
        {
            Debug.Assert(action != null);

            _dispatcher.BeginInvoke(action);
        }
    }
}
