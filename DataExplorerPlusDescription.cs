using QBM.CompositionApi.Definition;
using VI.Base;
using VI.DB;
using VI.DB.Entities;

namespace QBM.CompositionApi
{
    public class DataExplorerPlusDescription : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("dataexplorerplus/description")
                .Handle<PostedID, string>("POST", async (posted, qr, ct) =>
                {
                    try
                    {
                        string htmlBanner = "";
                        if (!string.IsNullOrWhiteSpace(posted.description))
                        {
                            var query1 = Query.From("DialogRichMailBody").Select("RichMailBody").Where(String.Format(@"UID_DialogCulture in (select UID_DialogCulture from QBMCulture where Ident_DialogCulture = '{0}')
                                                                                                                       and UID_DialogRichMail in (select UID_DialogRichMail from DialogRichMail 
                                                                                                                       where Ident_DialogRichMail = '{1}')", posted.culture, posted.description));
                            
                            var queryD = Query.From("DialogRichMailBody").Select("RichMailBody").Where(String.Format(@"UID_DialogCulture in (select UID_DialogCultureDefault from QBMCulture where Ident_DialogCulture = '{0}')
                                                                                                                       and UID_DialogRichMail in (select UID_DialogRichMail from DialogRichMail 
                                                                                                                       where Ident_DialogRichMail = '{1}')", posted.culture, posted.description));

                            var tryGetD = await qr.Session.Source().TryGetAsync(queryD, EntityLoadType.DelayedLogic).ConfigureAwait(false);

                            var tryGetText = await qr.Session.Source().TryGetAsync(query1, EntityLoadType.DelayedLogic).ConfigureAwait(false);

                            if (tryGetText.Success)
                            {
                                htmlBanner = await tryGetText.Result.GetValueAsync<string>("RichMailBody", ct).ConfigureAwait(false);
                            }
                            else if (tryGetD.Success)
                            {
                                htmlBanner = await tryGetD.Result.GetValueAsync<string>("RichMailBody", ct).ConfigureAwait(false);
                            }
                            else
                            {
                                var query2 = Query.From("DialogRichMailBody").Select("RichMailBody").Where(String.Format(@"UID_DialogCulture in (select UID_DialogCulture from QBMCulture where Ident_DialogCulture = 'en-US')
                                                                                                                           and UID_DialogRichMail in (select UID_DialogRichMail from DialogRichMail 
                                                                                                                           where Ident_DialogRichMail = '{0}')", posted.description));

                                var tryGetText2 = await qr.Session.Source().TryGetAsync(query2, EntityLoadType.DelayedLogic).ConfigureAwait(false);
                                if (tryGetText2.Success)
                                {
                                    htmlBanner = await tryGetText2.Result.GetValueAsync<string>("RichMailBody", ct).ConfigureAwait(false);
                                }
                            }
                        }
                        return htmlBanner;
                    }
                    catch (Exception)
                    {
                        return string.Empty;
                    }
                }));
        }

        public class PostedID
        {
            public string description { get; set; }
            public string culture { get; set; }
        }

    }
}
