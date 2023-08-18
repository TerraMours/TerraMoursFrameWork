using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerraMours_Gpt.Domains.PayDomain.Contracts.Req;
using TerraMours_Gpt.Domains.PayDomain.IServices;

namespace TerraMours_Gpt.Domains.PayDomain.MiniApi
{
    public class ProductMiniApiService : ServiceBase
    {
        private readonly IProductService _productService;
        public ProductMiniApiService(IProductService productService)
        {
            //IProductService
            _productService = productService;
            App.MapPost("/api/v1/Product/AddProduct", AddProduct);
            App.MapGet("/api/v1/Product/GetProductById", GetProductById);
            App.MapGet("/api/v1/Product/GetAllProductList", GetAllProductList);
            App.MapGet("/api/v1/Product/GetProductByCategoryId", GetProductByCategoryId);
            App.MapPut("/api/v1/Product/DeleteProduct", DeleteProduct);
            App.MapPut("/api/v1/Product/UpdateProduct", UpdateProduct);
        }

        /// <summary>
        /// 增加商品
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> AddProduct([FromBody] ProductReq req)
        {
            var res = await _productService.AddProduct(req);
            return Results.Ok(res);
        }

        /// <summary>
        /// 删除商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> DeleteProduct(long id)
        {
            var res = await _productService.DeleteProduct(id);
            return Results.Ok(res);
        }

        /// <summary>
        /// 修改商品信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> UpdateProduct([FromBody] ProductReq req)
        {
            var res = await _productService.UpdateProduct(req);
            return Results.Ok(res);
        }

        /// <summary>
        /// 查询所有商品信息
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> GetAllProductList()
        {
            var res = await _productService.GetAllProductList();
            return Results.Ok(res);
        }

        /// <summary>
        /// 根据id查询商品信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> GetProductById(long id)
        {
            var res = await _productService.GetProductById(id);
            return Results.Ok(res);
        }

        /// <summary>
        /// 根据分类id查询商品信息
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> GetProductByCategoryId(long categoryId)
        {
            var res = await _productService.GetProductByCategoryId(categoryId);
            return Results.Ok(res);
        }

    }
}
