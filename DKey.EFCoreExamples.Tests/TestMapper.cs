using AutoMapper;
using DKey.EFCoreExamples.Infrastructure;

namespace DKey.EFCoreExamples.Tests;

public class TestMapper
{
    private static IMapper? _instance;
    public static IMapper Instance => _instance ??= new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<MappingProfile>();
    }).CreateMapper();
}