using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;

[assembly: RegisterFormComponent(
    identifier: "kentico.xperience-typesense-query-field-weights",
    componentType: typeof(Kentico.Xperience.Typesense.Admin.Components.TypesenseQueryFieldWeightsComponent),
    name: "Typesense Query Field Weights")]

namespace Kentico.Xperience.Typesense.Admin.Components;

/// <summary>
/// Form component for configuring Typesense query field weights.
/// </summary>
public class TypesenseQueryFieldWeightsComponentAttribute : FormComponentAttribute
{
}

public class TypesenseQueryFieldWeightsComponentClientProperties : FormComponentClientProperties<IEnumerable<TypesenseQueryFieldWeight>>
{
}

/// <summary>
/// TypesenseQueryFieldWeightsComponent properties.
/// </summary>
public class TypesenseQueryFieldWeightsComponentProperties : FormComponentProperties
{
}

/// <summary>
/// Form component for configuring Typesense query field weights.
/// </summary>
[ComponentAttribute(typeof(TypesenseQueryFieldWeightsComponentAttribute))]
public class TypesenseQueryFieldWeightsComponent : FormComponent<TypesenseQueryFieldWeightsComponentProperties, TypesenseQueryFieldWeightsComponentClientProperties, IEnumerable<TypesenseQueryFieldWeight>>
{
    public const string IDENTIFIER = "delaware.xperience-integrations-typesense.typesense-query-field-weights";

    internal List<TypesenseQueryFieldWeight>? Value { get; set; }

    public override string ClientComponentName => "@delaware/xperience-integrations-typesense/TypesenseQueryFieldWeights";

    public override IEnumerable<TypesenseQueryFieldWeight> GetValue() => Value ?? [];

    public override void SetValue(IEnumerable<TypesenseQueryFieldWeight> value) => Value = value.ToList();

    protected override async Task ConfigureClientProperties(TypesenseQueryFieldWeightsComponentClientProperties properties)
    {
        properties.Value = GetValue();
        await base.ConfigureClientProperties(properties);
    }
}
