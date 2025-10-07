using APITutorial.API.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace APITutorial.API.Services;

public sealed class LinkService(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
{
    public LinkDto Create(string endpointName,
        string rel,
        string method,
        object? values = null,
        string? controllerName = null)
    {
        string? href = linkGenerator.GetUriByAction(
           httpContextAccessor.HttpContext!,
           endpointName,
           controllerName,
           values);

        return new()
        {
            Href = href ?? throw new InvalidOperationException("The provided endpoint name is invalid"),
            Rel = rel,
            Method = method,
        };
    }
}
