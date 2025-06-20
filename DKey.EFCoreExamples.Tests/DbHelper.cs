using DKey.EFCoreExamples.Infrastructure;
using AutoMapper;
using DKey.EFCoreExamples.Domain;
using DKey.EFCoreExamples.Shared;
using DKey.EFCoreExamples.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace DKey.EFCoreExamples.Tests
{
    public class DbHelper
    {
        public RepositoryManager RepositoryManager { get; set; }
        
        public DbHelper(AppDbContext dbContext)
        {
            RepositoryManager = new RepositoryManager(dbContext, Mapper, new DbConfig());
            
        }

        public static IMapper Mapper => TestMapper.Instance;

        public async Task<UserDto?> AddDefaultUser(string name = "TestUser")
        {
            var userRepo = RepositoryManager.UserRepository;
            var user = new UserDto
            {
                Email = $"{name}@gmail.com",
                UserName = name,
                LoginMethod = LoginMethod.Password,
            };
            var passwordDto = new PasswordDto
            {
                PasswordHashOrKey = "hash",
                LoginMethod = LoginMethod.Password,
            };
        
            return await userRepo.AddOrUpdateUserAsync(user, passwordDto);
        }
    }
}