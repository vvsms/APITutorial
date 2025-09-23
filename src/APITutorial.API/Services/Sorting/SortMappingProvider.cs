namespace APITutorial.API.Services.Sorting;

public sealed class SortMappingProvider(IEnumerable<ISortMappingDefinition> sortMappingDefinitions)
{
    public SortMapping[] GetMappings<TSource, TDestination>()
    {
        SortMappingDefinition<TSource,TDestination>? SortMappingDefinition=sortMappingDefinitions.OfType<SortMappingDefinition<TSource,TDestination>>().FirstOrDefault();

        return SortMappingDefinition is null
            ? throw new InvalidOperationException($"The mapping from '{typeof(TSource).Name}' into '{typeof(TDestination).Name}' isn't defined.")
            : SortMappingDefinition.Mappings;
    }

    public bool ValidateMappings<TSource, TDestination>(string? sort)
    {
        if(string.IsNullOrWhiteSpace(sort))
        {
            return true;
        }
        var sortFields=sort
            .Split(',')
            .Select(s=>s.Trim().Split(' ')[0])
            .Where(s=>!string.IsNullOrWhiteSpace(s))
            .ToArray();

        SortMapping[] mappings=GetMappings<TSource,TDestination>();

        return sortFields.All(field=>mappings.Any(m=>m.SortField.Equals(field,StringComparison.OrdinalIgnoreCase)));
    }
}