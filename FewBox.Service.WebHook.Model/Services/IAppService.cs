using FewBox.Service.WebHook.Model.Dtos;

namespace FewBox.Service.WebHook.Model.Services
{
    public interface IAppService
    {
        HealthyDto GetHealtyInfo();
    }
}