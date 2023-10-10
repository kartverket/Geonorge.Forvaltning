﻿using Geonorge.Forvaltning.Models.Api.User;
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
        private readonly DbConfiguration _config;

        public ObjectService(ApplicationContext context, IAuthService authService, IOptions<DbConfiguration> config)
        {
            _context = context;
            _authService = authService;
            _config = config.Value;
        }

        public async Task<DataObject> AddDefinition(ObjectDefinitionAdd o)
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
                    metadata.ForvaltningsObjektPropertiesMetadata.Add(new ForvaltningsObjektPropertiesMetadata { Name = item.Name, DataType = item.DataType, ColumnName = "c_" + col, OrganizationNumber = user.OrganizationNumber });
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
                        sql = sql + " " + property.ColumnName + " text,"; //todo support more data types numeric, date?

                }
                sql = sql + " geometry geometry  ";
                sql = sql + ", updatedate timestamp with time zone  ";
                sql = sql + ", editor text  ";
                sql = sql + ", owner_org text  ";
                sql = sql + ", contributor_org text[]  ";

                sql = sql + " ); ";

                sql = sql + "alter table "+ metadata.TableName + " enable row level security;";
                sql = sql + "CREATE POLICY \"Owner\" ON \"public\".\"" + metadata.TableName + "\" AS PERMISSIVE FOR ALL TO public USING ((EXISTS ( SELECT * FROM users WHERE (users.organization = " + metadata.TableName + ".owner_org)))) WITH CHECK ((EXISTS ( SELECT * FROM users WHERE (users.organization = " + metadata.TableName + ".owner_org))));";
                sql = sql + "CREATE POLICY \"Contributor\" ON \"public\".\"" + metadata.TableName + "\" AS PERMISSIVE FOR ALL TO public USING ((EXISTS ( SELECT * FROM users WHERE (users.organization = ANY(" + metadata.TableName + ".contributor_org))))) WITH CHECK ((EXISTS ( SELECT * FROM users WHERE (users.organization = ANY(" + metadata.TableName + ".contributor_org)))));";
                var con = new NpgsqlConnection(
                connectionString: _config.ForvaltningApiDatabase);
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

        public async Task<DataObject> AddObject(int id, ObjectItem item)
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

            var sql = $"INSERT INTO {table} ({columns}, updatedate) VALUES ({parameters}, CURRENT_TIMESTAMP );SELECT CAST(lastval() AS integer)";

            var con = new NpgsqlConnection(
            connectionString: _config.ForvaltningApiDatabase);
            con.Open();

            using var cmd = new NpgsqlCommand();

            foreach (var column in columnsList)
            {
                string field = column;
                if (column == "geometry")
                {
                    var value = data["geometry"].ToString();
                    if (value == "")
                        value = DBNull.Value;
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

                int newID = (int)cmd.ExecuteScalar();
                var newObject = await GetMetadataObject(id);

                con.Close();

                return newObject;
            }
            catch (NpgsqlException ex) 
            { 
            }

            return null;

        }

        public async Task<DataObject> GetMetadataObject(int id)
        {
            DataObject dataObject = new DataObject();
            var objekt = _context.ForvaltningsObjektMetadata.Where(x => x.Id == id).Include(i => i.ForvaltningsObjektPropertiesMetadata).FirstOrDefault();
            if(objekt != null) 
            {
                var columnsList = objekt.ForvaltningsObjektPropertiesMetadata.Select(x => x.ColumnName).ToList();
                var columns = string.Join<string>(",", columnsList);
                var con = new NpgsqlConnection(
                connectionString: _config.ForvaltningApiDatabase);
                con.Open();
                using var cmd = new NpgsqlCommand();
                cmd.Connection = con;

                cmd.CommandText = $"SELECT id, {columns}, ST_AsGeoJSON((geometry),15,0)::json As geometry FROM {objekt.TableName}";

                await using var reader = await cmd.ExecuteReaderAsync();

                ObjectDefinition objectDefinition = new ObjectDefinition();

                objectDefinition.Properties = new List<ObjectDefinitionProperty>();

                objectDefinition.Properties.Add(new ObjectDefinitionProperty { Name = "id", DataType = "SERIAL PRIMARY KEY" });

                objectDefinition.Id = objekt.Id;
                objectDefinition.Name = objekt.Name;
                objectDefinition.TableName = objekt.TableName;
                foreach (var property in objekt.ForvaltningsObjektPropertiesMetadata)
                    objectDefinition.Properties.Add(new ObjectDefinitionProperty { Name = property.Name, DataType = property.DataType, ColumnName = property.ColumnName });

                objectDefinition.Properties.Add(new ObjectDefinitionProperty { Name = "geometry", DataType = "geometry" } );

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
                                        var geo = geometry;// JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(geometry);
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

                    dataObject.definition = objectDefinition;
                    dataObject.objekt = items;

                    return dataObject;
                }
                catch (NpgsqlException ex)
                {
                }
            }

            return null;
        }

        public async Task<List<Geonorge.Forvaltning.Models.Api.ForvaltningsObjektMetadata>> GetMetadataObjects()
        {

            User user = await _authService.GetUserSupabase();

            var objectsList = _context.ForvaltningsObjektMetadata.Where(o => o.Organization == user.OrganizationNumber).ToList();
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
        Task<DataObject> AddDefinition(ObjectDefinitionAdd o);
        Task<DataObject> AddObject(int id, ObjectItem o);
        Task<List<Geonorge.Forvaltning.Models.Api.ForvaltningsObjektMetadata>> GetMetadataObjects();
        Task<DataObject> GetMetadataObject(int id);
    }
}
