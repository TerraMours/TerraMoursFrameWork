using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.Commons;
using TerraMours.Framework.Infrastructure.EFCore;

namespace TerraMours_Gpt.Framework.Infrastructure.Middlewares {
    public class KeyMiddleware {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<GptOptions> _options;
        private readonly Lazy<string[]> _lazyKeyList;
        private int _index;

        public KeyMiddleware(RequestDelegate next, IOptions<GptOptions> options, IServiceProvider serviceProvider) {
            _next = next;
            _serviceProvider = serviceProvider;
            _options = options;
            _lazyKeyList = new Lazy<string[]>(GetKeyList);
            _index = 0;
        }

        public async Task InvokeAsync(HttpContext context) {
            var keyList = _lazyKeyList.Value;
            var item = keyList[_index];
            _index = (_index + 1) % keyList.Length;
            context.Items["key"] = item;
            await _next(context);
        }

        private string[] GetKeyList() {
            using (var scope = _serviceProvider.CreateScope()) {
                var dbContext = scope.ServiceProvider.GetRequiredService<FrameworkDbContext>();
                var gptOptions = dbContext.GptOptions
                    .AsNoTracking()
                    .OrderBy(m => m.GptOptionsId)
                    .FirstOrDefault();

                if (gptOptions != null && gptOptions.OpenAIOptions?.OpenAI?.KeyList != null) {
                    return gptOptions.OpenAIOptions.OpenAI.KeyList;
                }

                return GetDefaultKeyList();
            }
        }

        private string[] GetDefaultKeyList() {
            var list = _options.Value.OpenAIOptions?.OpenAI?.KeyList;
            return list;
        }
    }

}
