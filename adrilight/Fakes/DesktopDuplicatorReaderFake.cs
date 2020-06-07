using System.Threading;
using System.Threading.Tasks;

namespace adrilight.Fakes
{
    internal class DesktopDuplicatorReaderFake : IDesktopDuplicatorReader
    {
        public RunStateEnum RunState { get; set; } = RunStateEnum.Stopped;

        public void Run(CancellationToken token)
        {
            Task.Factory.StartNew(() =>
            {
                RunState = RunStateEnum.Running;
                try
                {
                    token.WaitHandle.WaitOne();
                }
                finally
                {
                    RunState = RunStateEnum.Stopped;
                }
            });
        }
    }
}
