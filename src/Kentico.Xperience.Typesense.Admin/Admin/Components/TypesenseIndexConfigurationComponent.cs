using CMS.DataEngine;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Typesense.Admin;

[assembly: RegisterFormComponent(
    identifier: TypesenseCollectionConfigurationComponent.IDENTIFIER,
    componentType: typeof(TypesenseCollectionConfigurationComponent),
    name: "Typesense Search Collection Configuration")]

namespace Kentico.Xperience.Typesense.Admin;

#pragma warning disable S2094 // intentionally empty class
public class TypesenseCollectionConfigurationComponentProperties : FormComponentProperties
{
}
#pragma warning restore

public class TypesenseCollectionConfigurationComponentClientProperties : FormComponentClientProperties<IEnumerable<TypesenseCollectionIncludedPath>>
{
    public IEnumerable<TypesenseCollectionContentType>? PossibleContentTypeItems { get; set; }
}

public sealed class TypesenseCollectionConfigurationComponentAttribute : FormComponentAttribute
{
}

[ComponentAttribute(typeof(TypesenseCollectionConfigurationComponentAttribute))]
public class TypesenseCollectionConfigurationComponent : FormComponent<TypesenseCollectionConfigurationComponentProperties, TypesenseCollectionConfigurationComponentClientProperties, IEnumerable<TypesenseCollectionIncludedPath>>
{
    public const string IDENTIFIER = "delaware.xperience-integrations-typesense.typesense-index-configuration";

    internal List<TypesenseCollectionIncludedPath>? Value { get; set; }

    public override string ClientComponentName => "@delaware/xperience-integrations-typesense/TypesenseCollectionConfiguration";

    public override IEnumerable<TypesenseCollectionIncludedPath> GetValue() => Value ?? [];
    public override void SetValue(IEnumerable<TypesenseCollectionIncludedPath> value) => Value = value.ToList();

    [FormComponentCommand]
    public Task<ICommandResponse<RowActionResult>> DeletePath(string path)
    {
        var toRemove = Value?.Find(x => Equals(x.AliasPath == path, StringComparison.OrdinalIgnoreCase));
        if (toRemove != null)
        {
            Value?.Remove(toRemove);
            return Task.FromResult(ResponseFrom(new RowActionResult(false)));
        }
        return Task.FromResult(ResponseFrom(new RowActionResult(false)));
    }

    [FormComponentCommand]
    public Task<ICommandResponse<RowActionResult>> SavePath(TypesenseCollectionIncludedPath path)
    {
        var value = Value?.SingleOrDefault(x => Equals(x.AliasPath == path.AliasPath, StringComparison.OrdinalIgnoreCase));

        if (value is not null)
        {
            Value?.Remove(value);
        }

        Value?.Add(path);

        return Task.FromResult(ResponseFrom(new RowActionResult(false)));
    }

    [FormComponentCommand]
    public Task<ICommandResponse<RowActionResult>> AddPath(string path)
    {
        if (Value?.Exists(x => x.AliasPath == path) ?? false)
        {
            return Task.FromResult(ResponseFrom(new RowActionResult(false)));
        }
        else
        {
            Value?.Add(new TypesenseCollectionIncludedPath(path));
            return Task.FromResult(ResponseFrom(new RowActionResult(false)));
        }
    }

    protected override async Task ConfigureClientProperties(TypesenseCollectionConfigurationComponentClientProperties properties)
    {
        var allWebsiteContentTypes = DataClassInfoProvider.ProviderObject
           .Get()
           .WhereEquals(nameof(DataClassInfo.ClassContentTypeType), "Website")
           .GetEnumerableTypedResult()
           .Select(x => new TypesenseCollectionContentType(x.ClassName, x.ClassDisplayName));

        properties.Value = Value ?? [];
        properties.PossibleContentTypeItems = allWebsiteContentTypes.ToList();

        await base.ConfigureClientProperties(properties);
    }
}
