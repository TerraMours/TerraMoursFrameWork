namespace TerraMours.Domains.LoginDomain.MiniApi;

public class TestMiniApi : ServiceBase
{

    public TestMiniApi(IServiceCollection services) : base(services)
    {
        //此处/api/v1/Test 这里是swagger显示的路由
        App.MapGet("/api/v1/Test", TestMiniApiMethod);
        App.MapGet("/api/v1/Test2", TestMiniApiMethod222);
        /*        App.MapGet("/api/v1/catalog/items", GetItemsAsync);
                App.MapGet("/api/v1/catalog/brands", GetCatalogBrandsAsync);
                App.MapGet("/api/v1/catalog/types", GetCatalogTypesAsync);*/
    }


    public async Task<IResult> TestMiniApiMethod()
    {
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
