using Microsoft.Extensions.Options;
using TerraMours.Framework.Infrastructure.Contracts.Commons;

namespace TerraMours_Gpt.Framework.Infrastructure.Middlewares {
    public class KeyMiddleware {
        private readonly RequestDelegate _next;
        private readonly string[] _list;
        private int _index;
        public KeyMiddleware(RequestDelegate next, IOptions<OpenAIOptions> options) {
            var list = options.Value.OpenAI.KeyList;
            _next = next;
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
