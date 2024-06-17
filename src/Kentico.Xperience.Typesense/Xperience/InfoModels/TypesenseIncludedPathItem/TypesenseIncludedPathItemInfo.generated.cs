using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIncludedPathItem;

[assembly: RegisterObjectType(typeof(TypesenseIncludedPathItemInfo), TypesenseIncludedPathItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.Typesense.Admin;

/// <summary>
/// Data container class for <see cref="TypesenseIncludedPathItemInfo"/>.
/// </summary>
[Serializable]
public partial class TypesenseIncludedPathItemInfo : AbstractInfo<TypesenseIncludedPathItemInfo, ITypesenseIncludedPathItemInfoProvider>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticotypesense.typesenseincludedpathitem";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(TypesenseIncludedPathItemInfoProvider), OBJECT_TYPE, "KenticoTypesense.TypesenseIncludedPathItem", nameof(TypesenseIncludedPathItemId), null, nameof(TypesenseIncludedPathItemGuid), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        DependsOn = new List<ObjectDependency>()
        {
            new(nameof(TypesenseIncludedPathItemCollectionItemId), TypesenseCollectionItemInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
        },
        ContinuousIntegrationSettings =
        {
            Enabled = true
        }
    };


    /// <summary>
    /// Typesense included path item id.
    /// </summary>
    [DatabaseField]
    public virtual int TypesenseIncludedPathItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(TypesenseIncludedPathItemId)), 0);
        set => SetValue(nameof(TypesenseIncludedPathItemId), value);
    }

    /// <summary>
    /// Typesense included path item guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid TypesenseIncludedPathItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(TypesenseIncludedPathItemGuid)), default);
        set => SetValue(nameof(TypesenseIncludedPathItemGuid), value);
    }


    /// <summary>
    /// Alias path.
    /// </summary>
    [DatabaseField]
    public virtual string TypesenseIncludedPathItemAliasPath
    {
        get => ValidationHelper.GetString(GetValue(nameof(TypesenseIncludedPathItemAliasPath)), String.Empty);
        set => SetValue(nameof(TypesenseIncludedPathItemAliasPath), value);
    }


    /// <summary>
    /// Typesense index item id.
    /// </summary>
    [DatabaseField]
    public virtual int TypesenseIncludedPathItemCollectionItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(TypesenseIncludedPathItemCollectionItemId)), 0);
        set => SetValue(nameof(TypesenseIncludedPathItemCollectionItemId), value);
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
    protected TypesenseIncludedPathItemInfo(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="TypesenseIncludedPathItemInfo"/> class.
    /// </summary>
    public TypesenseIncludedPathItemInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="TypesenseIncludedPathItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public TypesenseIncludedPathItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
