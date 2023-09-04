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
                throw new UnauthorizedAccessException("Manglende eller feil autorisering");

            if (!user.IsAdmin || !user.HasRole(Role.Editor))
                throw new UnauthorizedAccessException("Brukeren har ikke tilgang");

            try
            {

                Models.Entity.ForvaltningsObjektMetadata metadata = new Models.Entity.ForvaltningsObjektMetadata();
                metadata.Organization = user.OrganizationNumber;
                metadata.Name = o.Name;
                metadata.TableName = "";
                metadata.ForvaltningsObjektPropertiesMetadata = new List<ForvaltningsObjektPropertiesMetadata>();
                int col = 1;
                foreach (var item in o.Properties)
                {
                    metadata.ForvaltningsObjektPropertiesMetadata.Add(new ForvaltningsObjektPropertiesMetadata { Name = item.Name, DataType = item.DataType, ColumnName = "c_" + col });
                    col++;
                }
                _context.ForvaltningsObjektMetadata.Add(metadata);
                _context.SaveChanges();

                metadata.TableName = "t_" + metadata.Id.ToString();
                _context.SaveChanges();

                string sql = "CREATE TABLE " + metadata.TableName + " (id SERIAL PRIMARY KEY,"; //Todo use uuid data type?
                foreach(var property in metadata.ForvaltningsObjektPropertiesMetadata)
                {
                    if (property.DataType.Contains("bool"))
                        sql = sql + " " + property.ColumnName + " boolean,";
                    else
                        sql = sql + " " + property.ColumnName + " text,"; //todo support more data types numeric?

                }
                sql = sql + " geometry geometry  ";
                sql = sql + ", updatedate timestamp with time zone  ";
                sql = sql + ", editor text  ";
                sql = sql + ", owner_org numeric  ";
                sql = sql + ", contributor_org numeric  ";

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
            catch (NpgsqlException ex)
            {
            }
            return null;
        }

        public async Task<object> AddObject(int id, ObjectItem item)
        {
            User user = await _authService.GetUser();

            if (user == null)
                throw new UnauthorizedAccessException("Manglende eller feil autorisering");

            var objekt = _context.ForvaltningsObjektMetadata.Where(x => x.Id == id).Include(i => i.ForvaltningsObjektPropertiesMetadata).FirstOrDefault();

            if (!user.IsAdmin)
                if(objekt.Organization != user.OrganizationNumber)
                    throw new UnauthorizedAccessException("Brukeren har ikke tilgang");

            var properties = objekt.ForvaltningsObjektPropertiesMetadata.ToList();

            var columnsList = properties.Select(x => x.ColumnName).ToList();
            columnsList.Add("geometry");
            columnsList.Add("editor");
            columnsList.Add("owner_org"); 
            var columns = string.Join<string>(",", columnsList);

            var parameterList = new List<string>();
            foreach (var col in columnsList)
                parameterList.Add("@"+ col + "");


            var parameters = string.Join<string>(",", parameterList);

            var data = JsonConvert.DeserializeObject<dynamic>(item.objekt.ToString());

            var table = objekt.TableName;

            var sql = $"INSERT INTO {table} ({columns}, updatedate) VALUES ({parameters}, CURRENT_TIMESTAMP )";

            var con = new NpgsqlConnection(
            connectionString: _config.ConnectionString);
            con.Open();

            using var cmd = new NpgsqlCommand();

            foreach (var column in columnsList)
            {
                string field = column;
                if (column == "geometry")
                {
                    var value = data["geometry"].ToString();
                    cmd.Parameters.AddWithValue("@" + "geometry", value);
                }
                else if (column == "editor")
                {
                    var value = user.Username;
                    cmd.Parameters.AddWithValue("@" + "editor", value);
                }
                else if (column == "owner_org")
                {
                    var value = user.OrganizationNumber;
                    cmd.Parameters.AddWithValue("@" + "owner_org", int.Parse(value));
                }
                else {
                    string objectName = properties.Where(p => p.ColumnName == field).Select(s => s.Name).FirstOrDefault();
                    var value = data[objectName].ToString();
                    if (value == "True")
                        value = true;
                    else if (value == "False")
                        value = false;

                    cmd.Parameters.AddWithValue("@" + field, value);
                }       
            }
            try { 
                cmd.Connection = con;
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
                var columnsList = objekt.ForvaltningsObjektPropertiesMetadata.Select(x => x.ColumnName).ToList();
                var columns = string.Join<string>(",", columnsList);
                var con = new NpgsqlConnection(
                connectionString: _config.ConnectionString);
                con.Open();
                using var cmd = new NpgsqlCommand();
                cmd.Connection = con;

                cmd.CommandText = $"SELECT id, {columns}, ST_AsGeoJSON((geometry),15,0)::json As geometry FROM {objekt.TableName}";

                await using var reader = await cmd.ExecuteReaderAsync();

                //todo return header table and columns?
                List<object> items = new List<object>();
                try
                {
                    while (await reader.ReadAsync())
                    {
                        var data = new System.Dynamic.ExpandoObject() as IDictionary<string, Object>;

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var columnName = objekt.ForvaltningsObjektPropertiesMetadata.Where(c => c.ColumnName == reader.GetName(i)).Select(s => s.Name).FirstOrDefault();
                            var columnDatatype = reader.GetDataTypeName(i);
                            if (columnName == null) 
                            {
                                if(columnDatatype == "integer") 
                                {
                                    var idField = reader["id"].ToString();
                                    data.Add("id", Convert.ToInt16(idField));
                                }
                                else 
                                { 
                                    var geometry = reader["geometry"].ToString();
                                    if(geometry != null && geometry != "{}") {
                                        var geo = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(geometry);
                                        if(geo != null) 
                                            data.Add("geometry", geo);
                                    }
                                }
                            }
                            else { 
                                var cellValue = reader[i];
                                data.Add(columnName, cellValue);
                            }

                        }

                        items.Add(data);
                    }

                    return items;
                }
                catch (NpgsqlException ex)
                {
                }
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
