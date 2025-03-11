using VI.Base;
using VI.DB;
using VI.DB.Entities;
using QBM.CompositionApi.Definition;
using System.Data.SqlClient;

namespace QBM.CompositionApi
{
    public class DataExplorerPlusRemoteSqlExecute : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>, IApiProvider
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("dataexplorerplus/remotesqlexecute")
                  .Handle<PostedSQL, List<List<ColumnData>>>("POST", async (posted, qr, ct) =>
                  {
                      var strUID_Person = qr.Session.User().Uid;
                      string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[posted.connString].ConnectionString;
                      string query = "";
                      var queryOI = Query.From("QBMLimitedSQL").Select("SQLContent").Where(string.Format("Ident_QBMLimitedSQL = '{0}'", posted.IdentQBMLimitedSQL));

                      var tryGetSQL = await qr.Session.Source().TryGetAsync(queryOI, EntityLoadType.DelayedLogic).ConfigureAwait(false);

                      query = tryGetSQL.Result.GetValue<string>("SQLContent");
                      if (query.Contains("@xkey"))
                      {
                          query = query.Replace("@xkey", string.Format("'{0}'", posted.xKey));
                      }
                      if (query.Contains("@xsubkey"))
                      {
                          query = query.Replace("@xsubkey", string.Format("'{0}'", posted.xSubKey));
                      }
                      if (query.Contains("@uidperson"))
                      {
                          query = query.Replace("@uidperson", string.Format("'{0}'", strUID_Person));
                      }

                      var results = new List<List<ColumnData>>();

                      using (SqlConnection connection = new SqlConnection(connectionString))
                      {

                          connection.Open();


                          using (SqlCommand command = new SqlCommand(query, connection))
                          {

                              using (SqlDataReader reader = command.ExecuteReader())
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
            public string connString { get; set; }
            public string IdentQBMLimitedSQL { get; set; }
            public string xKey { get; set; }
            public string xSubKey { get; set; }
        }
    }
}