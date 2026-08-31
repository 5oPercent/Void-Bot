using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Void.Bot.Core.Resources.i18n;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Api.Extension;

public static class ScannerExtensions
{
    public static List<EndpointDto> ScanEndpoints()
    {
        var endpoints = new List<EndpointDto>();
        var controllers = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type));

        foreach (var controller in controllers)
        {
            if (controller.GetCustomAttribute<SkipScanAttribute>() != null)
                continue;
            
            var controllerName = controller.Name.Replace("Controller", "");

            var controllerRoutes = controller.GetCustomAttributes<RouteAttribute>()
                .Select(attr => attr.Template)
                .DefaultIfEmpty("api/[controller]");

            var methods = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<SkipScanAttribute>() != null ||
                    method.GetCustomAttribute<SkipPermissionCheckAttribute>() != null)
                    continue;
                
                var httpAttrs = method.GetCustomAttributes()
                    .Where(attr => attr is HttpMethodAttribute)
                    .Cast<HttpMethodAttribute>();
                
                if (!httpAttrs.Any()) continue;
                
                var routeAttrs = method.GetCustomAttributes<RouteAttribute>()
                    .Select(attr => attr.Template)
                    .ToList();

                foreach (var httpAttr in httpAttrs)
                {
                    var httpMethod = httpAttr.HttpMethods.FirstOrDefault() ?? "GET";
                    var methodTemplates = new List<string>();

                    // Http*Attribute 通常包含 Template（如 HttpGet("test")）
                    if (!string.IsNullOrEmpty(httpAttr.Template))
                        methodTemplates.Add(httpAttr.Template);

                    // 补充 RouteAttribute 的 Template
                    methodTemplates.AddRange(routeAttrs);

                    if (methodTemplates.Count == 0)
                        methodTemplates.Add(string.Empty); // 无方法路径

                    foreach (var controllerRoute in controllerRoutes)
                    {
                        foreach (var methodTemplate in methodTemplates)
                        {
                            var fullPath = CombineRoutes(controllerRoute, methodTemplate, controllerName);

                            var descAttr = method.GetCustomAttribute<DescriptionAttribute>();
                            var keyAttr = method.GetCustomAttribute<DescriptionLocalizationAttribute>();
                            var descriptionKey = keyAttr?.Key ?? GetLocalizationKey(method.Name);
                            var description = descAttr?.Description ?? GetDefaultDescription(controllerName, descriptionKey);

                            endpoints.Add(new EndpointDto
                            {
                                Controller = controllerName,
                                Endpoint = "/" + fullPath.Trim('/'),
                                Method = httpMethod,
                                DescriptionKey = descriptionKey,
                                Description = description
                            });
                        }
                    }
                }
            }
        }

        return endpoints;
    }
    
    private static string CombineRoutes(string controllerRoute, string methodRoute, string controllerName)
    {
        string baseRoute = controllerRoute.Replace("[controller]", controllerName);

        if (string.IsNullOrWhiteSpace(methodRoute))
            return baseRoute;

        if (methodRoute.StartsWith("~"))
            return methodRoute.TrimStart('~', '/');

        return $"{baseRoute}/{methodRoute}".Replace("//", "/");
    }

    private static string GetDefaultDescription(string category, string key)
    {
        var templateType = typeof(IContextTemplate).Assembly.GetType(
            $"Void.Bot.Core.Resources.i18n.Endpoint.{category}EndpointContextTemplate");
        var property = templateType?.GetProperty(key);
        var template = templateType == null ? null : Activator.CreateInstance(templateType);
        return property?.GetValue(template)?.ToString() ?? $"{category}接口：{key}";
    }

    private static string GetLocalizationKey(string methodName)
    {
        return methodName.EndsWith("Async", StringComparison.Ordinal)
            ? methodName[..^"Async".Length]
            : methodName;
    }
}
