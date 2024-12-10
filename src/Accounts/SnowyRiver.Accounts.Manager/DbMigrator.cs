using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SnowyRiver.Accounts.Domain.Entities;
using SnowyRiver.Accounts.Domain.Helpers;
using SnowyRiver.Accounts.EntityFramework;

namespace SnowyRiver.Accounts.Manager
{
    public class DbMigrator
    {
        public static async Task MigrateAsync(AccountsDbContext dbContext)
        {
            await dbContext.Database.MigrateAsync();


            #region 用户
            var defaultUserName = "admin";
            var isAnyUser = await dbContext.Users.AnyAsync();
            if (!isAnyUser)
            {
                const string userPasswordValue = "admin";
                var passwordSalt = PasswordHelper.CreateSalt();
                var passwordHash = PasswordHelper.CreatePassword(userPasswordValue, passwordSalt);
                var user = new User
                {
                    Name = defaultUserName,
                    Password = passwordHash,
                    PasswordSalt = passwordSalt,
                    UserId = 100001,
                };
                dbContext.Users.Add(user);
            }
            #endregion

            await dbContext.Database.EnsureCreatedAsync();

            await dbContext.SaveChangesAsync();
        }
    }
}
