namespace DogCrateBot.Services
{
    public class HttpListenerService : IHostedService
    {
        private readonly HttpListener _listener = new();

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await ListenAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _listener.Close();

            return Task.CompletedTask;
        }

        public async Task ListenAsync()
        {
            _listener.Prefixes.Add("http://+:7860/");
            _listener.Start();
            HttpListenerContext context = await _listener.GetContextAsync();
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }
    }
}
