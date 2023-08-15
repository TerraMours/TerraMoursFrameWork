using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerraMours_Gpt.Domains.PayDomain.Contracts.Req;
using TerraMours_Gpt.Domains.PayDomain.IServices;

namespace TerraMours_Gpt.Domains.PayDomain.MiniApi
{
    /// <summary>
    /// 分类接口
    /// </summary>
    public class CategoryMiniApiService : ServiceBase
    {
        private readonly ICategoryService _categoryService;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="categoryService"></param>
        public CategoryMiniApiService(ICategoryService categoryService)
        {
            //ICategoryService AddCategory DeleteCategory GetCategoryById
            _categoryService = categoryService;
            App.MapPost("/api/v1/Category/AddCategory", AddCategory);
            App.MapGet("/api/v1/Category/GetCategoryById", GetCategoryById);
            App.MapGet("/api/v1/Category/GetAllUserList", GetAllCategoryList);
            App.MapPut("/api/v1/Category/DeleteCategory", DeleteCategory);
            App.MapPut("/api/v1/Category/UpdateCategory", UpdateCategory);
        }

        /// <summary>
        /// 查询所有分类信息
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> GetAllCategoryList()
        {
            var res = await _categoryService.GetAllCategoryList();
            return Results.Ok(res);
        }

        /// <summary>
        /// 新增分类信息
        /// </summary>
        /// <param name="categoryReq"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> AddCategory([FromBody] CategoryReq categoryReq)
        {
            var res = await _categoryService.AddCategory(categoryReq);
            return Results.Ok(res);
        }

        /// <summary>
        /// 删除分类信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> DeleteCategory(long id)
        {
            var res = await _categoryService.DeleteCategory(id);
            return Results.Ok(res);
        }

        /// <summary>
        /// 更新分类信息 
        /// </summary>
        /// <param name="categoryReq"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> UpdateCategory([FromBody] CategoryReq categoryReq)
        {
            var res = await _categoryService.UpdateCategory(categoryReq);
            return Results.Ok(res);
        }

        /// <summary>
        /// 根据id查询分类信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> GetCategoryById(long id)
        {
            var res = await _categoryService.GetCategoryById(id);
            return Results.Ok(res);
        }
    }
}
