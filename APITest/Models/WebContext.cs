using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace APITest.Models
{
    public class WebContext : DbContext
    {
        public DbSet<Article> articles { set; get; }        // bảng article
        public DbSet<Tag> tags { set; get; }                // bảng tag

        // chuỗi kết nối với tên db sẽ làm  việc đặt là webdb
        public const string ConnectStrring = @"Server=localhost;Port=3306;Uid=root;Pwd=P@ssw0rd;Database=MySqlDB;Allow User Variables=true";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(ConnectStrring,ServerVersion.AutoDetect(ConnectStrring));
            optionsBuilder.UseLoggerFactory(GetLoggerFactory());       // bật logger
        }

        private ILoggerFactory GetLoggerFactory()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder =>
                    builder.AddConsole()
                           .AddFilter(DbLoggerCategory.Database.Command.Name,
                                    LogLevel.Information));
            return serviceCollection.BuildServiceProvider()
                    .GetService<ILoggerFactory>();
        }

    }
}
