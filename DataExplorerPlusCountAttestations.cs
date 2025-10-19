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
                    int counter = await qr.Session.Source().GetCountAsync(Query.From("AttestationHelper")
                        .Where(String.Format(@"UID_PersonHead IN (select ac.UID_PersonHead from ATT_VAttestationDecisionPerson ac                                                           
                                                                join AttestationPolicy ap on ap.UID_AttestationPolicy = ac.UID_AttestationPolicy
                                                                Join DialogConfigParm dp on dp.Value = ap.UID_AttestationPolicy
                                                                where ac.RulerLevel = 0 and dp.FullPath like 'Custom\DataExplorerAttestations\%'
                                                                and dp.Enabled = 1 and ac.UID_PersonHead = '{0}')", qr.Session.User().Uid)).SelectCount());
                    return counter;
                }));
        }
    }
}