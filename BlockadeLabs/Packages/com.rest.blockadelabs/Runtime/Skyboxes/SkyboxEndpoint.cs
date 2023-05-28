using System.Threading;
using System.Threading.Tasks;

namespace BlockadeLabs
{
    public sealed class SkyboxEndpoint : BlockadeLabsBaseEndpoint
    {
        public SkyboxEndpoint(BlockadeLabsClient client) : base(client) { }

        protected override string Root => string.Empty;

        public async Task GenerateSkyboxAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        public async Task GetSkyboxCallbackAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        public async Task TokenizePromptAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }
    }
}
