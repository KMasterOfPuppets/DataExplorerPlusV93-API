using VI.DB.Entities;
using QBM.CompositionApi.Definition;
using QER.CompositionApi.Portal;
using VI.DB.DataAccess;
using VI.DB.Sync;

namespace QBM.CompositionApi
{
    public class DataExplorerPlusOrgChart : IApiProviderFor<PortalApiProject>
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("dataexplorerplus/orgchart")
                .Handle<PostedSQL, List<OrgChartNode>>("POST", async (posted, qr, ct) =>
                {
                    var strUID_Person = qr.Session.User().Uid;
                    var runner = qr.Session.Resolve<IStatementRunner>();
                    var sql = posted.IdentQBMLimitedSQL;
                    var paramList = new List<QueryParameter>
                    {
                        QueryParameter.Create("uidperson", strUID_Person),
                        QueryParameter.Create("xkey", posted.xKey),
                        QueryParameter.Create("xsubkey", posted.xSubKey)
                    };

                    var flatList = new List<OrgChartNode>();
                    using (var reader = runner.SqlExecute(sql, paramList.ToArray()))
                    {
                        while (reader.Read())
                        {
                            flatList.Add(new OrgChartNode
                            {
                                xUID = reader["xUID"]?.ToString(),
                                xParentUID = reader["xParentUID"]?.ToString(),
                                xDisplay = reader["xDisplay"]?.ToString(),
                                xDetail = reader["xDetail"]?.ToString(),
                                xActions = reader["xActions"]?.ToString(),
                                xKey = reader["xKey"]?.ToString(),
                                xSubKey = reader["xSubKey"]?.ToString(),
                                xPhoto = reader["xPhoto"] as byte[]
                            });
                        }
                    }

                    var tree = BuildHierarchy(flatList);
                    return tree;
                })
            );
        }
        private List<OrgChartNode> BuildHierarchy(List<OrgChartNode> flatList)
        {
            var lookup = new Dictionary<string, OrgChartNode>(StringComparer.OrdinalIgnoreCase);
            foreach (var node in flatList)
            {
                lookup[node.xUID] = node;
            }

            var roots = new List<OrgChartNode>();
            foreach (var node in flatList)
            {
                if (!string.IsNullOrEmpty(node.xParentUID) && lookup.ContainsKey(node.xParentUID))
                {
                    lookup[node.xParentUID].Children.Add(node);
                }
                else
                {
                    roots.Add(node);
                }
            }
            return roots;
        }

        public class PostedSQL
        {
            public string IdentQBMLimitedSQL { get; set; }
            public string xKey { get; set; }
            public string xSubKey { get; set; }
        }

        public class OrgChartNode
        {
            public string xUID { get; set; }
            public string xParentUID { get; set; }
            public string xDisplay { get; set; }
            public string xDetail { get; set; }
            public byte[] xPhoto { get; set; }
            public string xActions { get; set; }
            public string xKey { get; set; }
            public string xSubKey { get; set; }
            public List<OrgChartNode> Children { get; set; } = new List<OrgChartNode>();
        }
    }
}
