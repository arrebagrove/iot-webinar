using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Microsoft.Azure.Documents.Client;

namespace Web
{
    public class MapController : ApiController
    {
        [Route("api/map/{id}")]
        public object Get(int id)
        {
            var uri = new Uri(ConfigurationManager.AppSettings["docdbUri"]);
            var key = ConfigurationManager.AppSettings["docdbKey"];
            var dbName = ConfigurationManager.AppSettings["docdbDatabaseName"];
            var collName = ConfigurationManager.AppSettings["docdbCollName"];

            var client = new DocumentClient(uri, key);

            var query = client.CreateDocumentQuery<BusInfo>(UriFactory.CreateDocumentCollectionUri(dbName, collName),
                    new FeedOptions { MaxItemCount = 1 })
                .Where(bi => bi.VehicleId == id);

            var result = query.AsEnumerable().FirstOrDefault();

            return result != null
                ? new
                {
                    tripid = result.TripId,
                    route = result.RouteShortName,
                    vehicleid = result.VehicleId,
                    timeliness = result.Timeliness.ToString()
                }
                : null;
        }
    }
}
