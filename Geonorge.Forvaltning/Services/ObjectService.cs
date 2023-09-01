using Geonorge.Forvaltning.Models.Api.User;
using Geonorge.Forvaltning.Models.Entity;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Geonorge.Forvaltning.Models.Api;
using Newtonsoft.Json;
using System.Linq;

namespace Geonorge.Forvaltning.Services
{
    public class ObjectService : IObjectService
    {
        private readonly ApplicationContext _context;
        private readonly IAuthService _authService;
        private readonly DbTestConfiguration _config;

        public ObjectService(ApplicationContext context, IAuthService authService, IOptions<DbTestConfiguration> config)
        {
            _context = context;
            _authService = authService;
            _config = config.Value;
        }

        public async Task<object> AddDefinition(ObjectDefinition o)
        {
            User user = await _authService.GetUser();

            if (user == null)
                throw new UnauthorizedAccessException("Brukeren har ikke tilgang");

            Models.Entity.ForvaltningsObjektMetadata metadata = new Models.Entity.ForvaltningsObjektMetadata();
            metadata.Organization = "Kartverket";
            metadata.Name = o.Name;
            metadata.ForvaltningsObjektPropertiesMetadata = new List<ForvaltningsObjektPropertiesMetadata>();
            foreach (var item in o.Properties) 
            {
                metadata.ForvaltningsObjektPropertiesMetadata.Add( new ForvaltningsObjektPropertiesMetadata { Name = item.Name, DataType = item.DataType  });
            }
            _context.ForvaltningsObjektMetadata.Add(metadata);
            _context.SaveChanges();

            //Problem create table tablename cannot be parameter. create table bensinstasjon4()  Then possible to create alter table with parameters?
            //columns named are fixed c1,c2,c3 and maby also table t1,t2... ?

            string sql = "CREATE TABLE " + metadata.Name + " (id SERIAL PRIMARY KEY,"; //Todo make table name unique to avoid conflict?
            for (int i = 0; i < o.Properties.Count; i++)
            {
                if (o.Properties[i].DataType.Contains("bool"))
                    sql = sql + " " + o.Properties[i].Name + " boolean";
                else
                    sql = sql + " "+ o.Properties[i].Name + " VARCHAR(255)";

                if (i < o.Properties.Count - 1)
                    sql = sql + " , ";

            }
            sql = sql + " ) ";
            var con = new NpgsqlConnection(
            connectionString: _config.ConnectionString);
            con.Open();
            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync();
            con.Close();

            var created = await GetMetadataObject(metadata.Id);

            return created;
        }

        public async Task<object> AddObject(int id, ObjectItem item)
        {
            var objekt = _context.ForvaltningsObjektMetadata.Where(x => x.Id == id).Include(i => i.ForvaltningsObjektPropertiesMetadata).FirstOrDefault();
            var columnsList = objekt.ForvaltningsObjektPropertiesMetadata.Select(x => x.Name).ToList();
            var columns = string.Join<string>(",", columnsList);

            var parameterList = new List<string>();
            foreach (var col in columnsList)
                parameterList.Add("@"+ col + "");

            var parameters = string.Join<string>(",", parameterList);

            var data = JsonConvert.DeserializeObject<dynamic>(item.Objekt.ToString());

            var table = objekt.Name; //todo use column for tableName

            var sql = $"INSERT INTO {table} ({columns}) VALUES ({parameters})";

            var con = new NpgsqlConnection(
            connectionString: _config.ConnectionString);
            con.Open();

            using var cmd = new NpgsqlCommand();

            foreach (var column in columnsList) 
            {
                string field = column;
                var value = data[field].ToString();
                if (value == "True")
                    value = true;
                cmd.Parameters.AddWithValue("@" + field, value);
            }
            try { 
                cmd.Connection = con;
                cmd.CommandText = sql;

                cmd.CommandText = sql;

                await cmd.ExecuteNonQueryAsync();
            }
            catch (NpgsqlException ex) 
            { 
            }



            con.Close();

            return null;

        }

        public async Task<object> GetMetadataObject(int id)
        {
            var objekt = _context.ForvaltningsObjektMetadata.Where(x => x.Id == id).Include(i => i.ForvaltningsObjektPropertiesMetadata).FirstOrDefault();
            if(objekt != null) 
            {
                var columnsList = objekt.ForvaltningsObjektPropertiesMetadata.Select(x => x.Name).ToList();
                var columns = string.Join<string>(",", columnsList);
                var con = new NpgsqlConnection(
                connectionString: _config.ConnectionString);
                con.Open();
                using var cmd = new NpgsqlCommand();
                cmd.Connection = con;

                cmd.CommandText = $"SELECT {columns} FROM {objekt.Name}";

                await using var reader = await cmd.ExecuteReaderAsync();

                //todo return header table and columns?
                List<object> items = new List<object>();

                while (await reader.ReadAsync())
                {
                    var data = new System.Dynamic.ExpandoObject() as IDictionary<string, Object>;

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var columnName = reader.GetName(i);
                        var columnDatatype = reader.GetDataTypeName(i);
                        var cellValue = reader[i];
                        data.Add(columnName, cellValue);
                    }

                    items.Add(data);
                }

                return items;
            }

            return null;
        }

        public async Task<List<Geonorge.Forvaltning.Models.Api.ForvaltningsObjektMetadata>> GetMetadataObjects()
        {
           var objectsList = _context.ForvaltningsObjektMetadata.ToList(); //todo filter organization
            List<Geonorge.Forvaltning.Models.Api.ForvaltningsObjektMetadata> objects = new List<Models.Api.ForvaltningsObjektMetadata>();
            if (objects != null) 
            {
                foreach (var objekt in objectsList)
                    objects.Add( new Models.Api.ForvaltningsObjektMetadata { Id = objekt.Id, Name = objekt.Name });

            }
            return objects;
        }
    }

    public interface IObjectService
    {
        Task<object> AddDefinition(ObjectDefinition o);
        Task<object> AddObject(int id, ObjectItem o);
        Task<List<Geonorge.Forvaltning.Models.Api.ForvaltningsObjektMetadata>> GetMetadataObjects();
        Task<object> GetMetadataObject(int id);
    }
}
