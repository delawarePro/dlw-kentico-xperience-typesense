using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Xperience.Typesense.Admin;

[assembly: RegisterObjectType(typeof(IndexQueueItemInfo), IndexQueueItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.Typesense.Admin;

/// <summary>
/// Data container class for <see cref="IndexQueueItemInfo"/>.
/// </summary>
[Serializable]
public partial class IndexQueueItemInfo : AbstractInfo<IndexQueueItemInfo, IInfoProvider<IndexQueueItemInfo>>, IInfoWithId, IInfoWithGuid
{
    public const string OBJECT_TYPE = "kenticotypesense.typesenseindexqueueitem";

    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<IndexQueueItemInfo>), OBJECT_TYPE, "KenticoTypesense.TypesenseIndexQueueItem", nameof(IndexQueueItemID), null, nameof(IndexQueueItemID), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        DependsOn = new List<ObjectDependency>()
        {
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
    public virtual int IndexQueueItemID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(IndexQueueItemID)), 0);
        set => SetValue(nameof(IndexQueueItemID), value);
    }


    /// <summary>
    /// Collectioned language id.
    /// </summary>
    [DatabaseField]
    public virtual Guid IndexQueueItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(IndexQueueItemGuid)), default);
        set => SetValue(nameof(IndexQueueItemGuid), value);
    }

    /// <summary>
    /// Task Type
    /// </summary>
    [DatabaseField]
    public virtual int TaskType
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(TaskType)), 0);
        set => SetValue(nameof(TaskType), value);
    }

    [DatabaseField]
    public virtual string CollectionName
    {
        get => ValidationHelper.GetString(GetValue(nameof(CollectionName)), String.Empty);
        set => SetValue(nameof(CollectionName), value);
    }

    [DatabaseField]
    public virtual string CollectionEvent
    {
        get => ValidationHelper.GetString(GetValue(nameof(CollectionEvent)), String.Empty);
        set => SetValue(nameof(CollectionEvent), value);
    }

    /// <summary>
    /// Crm attachment created at.
    /// </summary>
    [DatabaseField]
    public virtual DateTime EnqueuedAt
    {
        get => ValidationHelper.GetDateTime(GetValue(nameof(EnqueuedAt)), DateTimeHelper.ZERO_TIME);
        set => SetValue(nameof(EnqueuedAt), value);
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
    /// Creates an empty instance of the <see cref="IndexQueueItemInfo"/> class.
    /// </summary>
    public IndexQueueItemInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="IndexQueueItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public IndexQueueItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
