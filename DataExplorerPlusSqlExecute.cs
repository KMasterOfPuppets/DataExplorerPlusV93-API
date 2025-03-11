using VI.DB.Entities;
using QBM.CompositionApi.Definition;
using VI.DB.DataAccess;
using VI.DB.Sync;

namespace QBM.CompositionApi
{
    public class DataExplorerPlusSqlExecute : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("dataexplorerplus/sqlexecute")
                  .Handle<PostedSQL, List<List<ColumnData>>>("POST", async (posted, qr, ct) =>
                  {

                      var strUID_Person = qr.Session.User().Uid;
                      var results = new List<List<ColumnData>>();
                      var runner = qr.Session.Resolve<IStatementRunner>();
                      using (var reader = runner.SqlExecute(posted.IdentQBMLimitedSQL, new[]
                      {
                            QueryParameter.Create("uidperson", strUID_Person),
                            QueryParameter.Create("xkey", posted.xKey),
                            QueryParameter.Create("xsubkey", posted.xSubKey)
                        }))
                      {
                          while (reader.Read())
                          {
                              var row = new List<ColumnData>();
                              for (int i = 0; i < reader.FieldCount; i++)
                              {
                                  row.Add(new ColumnData
                                  {
                                      Column = reader.GetName(i),
                                      Value = reader.IsDBNull(i) ? null : reader.GetValue(i).ToString()
                                  });
                              }
                              results.Add(row);
                          }
                      }
                      return results;
                  }));
        }
        public class ColumnData
        {
            public string Column { get; set; }
            public string Value { get; set; }
        }

        public class PostedSQL
        {
            public string IdentQBMLimitedSQL { get; set; }
            public string xKey { get; set; }
            public string xSubKey { get; set; }
        }
    }
}