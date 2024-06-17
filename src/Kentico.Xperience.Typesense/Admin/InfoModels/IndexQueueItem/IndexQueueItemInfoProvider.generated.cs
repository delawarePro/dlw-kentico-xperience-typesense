using CMS.DataEngine;

namespace Kentico.Xperience.Typesense.Admin;

/// <summary>
/// Class providing <see cref="IndexQueueItemInfo"/> management.
/// </summary>
[ProviderInterface(typeof(IIndexQueueItemInfoProvider))]
public partial class IndexQueueItemProvider : AbstractInfoProvider<IndexQueueItemInfo, IndexQueueItemProvider>, IIndexQueueItemInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IndexQueueItemProvider"/> class.
    /// </summary>
    public IndexQueueItemProvider()
        : base(IndexQueueItemInfo.TYPEINFO)
    {
    }
}
