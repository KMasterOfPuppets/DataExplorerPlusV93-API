using VI.Base;
using VI.DB;
using VI.DB.Entities;
using QBM.CompositionApi.Definition;

namespace QBM.CompositionApi
{
    public class DataExplorerPlusAttestations : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("dataexplorerplus/attestations")
                .HandleGet(async (qr, ct) =>
                {
                    var query = Query.From("DialogConfigParm").Select("Value").Where("Enabled = 1 and FullPath like 'Custom\\DataExplorerAttestations\\%'");
                    var tryGet = await qr.Session.Source().TryGetAsync(query, EntityLoadType.DelayedLogic).ConfigureAwait(false);
                    List<string> responseArray = new List<string>();
                    if (tryGet.Success)
                    {
                        var CollGet = await qr.Session.Source().GetCollectionAsync(query, EntityCollectionLoadType.Default, ct).ConfigureAwait(false);
                        foreach (var entity in CollGet)
                        {
                            var value = await entity.GetValueAsync<string>("Value").ConfigureAwait(false);
                            responseArray.Add(value);
                        }                  
                    }
                    return responseArray;
                }));
        }
    }
}