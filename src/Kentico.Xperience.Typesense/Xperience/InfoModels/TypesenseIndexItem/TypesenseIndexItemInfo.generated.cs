using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexItem;

[assembly: RegisterObjectType(typeof(TypesenseIndexItemInfo), TypesenseIndexItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexItem;

/// <summary>
/// Data container class for <see cref="TypesenseIndexItemInfo"/>.
/// </summary>
[Serializable]
public partial class TypesenseIndexItemInfo : AbstractInfo<TypesenseIndexItemInfo, ITypesenseCollectionItemInfoProvider>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticotypesense.typesenseindexitem";

    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(TypesenseCollectionItemInfoProvider), OBJECT_TYPE, "KenticoTypesense.TypesenseIndexItem", nameof(TypesenseCollectionItemId), null, nameof(TypesenseCollectionItemGuid), nameof(TypesenseCollectionItemcollectionName), null, null, null, null)
    {
        TouchCacheDependencies = true,
        ContinuousIntegrationSettings =
        {
            Enabled = true,
        },
    };

    /// <summary>
    /// Typesense index item id.
    /// </summary>
    [DatabaseField]
    public virtual int TypesenseCollectionItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(TypesenseCollectionItemId)), 0);
        set => SetValue(nameof(TypesenseCollectionItemId), value);
    }

    /// <summary>
    /// Typesense index item Guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid TypesenseCollectionItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(TypesenseCollectionItemGuid)), default);
        set => SetValue(nameof(TypesenseCollectionItemGuid), value);
    }

    /// <summary>
    /// Collection name.
    /// </summary>
    [DatabaseField]
    public virtual string TypesenseCollectionItemcollectionName
    {
        get => ValidationHelper.GetString(GetValue(nameof(TypesenseCollectionItemcollectionName)), String.Empty);
        set => SetValue(nameof(TypesenseCollectionItemcollectionName), value);
    }

    /// <summary>
    /// Channel name.
    /// </summary>
    [DatabaseField]
    public virtual string TypesenseCollectionItemChannelName
    {
        get => ValidationHelper.GetString(GetValue(nameof(TypesenseCollectionItemChannelName)), String.Empty);
        set => SetValue(nameof(TypesenseCollectionItemChannelName), value);
    }

    /// <summary>
    /// Strategy name.
    /// </summary>
    [DatabaseField]
    public virtual string TypesenseCollectionItemStrategyName
    {
        get => ValidationHelper.GetString(GetValue(nameof(TypesenseCollectionItemStrategyName)), String.Empty);
        set => SetValue(nameof(TypesenseCollectionItemStrategyName), value);
    }

    /// <summary>
    /// Rebuild hook.
    /// </summary>
    [DatabaseField]
    public virtual string TypesenseCollectionItemRebuildHook
    {
        get => ValidationHelper.GetString(GetValue(nameof(TypesenseCollectionItemRebuildHook)), String.Empty);
        set => SetValue(nameof(TypesenseCollectionItemRebuildHook), value, String.Empty);
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
    }

    /// <summary>
    /// Constructor for de-serialization.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    protected TypesenseIndexItemInfo(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    /// <summary>
    /// Creates an empty instance of the <see cref="TypesenseIndexItemInfo"/> class.
    /// </summary>
    public TypesenseIndexItemInfo()
        : base(TYPEINFO)
    {
    }

    /// <summary>
    /// Creates a new instances of the <see cref="TypesenseIndexItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public TypesenseIndexItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
