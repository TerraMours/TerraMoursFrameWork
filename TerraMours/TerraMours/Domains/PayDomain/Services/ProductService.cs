using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours.Framework.Infrastructure.Redis;
using TerraMours_Gpt.Domains.PayDomain.Contracts.Req;
using TerraMours_Gpt.Domains.PayDomain.Contracts.Res;
using TerraMours_Gpt.Domains.PayDomain.IServices;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.ProductModels;

namespace TerraMours_Gpt.Domains.PayDomain.Services
{
    /// <summary>
    /// 商品信息增删改查
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly FrameworkDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IDistributedCacheHelper _helper;
        public ProductService(FrameworkDbContext dbContext, IMapper mapper, IDistributedCacheHelper helper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _helper = helper;
        }

        /// <summary>
        /// 添加商品信息
        /// </summary>
        /// <param name="productReq"></param>
        /// <returns></returns>
        public async Task<ApiResponse<bool>> AddProduct(ProductReq productReq)
        {
            //var product = _mapper.Map<Product>(productReq);
            var product = new Product(productReq.Name, productReq.Description, productReq.Price, productReq.CategoryId, productReq.Stock);
            _mapper.Map(productReq, product);
            await _dbContext.Products.AddAsync(product);
            var res = await _dbContext.SaveChangesAsync();
            //删除过期的缓存
            await _helper.RemoveAsync("GetAllProductList");
            //把新的信息添加到redis中
            var categoryList = await _helper.GetOrCreateAsync("GetAllProductList", async options => { return await _dbContext.Products.ToListAsync(); });
            return ApiResponse<bool>.Success(true);
        }

        /// <summary>
        /// 删除商品信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ApiResponse<bool>> DeleteProduct(long id)
        {
            var category = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (category == null)
            {
                return ApiResponse<bool>.Fail("该商品不存在");
            }
            category.DeleteProduct();
            await _dbContext.SaveChangesAsync();
            await _helper.RemoveAsync("GetAllProductList");
            return ApiResponse<bool>.Success(true);
        }

        /// <summary>
        /// 查询所有的商品信息
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<List<ProductRes>>> GetAllProductList()
        {
            var productList = await _helper.GetOrCreateAsync("GetAllProductList", async options => { return await _dbContext.Products.Include(x => x.Category).ToListAsync(); });
            var productResList = _mapper.Map<List<ProductRes>>(productList);
            return ApiResponse<List<ProductRes>>.Success(productResList);
        }

        /// <summary>
        /// 根据分类id查询商品信息
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<ApiResponse<List<ProductRes>>> GetProductByCategoryId(long categoryId)
        {
            //to do
            //所有使用includ 后面逻辑需要修改或者说修改序列号哪里的逻辑 或者加个DTO 来存储然后再序列化之后再放进redis
            var productList = await _helper.GetOrCreateAsync("GetAllProductList", async options => { return await _dbContext.Products.Include(x => x.Category).ToListAsync(); });
            var productResList = productList?.FindAll(x => x.CategoryId == categoryId) ?? throw new Exception("该商品不存在");
            var res = _mapper.Map<List<ProductRes>>(productResList);
            return ApiResponse<List<ProductRes>>.Success(res);
        }

        /// <summary>
        /// 根据id查询商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<ApiResponse<ProductRes>> GetProductById(long id)
        {
            var productList = await _helper.GetOrCreateAsync("GetAllProductList", async options => { return await _dbContext.Products.Include(x => x.Category).ToListAsync(); }) ?? throw new Exception("该商品不存在");
            var product = productList.Find(x => x.Id == id);
            var res = _mapper.Map<ProductRes>(product);
            return ApiResponse<ProductRes>.Success(res);
        }

        /// <summary>
        /// 修改商品信息
        /// </summary>
        /// <param name="productReq"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<ApiResponse<bool>> UpdateProduct(ProductReq productReq)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == productReq.Id) ?? throw new Exception("该商品不存在");
            _mapper.Map(productReq, product);
            product.UpdateProduct(productReq.Name, product.Description, product.Price, product.CategoryId);
            _dbContext.Products.Update(product);
            var res = await _dbContext.SaveChangesAsync();
            //删除过期的缓存
            await _helper.RemoveAsync("GetAllCategoryList");
            return ApiResponse<bool>.Success(true);
        }
    }
}
