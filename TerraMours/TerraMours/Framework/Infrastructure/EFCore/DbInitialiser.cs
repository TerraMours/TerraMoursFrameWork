using Microsoft.EntityFrameworkCore;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours_Gpt.Domains.GptDomain.IServices;

namespace TerraMours_Gpt.Framework.Infrastructure.EFCore {
    public class DbInitialiser {
        private readonly FrameworkDbContext _context;
        private readonly ISeedDataService _seedDataService;
        public DbInitialiser(FrameworkDbContext context, ISeedDataService seedDataService) {
            _context = context;
            _seedDataService = seedDataService;
        }

        public void Run() {
            var tables = _context.Database.GetDbConnection().GetSchema("Tables");
            System.Console.WriteLine($"数据库中表数：{tables.Rows.Count}");
            //hangfile初始化12张表
            if (tables.Rows.Count < 13) {
                System.Console.WriteLine("执行数据迁移");
                _context.Database.Migrate();
            }
            _seedDataService.EnsureSeedData();
        }
    }
}
