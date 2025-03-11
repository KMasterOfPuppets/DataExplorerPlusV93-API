using VI.Base;
using VI.DB;
using VI.DB.Entities;
using QBM.CompositionApi.Definition;

namespace QBM.CompositionApi
{
    public class DataExplorerPlusConfiguration : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("dataexplorerplus/configparms")
                .HandleGet(async (qr, ct) =>
                {
                    var strUID_Person = qr.Session.User().Uid;
                    var response = new List<object>();
                    var query1 = Query.From("DialogConfigParm")
                                    .Select("ConfigParm")
                                    .Where(String.Format(@"UID_ConfigParm IN (select c.UID_ConfigParm
                                                                        from DialogConfigParm d
																		left join DialogConfigParm c on c.UID_ConfigParm = d.UID_ParentConfigparm
																		join AERole a on d.Value = a.FullPath
																	    join PersonInAERole pia on a.UID_AERole = pia.UID_AERole
                                                                        where d.FullPath like 'Custom\WebPortalPlus\DataExplorerPlus%'
                                                                        and d.FullPath <> 'Custom\WebPortalPlus\DataExplorerPlus'
                                                                        and d.Enabled = '1'
                                                                        and c.Enabled = '1'
                                                                        and pia.UID_Person = '{0}'
                                                                        )", strUID_Person));

                    var tryGet1 = await qr.Session.Source().TryGetAsync(query1, EntityLoadType.DelayedLogic).ConfigureAwait(false);

                    if (tryGet1.Success)
                    {
                        var entities1 = await qr.Session.Source().GetCollectionAsync(query1, EntityCollectionLoadType.Default, ct).ConfigureAwait(false);

                        var allTrees = new List<ConfigParamNode>();

                        foreach (var entity in entities1)
                        {
                            var configParm = await entity.GetValueAsync<string>("ConfigParm").ConfigureAwait(false);


                            var query2 = Query.From("DialogConfigParm")
                                                .Select("UID_ConfigParm", "UID_ParentConfigparm", "ConfigParm", "Value", "Enabled")
                                                .Where($@"FullPath LIKE 'Custom\WebPortalPlus\DataExplorerPlus\{configParm}%'");

                            var entities2 = await qr.Session.Source().GetCollectionAsync(query2, EntityCollectionLoadType.Default, ct).ConfigureAwait(false);
                            var tree = await BuildTree(entities2).ConfigureAwait(false);

                            allTrees.AddRange(tree);
                        }

                        var prunedTrees = PruneDisabledNodes(allTrees);

                        response = prunedTrees.Select(node => node.ToResponse()).ToList();
                        
                    }
                    return response;
                }));
        }

        private async Task<List<ConfigParamNode>> BuildTree(IEntityCollection entities)
        {
            var nodesByUID = new Dictionary<string, ConfigParamNode>();
            var rootNodes = new List<ConfigParamNode>();

            foreach (var entity in entities)
            {
                var uid = await entity.GetValueAsync<string>("UID_ConfigParm").ConfigureAwait(false);
                var parentUid = await entity.GetValueAsync<string>("UID_ParentConfigparm").ConfigureAwait(false);
                var configParm = await entity.GetValueAsync<string>("ConfigParm").ConfigureAwait(false);
                var value = await entity.GetValueAsync<string>("Value").ConfigureAwait(false);
                var enabled = await entity.GetValueAsync<string>("Enabled").ConfigureAwait(false);

                if (!nodesByUID.ContainsKey(uid))
                {
                    nodesByUID[uid] = new ConfigParamNode
                    {
                        ConfigParm = configParm,
                        Value = value,
                        Enabled = enabled,
                        Children = new List<ConfigParamNode>()
                    };
                }

                if (!string.IsNullOrEmpty(parentUid) && nodesByUID.ContainsKey(parentUid))
                {
                    nodesByUID[parentUid].Children.Add(nodesByUID[uid]);
                }
                else
                {
                    rootNodes.Add(nodesByUID[uid]);
                }
            }

            return rootNodes;
        }

        private List<ConfigParamNode> PruneDisabledNodes(List<ConfigParamNode> nodes)
        {
            var prunedNodes = new List<ConfigParamNode>();

            foreach (var node in nodes)
            {

                if (node.Enabled.Equals("True", StringComparison.OrdinalIgnoreCase))
                {

                    node.Children = PruneDisabledNodes(node.Children);
                    prunedNodes.Add(node);
                }
            }

            return prunedNodes;
        }
    }

    public class ConfigParamNode
    {
        public string ConfigParm { get; set; }
        public string Value { get; set; }
        public string Enabled { get; set; }
        public List<ConfigParamNode> Children { get; set; } = new List<ConfigParamNode>();

        public object ToResponse()
        {
            return new
            {
                ConfigParm = this.ConfigParm,
                Value = this.Value,
                Children = this.Children.Select(child => child.ToResponse()).ToList()
            };
        }
    }
}