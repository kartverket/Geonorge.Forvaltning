using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Npgsql;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Geonorge.Forvaltning.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdministrationController : ControllerBase
    {
        

        private readonly ILogger<AdministrationController> _logger;
        private readonly DbConfiguration _config;

        public AdministrationController(ILogger<AdministrationController> logger, IOptions<DbConfiguration> options)
        {
            _logger = logger;
            _config = options.Value;
        }

        [HttpGet(Name = "GetAdministration")]
        public async void GetAdministration()
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
            await supabase.From<DatamodelsForOrganizations>().Insert(model);
            //todo How can i create tables from my app dynamically? https://github.com/orgs/supabase/discussions/3852
            //supabase.Rpc

            //https://michaelscodingspot.com/postgres-in-csharp/

            var con = new NpgsqlConnection(
            connectionString: connectionstring);
            con.Open();
            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;

            cmd.CommandText = $"DROP TABLE IF EXISTS teachers";
            await cmd.ExecuteNonQueryAsync();
            cmd.CommandText = "CREATE TABLE teachers (id SERIAL PRIMARY KEY," +
                "first_name VARCHAR(255)," +
                "last_name VARCHAR(255)," +
                "subject VARCHAR(255)," +
                "salary INT)";
            await cmd.ExecuteNonQueryAsync();



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