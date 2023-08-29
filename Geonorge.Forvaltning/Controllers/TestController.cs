using Geonorge.Forvaltning.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Npgsql;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Geonorge.Forvaltning.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        

        private readonly ILogger<TestController> _logger;
        private readonly DbTestConfiguration _config;
        private readonly ApplicationContext _context;

        public TestController(ILogger<TestController> logger, IOptions<DbTestConfiguration> options, ApplicationContext context)
        {
            _logger = logger;
            _config = options.Value;
            _context = context;
        }

        [HttpGet(Name = "GetTest")]
        public async Task<object> GetTest()
        {

            var model = new DatamodelsForOrganizations
            {
                organization = "Nærings- og fiskeridepartementet",
                datatype_name = "Bensinstasjoner"
            };

            var url = _config.SUPABASE_URL;
            var key = _config.SUPABASE_KEY;
            var connectionstring = _config.ConnectionString;

            var options = new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = true
            };

            var supabase = new Supabase.Client(url, key, options);
            //await supabase.From<DatamodelsForOrganizations>().Insert(model);
            //todo How can i create tables from my app dynamically? https://github.com/orgs/supabase/discussions/3852
            //supabase.Rpc

            //https://michaelscodingspot.com/postgres-in-csharp/

            var con = new NpgsqlConnection(
            connectionString: connectionstring);
            con.Open();
            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;

            cmd.CommandText = $"DROP TABLE IF EXISTS bensinstasjoner";
            //await cmd.ExecuteNonQueryAsync();
            cmd.CommandText = "CREATE TABLE bensinstasjoner (id SERIAL PRIMARY KEY," +
                "navn VARCHAR(255)," +
                "merke VARCHAR(255), bensin boolean )";
            //await cmd.ExecuteNonQueryAsync();
            con.Close();

            con = new NpgsqlConnection(
            connectionString: connectionstring);
            con.Open();
            using var cmd2 = new NpgsqlCommand();
            cmd2.Connection = con;

            cmd2.CommandText = $"SELECT navn,merke,bensin FROM bensinstasjoner";

            await using var reader = await cmd2.ExecuteReaderAsync();

            List<object> items = new List<object>();

            while (await reader.ReadAsync())
            {            
                var data= new System.Dynamic.ExpandoObject() as IDictionary<string, Object>;

                for (int i = 0; i < reader.FieldCount; i++) {
                    var columnName = reader.GetName(i);
                    var columnDatatype = reader.GetDataTypeName(i);
                    var cellValue = reader[i]; 
                    data.Add(columnName, cellValue);    
                }

                items.Add(data);
            }

            return items;

        }
    }

    [Table("DatamodelsForOrganizations")]
    class DatamodelsForOrganizations : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("organization")]
        public string organization { get; set; }

        [Column("datatype_name")]
        public string datatype_name { get; set; }
    }

}