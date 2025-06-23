using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseQueryItem;

[assembly: RegisterObjectType(typeof(TypesenseQueryItemInfo), TypesenseQueryItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseQueryItem;

/// <summary>
/// Data container class for <see cref="TypesenseQueryItemInfo"/>.
/// </summary>
[Serializable]
public partial class TypesenseQueryItemInfo : AbstractInfo<TypesenseQueryItemInfo, IInfoProvider<TypesenseQueryItemInfo>>, IInfoWithId, IInfoWithGuid
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticotypesense.typesenseueryitem";

    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<TypesenseQueryItemInfo>), OBJECT_TYPE, "KenticoTypesense.TypesenseQueryItem", nameof(TypesenseQueryItemId), null, nameof(TypesenseQueryItemGuid), nameof(TypesenseQueryItemCollectionAlias), null, null, null, null)
    {
        TouchCacheDependencies = true,
        ContinuousIntegrationSettings =
        {
            Enabled = true,
        },
    };

    /// <summary>
    /// Typesense query item id.
    /// </summary>
    [DatabaseField]
    public virtual int TypesenseQueryItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(TypesenseQueryItemId)), 0);
        set => SetValue(nameof(TypesenseQueryItemId), value);
    }

    /// <summary>
    /// Typesense query item Guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid TypesenseQueryItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(TypesenseQueryItemGuid)), default);
        set => SetValue(nameof(TypesenseQueryItemGuid), value);
    }

    /// <summary>
    /// Collection alias.
    /// </summary>
    [DatabaseField]
    public virtual string TypesenseQueryItemCollectionAlias
    {
        get => ValidationHelper.GetString(GetValue(nameof(TypesenseQueryItemCollectionAlias)), String.Empty);
        set => SetValue(nameof(TypesenseQueryItemCollectionAlias), value);
    }

    /// <summary>
    /// Deletes the object using appropriate provider.
    /// </summary>
    protected override void DeleteObject()
    {
        Provider.Delete(this);
    }    /// <summary>
         /// Updates the object using appropriate provider.
         /// </summary>
    protected override void SetObject()
    {
        Provider.Set(this);
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="TypesenseQueryItemInfo"/> class.
    /// </summary>
    public TypesenseQueryItemInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="TypesenseQueryItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public TypesenseQueryItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
