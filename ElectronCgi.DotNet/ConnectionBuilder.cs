namespace ElectronCgi.DotNet
{
    public class ConnectionBuilder
    {
        private string _logFilePath;
        public ConnectionBuilder WithLogging(string logFilePath = null)
        {
            _logFilePath = logFilePath;
            return this;
        }

        public Connection Build()
        {
            var connection = new Connection(new Channel(new TabSeparatedInputStreamParser(), new JsonSerialiser()), new JsonSerialiser());
            return connection;
        }
    }
}