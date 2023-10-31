using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.Commons;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours.Framework.Infrastructure.Redis;

namespace TerraMours_Gpt.Framework.Infrastructure.Middlewares {
    public class KeyMiddleware {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<GptOptions> _options;
        private int _index;

        public KeyMiddleware(RequestDelegate next, IOptions<GptOptions> options, IServiceProvider serviceProvider) {
            _next = next;
            _serviceProvider = serviceProvider;
            _options = options;
            _index = 0;
        }

        public async Task InvokeAsync(HttpContext context) {
            var endpoint = context.GetEndpoint();
            var isEnabled = endpoint?.Metadata.GetMetadata<KeyMiddlewareEnabledAttribute>()?.Enabled ?? false;
            if (isEnabled)
            {
                var keyList = GetKeyList();
                var item = keyList[_index];
                _index = (_index + 1) % keyList.Length;
                context.Items["key"] = item;
            }
            await _next(context);
        }

        private string[] GetKeyList() {
            using (var scope = _serviceProvider.CreateScope()) {
                var _helper = scope.ServiceProvider.GetRequiredService<IDistributedCacheHelper>();
                //key池缓存
                var keyList =  _helper.GetOrCreate("GetKey",  options =>
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<FrameworkDbContext>();
                    var gptOptions = dbContext.GptOptions
                        .AsNoTracking()
                        .OrderBy(m => m.GptOptionsId)
                        .FirstOrDefault();

                    if (gptOptions != null && gptOptions.OpenAIOptions?.OpenAI?.KeyList != null) {
                        return gptOptions.OpenAIOptions.OpenAI.KeyList;
                    }
                    return GetDefaultKeyList();
                });
                return keyList;
            }
        }

        private string[] GetDefaultKeyList() {
            var list = _options.Value.OpenAIOptions?.OpenAI?.KeyList;
            return list;
        }
    }

}
