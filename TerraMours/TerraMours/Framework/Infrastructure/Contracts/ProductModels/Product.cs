using TerraMours.Framework.Infrastructure.Contracts.SystemModels;

namespace TerraMours_Gpt.Framework.Infrastructure.Contracts.ProductModels
{
    /// <summary>
    /// 商品表
    /// </summary>
    public class Product : BaseEntity
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 商品描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 商品价格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 商品折扣 默认不打折
        /// </summary>
        public decimal? Discount { get; set; }
        /// <summary>
        /// 商品图片路径
        /// </summary>
        public string? ImagePath { get; set; }
        /// <summary>
        /// 商品库存
        /// </summary>
        public int? Stock { get; set; }
        /// <summary>
        /// 商品分类Id
        /// </summary>
        public long CategoryId { get; set; }
        /// <summary>
        /// 商品分类
        /// </summary>
        public Category Category { get; set; }

        /// <summary>
        /// 初始化商品实体类
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="price"></param>
        /// <param name="categoryId"></param>
        /// <param name="stock"></param>
        public Product(string name, string description, decimal price, long categoryId, int? stock)
        {
            //初始化用户 ：以下是有用的字段
            this.Name = name;
            this.Description = description;
            this.Price = price;
            this.CategoryId = categoryId;
            //EntityBase
            Enable = true;
            CreateDate = DateTime.Now;
            this.Stock = stock;
        }

        /// <summary>
        /// 修改商品实体类信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="price"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public Product UpdateProduct(string name, string description, decimal price, long categoryId)
        {
            this.Name = name;
            this.Description = description;
            this.Price = price;
            this.CategoryId = categoryId;
            this.ModifyDate = DateTime.Now;
            return this;
        }

        /// <summary>
        /// 下架分类 或者删除分类
        /// </summary>
        /// <returns></returns>
        public Product DeleteProduct()
        {
            this.Enable = false;
            return this;
        }

    }
}
