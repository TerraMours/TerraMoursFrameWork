using TerraMours.Framework.Infrastructure.Filters;

namespace TerraMours.Domains.LoginDomain.MiniApi;

public class TestMiniApi : ServiceBase
{
    private readonly Serilog.ILogger _logger;

    public TestMiniApi(IServiceCollection services, Serilog.ILogger logger) : base()
    {
        _logger = logger;
        //此处/api/v1/Test 这里是swagger显示的路由
        App.MapGet("/api/v1/Test", TestMiniApiMethod);
        App.MapGet("/api/v1/Test2", TestMiniApiMethod222);
        /*        App.MapGet("/api/v1/catalog/items", GetItemsAsync);
                App.MapGet("/api/v1/catalog/brands", GetCatalogBrandsAsync);
                App.MapGet("/api/v1/catalog/types", GetCatalogTypesAsync);*/
    }

    [ExceptionFilter]
    public async Task<IResult> TestMiniApiMethod()
    {
        //throw new NotImplementedException("测试异常");
        return Results.Ok("Hello world");
    }


    public async Task<IResult> TestMiniApiMethod222()
    {
        var summaries = new[]
    {
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"};
        return Results.Ok("Hello world2222");

    }


}
