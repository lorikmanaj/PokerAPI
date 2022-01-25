using DataService.Helpers;
using FunctionalService;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService
{
    public static class DbContextInitializer
    {
        public static async Task Initialize(DataProtectionKeysContext dataProtectionKeysContext,
            ApplicationDbContext applicationDbContext,
            IFunctionalSvc functionalService)
        {
            try
            {
                await dataProtectionKeysContext.Database.EnsureCreatedAsync();
                await applicationDbContext.Database.EnsureCreatedAsync();

                if (!applicationDbContext.ApplicationUsers.Any()) 
                {
                    await functionalService.CreateDefaultAdminUser();
                    await functionalService.CreateDefaultUser();
                }

                if (!applicationDbContext.Rooms.Any())
                {
                    var dataHelper = new DefaultDataHelper(applicationDbContext);
                    await dataHelper.CreateDefaultRooms();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error while initializing database: {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
        }
    }
}
