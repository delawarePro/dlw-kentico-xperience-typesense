using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseContentTypeItem;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIncludedPathItem;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexItem;

[assembly: RegisterObjectType(typeof(TypesenseContentTypeItemInfo), TypesenseContentTypeItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseContentTypeItem;

/// <summary>
/// Data container class for <see cref="TypesenseContentTypeItemInfo"/>.
/// </summary>
[Serializable]
public partial class TypesenseContentTypeItemInfo : AbstractInfo<TypesenseContentTypeItemInfo, ITypesenseContentTypeItemInfoProvider>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticotypesense.typesensecontenttypeitem";

    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(TypesenseContentTypeItemInfoProvider), OBJECT_TYPE, "KenticoTypesense.TypesenseContentTypeItem", nameof(TypesenseContentTypeItemId), null, nameof(TypesenseContentTypeItemGuid), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        DependsOn = new List<ObjectDependency>()
        {
            new(nameof(TypesenseContentTypeItemIncludedPathItemId), TypesenseIncludedPathItemInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
            new(nameof(TypesenseContentTypeItemCollectionItemId), TypesenseIndexItemInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
        },
        ContinuousIntegrationSettings =
        {
            Enabled = true
        }
    };

    /// <summary>
    /// Typesense content type item id.
    /// </summary>
    [DatabaseField]
    public virtual int TypesenseContentTypeItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(TypesenseContentTypeItemId)), 0);
        set => SetValue(nameof(TypesenseContentTypeItemId), value);
    }

    /// <summary>
    /// Typesense content type item guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid TypesenseContentTypeItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(TypesenseContentTypeItemGuid)), default);
        set => SetValue(nameof(TypesenseContentTypeItemGuid), value);
    }

    /// <summary>
    /// Content type name.
    /// </summary>
    [DatabaseField]
    public virtual string TypesenseContentTypeItemContentTypeName
    {
        get => ValidationHelper.GetString(GetValue(nameof(TypesenseContentTypeItemContentTypeName)), String.Empty);
        set => SetValue(nameof(TypesenseContentTypeItemContentTypeName), value);
    }

    /// <summary>
    /// Typesense included path item id.
    /// </summary>
    [DatabaseField]
    public virtual int TypesenseContentTypeItemIncludedPathItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(TypesenseContentTypeItemIncludedPathItemId)), 0);
        set => SetValue(nameof(TypesenseContentTypeItemIncludedPathItemId), value);
    }

    /// <summary>
    /// Typesense index item id.
    /// </summary>
    [DatabaseField]
    public virtual int TypesenseContentTypeItemCollectionItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(TypesenseContentTypeItemCollectionItemId)), 0);
        set => SetValue(nameof(TypesenseContentTypeItemCollectionItemId), value);
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
    protected TypesenseContentTypeItemInfo(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    /// <summary>
    /// Creates an empty instance of the <see cref="TypesenseContentTypeItemInfo"/> class.
    /// </summary>
    public TypesenseContentTypeItemInfo()
        : base(TYPEINFO)
    {
    }

    /// <summary>
    /// Creates a new instances of the <see cref="TypesenseContentTypeItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public TypesenseContentTypeItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
