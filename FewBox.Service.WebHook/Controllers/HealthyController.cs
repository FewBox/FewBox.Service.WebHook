using FewBox.Service.WebHook.Model.Dtos;
using FewBox.Service.WebHook.Model.Services;
using FewBox.Core.Web.Dto;
using Microsoft.AspNetCore.Mvc;

namespace FewBox.Service.WebHook.Controllers
{
    [Route("api/[controller]")]
    public class HealthyController : ControllerBase
    {
        private IAppService AppService { get; set; }

        public HealthyController(IAppService AppService)
        {
            this.AppService = AppService;
        }

        [HttpGet]
        public PayloadResponseDto<HealthyDto> Get()
        {
            return new PayloadResponseDto<HealthyDto>
            {
                Payload = this.AppService.GetHealtyInfo()
            };
        }
    }
}
