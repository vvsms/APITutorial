namespace APITutorial.API.Services;

internal static class CustomMediaTypeNames
{
    internal static class Application
    {
        public const string JsonV1 = "application/json;v=1";
        public const string JsonV2 = "application/json;v=2";
        public const string HateoasJson = "application/vnd.apitutorial.hateoas+json";
        public const string HateoasJsonV1 = "application/vnd.apitutorial.hateoas.1+json";
        public const string HateoasJsonV2 = "application/vnd.apitutorial.hateoas.2+json";
    }
}
