using FewBox.Service.WebHook.Model.Configs;
using FewBox.Service.WebHook.Model.Dtos;
using FewBox.Service.WebHook.Model.Services;

namespace FewBox.Service.WebHook.Domain
{
    public class AppService : IAppService
    {
        private HealthyConfig HealthyConfig { get; set; }
        public AppService(HealthyConfig healthyConfig)
        {
            this.HealthyConfig = healthyConfig;
        }

        public HealthyDto GetHealtyInfo()
        {
            return new HealthyDto{
                Version = this.HealthyConfig.Version
            };
        }
    }
}