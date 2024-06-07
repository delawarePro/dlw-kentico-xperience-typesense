using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

using System.Collections.Generic;

using Kentico.Xperience.Typesense.Search;

namespace Kentico.Xperience.Typesense.JsonResolvers;

public class TypeSenseTypeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);

        var baseType = typeof(TypesenseSearchResultModel);
        var derivedTypes = GetDerivedTypes<TypesenseSearchResultModel>().Select(x => new JsonDerivedType(x));
        if (jsonTypeInfo.Type == baseType)
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$discriminator",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
            };

            foreach (var derivedType in derivedTypes)
            {
                jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(derivedType);
            }
        }

        return jsonTypeInfo;
    }

    private IEnumerable<Type> GetDerivedTypes<TBase>()
    {
        var baseType = typeof(TBase);
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var derivedTypes = new List<Type>();

        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(baseType));
                derivedTypes.AddRange(types);
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Handle the exception if needed, log it, etc.
                var types = ex.Types.Where(t => t != null && t.IsClass && !t.IsAbstract && t.IsSubclassOf(baseType));
                derivedTypes.AddRange(types!);
            }
        }

        return derivedTypes;
    }
}
