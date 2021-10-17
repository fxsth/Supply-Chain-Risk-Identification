using Microsoft.Extensions.Configuration;

namespace SCRI.Configuration
{
    public static class SettingsConfigurationExtensions
    {
        public static GraphSettings GetGraphSettings(this IConfiguration configuration)
        {
            return configuration.GetSection("GraphSettings").Get<GraphSettings>();
        }
        
        public static MLSettings GetMLSettings(this IConfiguration configuration)
        {
            return configuration.GetSection("MLSettings").Get<MLSettings>();
        }
        
    }
}