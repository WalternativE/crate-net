using System.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Crate.Client {

    public static class SqlClient
    {
        public static async Task<SqlResponse> Execute(string sqlUri, SqlRequest request) {
            using (var client = new WebClient()) {
                var data = JsonConvert.SerializeObject(request);
                var resp = await client.UploadStringTaskAsync(sqlUri, data);
                return JsonConvert.DeserializeObject<SqlResponse>(resp);
            }
        }
    }
}
