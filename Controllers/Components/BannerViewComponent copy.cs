using Microsoft.AspNetCore.Mvc;

namespace NETCORE.Controllers.Components;
public class BannerViewComponent : ViewComponent
{
    public Task<IViewComponentResult> InvokeAsync()
    {
        return Task.FromResult((IViewComponentResult)View("Default"));
    }
 

}