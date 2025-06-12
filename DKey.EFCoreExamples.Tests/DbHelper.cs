using DKey.EFCoreExamples.Infrastructure;
using AutoMapper;
using DKey.EFCoreExamples.Domain;
using DKey.EFCoreExamples.Shared;
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

    }

}

