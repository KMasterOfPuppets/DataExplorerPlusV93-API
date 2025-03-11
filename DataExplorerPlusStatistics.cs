using QBM.CompositionApi.Chart;
using QBM.CompositionApi.Definition;
using QER.CompositionApi.Portal;
using QER.CompositionApi.Statistics;
using VI.Base;

namespace QBM.CompositionApi
{
    public class DataExplorerPlusStatistics : IApiProviderFor<PortalApiProject>, IApiProvider
    {
        public void Build(IApiBuilder builder)
        {
            IChartDataProvider chartDataProvider = builder.Resolver.Resolve<IChartDataProvider>();
            builder.AddMethod(Method.Define("dataexplorerplus/statistics")
                .Handle<PostedID, ChartDto>("POST", async (posted, qr, ct) =>
                {
                    IChartInfo chartInfo = new ChartInfo(posted.xStatistic)
                    {
                        Area = MapArea(posted.xArea),
                        RpsReportName = posted.xReport
                    };
                    ChartOptions options = posted.xNoHistory ? ChartOptions.WithTruncatedHistory : ChartOptions.Default;
                    TryResult<ChartDto> result = await chartDataProvider.TryGetDataAsync(qr.Session, chartInfo, options, null, ct).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(posted.xKey))
                    {
                        result.Result.Data = result.Result.Data
                            .Where(d => d.ObjectKey == posted.xKey)
                            .ToArray();
                    }
                    return result.Result;
                }));
        }
        private IChartArea MapArea(string area)
        {
            if (string.IsNullOrWhiteSpace(area))
            {
                return null;
            }
            switch (area.Trim().ToLowerInvariant())
            {
                case "identities":
                    return HeatmapConfig.AreaIdentities;
                case "organizations":
                    return HeatmapConfig.AreaOrganizations;
                case "shop":
                    return HeatmapConfig.AreaShop;
                case "risk":
                    return HeatmapConfig.AreaRisk;
                default:
                    return null;
            }
        }

        public class PostedID
        {
            public string xKey { get; set; }
            public string xStatistic { get; set; }
            public bool xNoHistory { get; set; }
            public string xArea { get; set; }
            public string xReport { get; set; }
        }
    }
}