namespace Kentico.Xperience.Typesense.Collectioning;

/// <summary>
/// Typesense integration options.
/// </summary>
public sealed class TypesenseOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string CMS_TYPESENSE_SECTION_NAME = "CMSTypesenseSearch";

    /// <summary>
    /// /// Turn off functionality if application is not configured in the appsettings
    /// </summary>
    public bool IsConfigured
    {
        get;
        set;
    } = false;


    /// <summary>
    /// Typesense API key.
    /// </summary>
    public string ApiKey
    {
        get;
        set;
    } = "NO_KEY";

    public NodeOptions Node
    {
        get;
        set;
    } = new NodeOptions();


    /// <summary>
    /// The Typesense crawler API key.
    /// </summary>
    public string CrawlerApiKey
    {
        get;
        set;
    } = "";


    /// <summary>
    /// The Typesense crawler user ID.
    /// </summary>
    public string CrawlerUserId
    {
        get;
        set;
    } = "";


    /// <summary>
    /// The interval at which <see cref="TypesenseQueueWorker"/> runs, in milliseconds.
    /// </summary>
    public int CrawlerInterval
    {
        get;
        set;
    }

    public string ObjectIdParameterName
    {
        get;
        set;
    } = "object";

    public string QueryIdParameterName
    {
        get;
        set;
    } = "query";

    public string PositionParameterName
    {
        get;
        set;
    } = "pos";
}
