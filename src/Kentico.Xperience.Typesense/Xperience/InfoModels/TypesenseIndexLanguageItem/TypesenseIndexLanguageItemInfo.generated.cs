using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexItem;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexLanguageItem;

[assembly: RegisterObjectType(typeof(TypesenseIndexLanguageItemInfo), TypesenseIndexLanguageItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexLanguageItem;

/// <summary>
/// Data container class for <see cref="TypesenseIndexLanguageItemInfo"/>.
/// </summary>
[Serializable]
public partial class TypesenseIndexLanguageItemInfo : AbstractInfo<TypesenseIndexLanguageItemInfo, ITypesenseCollectionLanguageItemInfoProvider>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticotypesense.typesenseindexlanguageitem";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(TypesenseCollectionedLanguageInfoProvider), OBJECT_TYPE, "KenticoTypesense.TypesenseIndexLanguageItem", nameof(TypesenseCollectionLanguageItemID), null, nameof(TypesenseCollectionLanguageItemGuid), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        DependsOn = new List<ObjectDependency>()
        {
            new(nameof(TypesenseCollectionLanguageItemCollectionItemId), TypesenseIndexItemInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
        },
        ContinuousIntegrationSettings =
        {
            Enabled = true
        }
    };


    /// <summary>
    /// Collectioned language id.
    /// </summary>
    [DatabaseField]
    public virtual int TypesenseCollectionLanguageItemID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(TypesenseCollectionLanguageItemID)), 0);
        set => SetValue(nameof(TypesenseCollectionLanguageItemID), value);
    }


    /// <summary>
    /// Collectioned language id.
    /// </summary>
    [DatabaseField]
    public virtual Guid TypesenseCollectionLanguageItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(TypesenseCollectionLanguageItemGuid)), default);
        set => SetValue(nameof(TypesenseCollectionLanguageItemGuid), value);
    }


    /// <summary>
    /// Code.
    /// </summary>
    [DatabaseField]
    public virtual string TypesenseCollectionLanguageItemName
    {
        get => ValidationHelper.GetString(GetValue(nameof(TypesenseCollectionLanguageItemName)), String.Empty);
        set => SetValue(nameof(TypesenseCollectionLanguageItemName), value);
    }


    /// <summary>
    /// Typesense index item id.
    /// </summary>
    [DatabaseField]
    public virtual int TypesenseCollectionLanguageItemCollectionItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(TypesenseCollectionLanguageItemCollectionItemId)), 0);
        set => SetValue(nameof(TypesenseCollectionLanguageItemCollectionItemId), value);
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
    protected TypesenseIndexLanguageItemInfo(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="TypesenseIndexLanguageItemInfo"/> class.
    /// </summary>
    public TypesenseIndexLanguageItemInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="TypesenseIndexLanguageItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public TypesenseIndexLanguageItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
