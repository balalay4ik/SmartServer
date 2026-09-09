// using HomeServer.Persistence.Context;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Design;

// namespace HomeServer.Persistence.Context;

// public class AppDbContextFactory
//     : IDesignTimeDbContextFactory<AppDbContext>
// {
//     public AppDbContext CreateDbContext(string[] args)
//     {
//         var optionsBuilder =
//             new DbContextOptionsBuilder<AppDbContext>();

//         optionsBuilder.UseSqlite(
//             "Data Source=homeserver.db");

//         return new AppDbContext(optionsBuilder.Options);
//     }
// }