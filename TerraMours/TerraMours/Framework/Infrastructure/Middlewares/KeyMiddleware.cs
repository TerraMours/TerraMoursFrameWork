using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.Commons;
using TerraMours.Framework.Infrastructure.EFCore;

namespace TerraMours_Gpt.Framework.Infrastructure.Middlewares {
    public class KeyMiddleware {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly string[] _list;
        private int _index;
        public KeyMiddleware(RequestDelegate next, IOptions<GptOptions> options, IServiceProvider serviceProvider) {
            var list = options.Value.OpenAIOptions.OpenAI.KeyList;
            _next = next;
            _serviceProvider = serviceProvider;
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<FrameworkDbContext>();
                // 然后在此处使用dbContext
                var gptOptions = dbContext.GptOptions.AsNoTracking()
                    .OrderBy(m => m.GptOptionsId)
                    .FirstOrDefault();
                if (gptOptions != null) {
                    list = gptOptions.OpenAIOptions.OpenAI.KeyList;
                }
            }
            _list = list ?? throw new ArgumentNullException(nameof(list));
            if (_list.Length == 0) throw new ArgumentException("List cannot be empty.", nameof(list));
        }

        public async Task InvokeAsync(HttpContext context) {
            var item = _list[_index];
            _index = (_index + 1) % _list.Length;
            // 将数据存储在 HttpContext.Items 中
            context.Items["key"] = item;
            await _next(context);
        }
    }
}
