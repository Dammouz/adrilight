namespace adrilight.Fakes
{
    internal class SerialStreamFake : ISerialStream
    {
        public bool IsRunning { get; set; } = false;

        public void Start()
        {
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public bool IsValid() => true;
    }
}
