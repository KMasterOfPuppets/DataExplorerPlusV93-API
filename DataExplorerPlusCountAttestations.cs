using VI.Base;
using VI.DB;
using VI.DB.Entities;
using QBM.CompositionApi.Definition;

namespace QBM.CompositionApi
{
    public class DataExplorerPlusCountAttestations : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("dataexplorerplus/countattestations")
                .HandleGet(async (qr, ct) =>
                {
                    var strUID_Person = qr.Session.User().Uid;

                    var query1 = Query.From("AttestationCase")
                        .Select("UID_AttestationCase")
                        .Where(String.Format(@"UID_AttestationCase IN (select distinct ac.UID_AttestationCase
                                                                from AttestationCase ac                                                           
                                                                join AttestationPolicy ap on ap.UID_AttestationPolicy = ac.UID_AttestationPolicy
                                                                Join DialogConfigParm dp on dp.Value = ap.UID_AttestationPolicy
                                                                join ATT_VAttestationDecisionPerson ah on ah.UID_AttestationCase = ac.UID_AttestationCase
                                                                where 2 = 2
                                                                and ah.RulerLevel <> 2
                                                                and dp.FullPath like 'Custom\DataExplorerAttestations\%'
                                                                and dp.Enabled = 1
                                                                and ah.UID_PersonHead = '{0}')", strUID_Person));
                    var tryGet1 = await qr.Session.Source().TryGetAsync(query1, EntityLoadType.DelayedLogic).ConfigureAwait(false);

                    if (tryGet1.Success)
                    {
                        var CollGet1 = await qr.Session.Source().GetCollectionAsync(query1, EntityCollectionLoadType.Default, ct).ConfigureAwait(false);
                        List<string> responseArray = new List<string>();
                        foreach (var entity in CollGet1)
                        {
                            var AttCase = await entity.GetValueAsync<string>("UID_AttestationCase").ConfigureAwait(false);
                            responseArray.Add(AttCase);
                        }
                        return responseArray.Count;
                    }
                    else
                    {
                        return 0;
                    }
                }));
        }
    }
}