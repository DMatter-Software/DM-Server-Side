using DMServer.db;
using Microsoft.EntityFrameworkCore;

namespace DMServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Регистрируем DbContext в DI
            builder.Services.AddDbContext<Database>(options =>
                options.UseMySql(
                    "server=localhost;" +
                    "database=DarkMatter;" +
                    "user=root;" +
                    "password=roottest",
                    new MySqlServerVersion(new Version(10, 11, 0))
                )
            );

            // Add services to the container.
            builder.Services.AddControllers();

            var app = builder.Build();

            // Создаём базу данных при старте
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<Database>();
                db.Database.EnsureCreated(); // создаёт БД и таблицы, если их нет
            }

            // Configure the HTTP request pipeline.
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
