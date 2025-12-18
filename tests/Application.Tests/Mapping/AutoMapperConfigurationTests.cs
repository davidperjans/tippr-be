using AutoMapper;

namespace Application.Tests.Mapping
{
    public sealed class AutoMapperConfigurationTests
    {
        [Fact]
        public void AutoMapper_Configuration_IsValid()
        {
            var config = new MapperConfiguration(cfg =>
            {
                // Byt ut AssemblyReference mot valfri typ som ligger i Application-assemblyn
                cfg.AddMaps(typeof(Application.DependencyInjection).Assembly);
            });

            config.AssertConfigurationIsValid();
        }
    }
}
