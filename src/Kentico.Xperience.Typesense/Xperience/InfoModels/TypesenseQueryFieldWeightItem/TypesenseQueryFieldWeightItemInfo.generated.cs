using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseQueryFieldWeightItem;

[assembly: RegisterObjectType(typeof(TypesenseQueryFieldWeightItemInfo), TypesenseQueryFieldWeightItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseQueryFieldWeightItem;

/// <summary>
/// Data container class for <see cref="TypesenseQueryFieldWeightItemInfo"/>.
/// </summary>
[Serializable]
public partial class TypesenseQueryFieldWeightItemInfo : AbstractInfo<TypesenseQueryFieldWeightItemInfo, IInfoProvider<TypesenseQueryFieldWeightItemInfo>>, IInfoWithId, IInfoWithGuid
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticotypesense.typesensequeryfieldweightitem";

    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<TypesenseQueryFieldWeightItemInfo>), OBJECT_TYPE, "KenticoTypesense.TypesenseQueryFieldWeightItem", nameof(TypesenseQueryFieldWeightItemId), null, nameof(TypesenseQueryFieldWeightItemGuid), nameof(TypesenseQueryFieldWeightItemFieldName), null, null, null, null)
    {
        TouchCacheDependencies = true,
        ContinuousIntegrationSettings =
        {
            Enabled = true,
        },
    };

    /// <summary>
    /// Typesense query field weight item id.
    /// </summary>
    [DatabaseField]
    public virtual int TypesenseQueryFieldWeightItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(TypesenseQueryFieldWeightItemId)), 0);
        set => SetValue(nameof(TypesenseQueryFieldWeightItemId), value);
    }

    /// <summary>
    /// Typesense query field weight item Guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid TypesenseQueryFieldWeightItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(TypesenseQueryFieldWeightItemGuid)), default);
        set => SetValue(nameof(TypesenseQueryFieldWeightItemGuid), value);
    }

    /// <summary>
    /// Field name.
    /// </summary>
    [DatabaseField]
    public virtual string TypesenseQueryFieldWeightItemFieldName
    {
        get => ValidationHelper.GetString(GetValue(nameof(TypesenseQueryFieldWeightItemFieldName)), String.Empty);
        set => SetValue(nameof(TypesenseQueryFieldWeightItemFieldName), value);
    }

    /// <summary>
    /// Field weight.
    /// </summary>
    [DatabaseField]
    public virtual int TypesenseQueryFieldWeightItemWeight
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(TypesenseQueryFieldWeightItemWeight)), 1);
        set => SetValue(nameof(TypesenseQueryFieldWeightItemWeight), value);
    }

    /// <summary>
    /// Query item id.
    /// </summary>
    [DatabaseField]
    public virtual int TypesenseQueryFieldWeightItemQueryItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(TypesenseQueryFieldWeightItemQueryItemId)), 0);
        set => SetValue(nameof(TypesenseQueryFieldWeightItemQueryItemId), value);
    }

    /// <summary>
    /// Deletes the object using appropriate provider.
    /// </summary>
    protected override void DeleteObject()
    {
        Provider.Delete(this);
    }

    /// <summary>
    /// Updates the object using appropriate provider.
    /// </summary>
    protected override void SetObject()
    {
        Provider.Set(this);
    }    /// <summary>
         /// Creates an empty instance of the <see cref="TypesenseQueryFieldWeightItemInfo"/> class.
         /// </summary>
    public TypesenseQueryFieldWeightItemInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="TypesenseQueryFieldWeightItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public TypesenseQueryFieldWeightItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
