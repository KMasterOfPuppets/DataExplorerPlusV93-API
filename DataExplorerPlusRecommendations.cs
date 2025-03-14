using QBM.CompositionApi.Definition;
using VI.DB.Entities;
using VI.Base;

namespace QBM.CompositionApi
{
    public class DataExplorerPlusRecommendations : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("dataexplorerplus/recommendations")
                .Handle<PostedID, List<Dictionary<string, object>>>("POST", async (posted, qr, ct) =>
                {
                    var CCC_RecKey1 = "";
                    var CCC_RecKey2 = "";
                    var CCC_RecKey3 = "";
                    List<Dictionary<string, object>> recItems = new List<Dictionary<string, object>>();

                    foreach (var column in posted.columns)
                    {
                        if (column.column == "xCCC_RecKey1")
                        {
                            CCC_RecKey1 = column.value;
                        }
                        else if (column.column == "xCCC_RecKey2")
                        {
                            CCC_RecKey2 = column.value;
                        }
                        else if (column.column == "xCCC_RecKey3")
                        {
                            CCC_RecKey3 = column.value;
                        }
                    }
                    var conditions = new List<string>();
                    if (!string.IsNullOrEmpty(CCC_RecKey1))
                    {
                        conditions.Add($"CCC_RecKey1 = '{CCC_RecKey1}'");
                    }
                    if (!string.IsNullOrEmpty(CCC_RecKey2))
                    {
                        conditions.Add($"CCC_RecKey2 = '{CCC_RecKey2}'");
                    }
                    if (!string.IsNullOrEmpty(CCC_RecKey3))
                    {
                        conditions.Add($"CCC_RecKey3 = '{CCC_RecKey3}'");
                    }
                    if (conditions.Count > 0)
                    {
                        var queryCondition = string.Join(" and ", conditions);
                        var query1 = Query.From("CCC_DE_Recommendations")
                                          .Select("CCC_Display", "CCC_Detail", "CCC_Weight")
                                          .Where(queryCondition);

                        var tryGet1 = await qr.Session.Source().TryGetAsync(query1, EntityLoadType.DelayedLogic).ConfigureAwait(false);

                        if (tryGet1.Success)
                        {
                            var CollGet1 = await qr.Session.Source().GetCollectionAsync(query1, EntityCollectionLoadType.Default, ct).ConfigureAwait(false);
                            foreach (var entity in CollGet1)
                            {
                                var Display = await entity.GetValueAsync<string>("CCC_Display").ConfigureAwait(false);
                                var Detail = await entity.GetValueAsync<string>("CCC_Detail").ConfigureAwait(false);
                                var Weight = await entity.GetValueAsync<int>("CCC_Weight").ConfigureAwait(false);

                                var innerObj = new Dictionary<string, object>
                                {
                                    { "Display", Display },
                                    { "Detail", Detail },
                                    { "Weight", Weight }
                                };

                                recItems.Add(innerObj);
                            }
                        }
                    }

                    return recItems;
                }));
        }

        public class PostedID
        {
            public columnsarray[] columns { get; set; }
        }

        public class columnsarray
        {
            public string column { get; set; }
            public string value { get; set; }
        }
    }
}