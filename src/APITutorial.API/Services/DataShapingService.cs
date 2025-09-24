using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;

namespace APITutorial.API.Services;

public sealed class DataShapingService
{
    private readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertiesCache = new();


    public ExpandoObject ShapeData<T>(T entity, string? fields)
    {
        HashSet<string> fieldSet = fields?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        PropertyInfo[] propertyInfos = PropertiesCache.GetOrAdd(typeof(T), t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        if (fieldSet.Count != 0)
        {
            propertyInfos = propertyInfos
                .Where(p => fieldSet.Contains(p.Name))
                .ToArray();
        }
         IDictionary<string,object?> shapedObject = new ExpandoObject();

        foreach (PropertyInfo propertyInfo in propertyInfos)
        {
            shapedObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
        }
        return (ExpandoObject)shapedObject;
    }


    public List<ExpandoObject> ShapeCollectionData<T>(IEnumerable<T> entities, string? fields)
    {
        HashSet<string> fieldSet = fields?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        PropertyInfo[] propertyInfos = PropertiesCache.GetOrAdd(typeof(T), t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        if (fieldSet.Count != 0)
        {
            propertyInfos = propertyInfos
                .Where(p => fieldSet.Contains(p.Name))
                .ToArray();
        }
        List<ExpandoObject> shapedObjects = [];

        foreach (var entity in entities)
        {
            IDictionary<string, object?> shapedObject = new ExpandoObject();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                shapedObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
            }

            shapedObjects.Add((ExpandoObject)shapedObject);
        }
        return shapedObjects;
    }

    public bool Validate<T>(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return true;
        }
        var fieldSet = fields.Split(',', StringSplitOptions.RemoveEmptyEntries)
           .Select(f => f.Trim())
           .ToHashSet(StringComparer.OrdinalIgnoreCase);

        PropertyInfo[] propertyInfos = PropertiesCache.GetOrAdd(
            typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        return fieldSet.All(f => propertyInfos.Any(p => p.Name.Equals(f, StringComparison.OrdinalIgnoreCase)));
    }
}
