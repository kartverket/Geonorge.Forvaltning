using Geonorge.Forvaltning.Models.Api.User;
using Geonorge.Forvaltning.Models.Entity;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Geonorge.Forvaltning.Models.Api;
using Newtonsoft.Json;
using System.Linq;
using MimeKit;
using MailKit.Net.Smtp;

namespace Geonorge.Forvaltning.Services
{
    public class ObjectService : IObjectService
    {
        private readonly ApplicationContext _context;
        private readonly IAuthService _authService;
        private readonly DbConfiguration _config;
        private readonly EmailConfiguration _configEmail;
        private readonly ILogger<ObjectService> _logger;

        public ObjectService(ApplicationContext context, IAuthService authService, IOptions<DbConfiguration> config, IOptions<EmailConfiguration> configEmail, ILogger<ObjectService> logger)
        {
            _context = context;
            _authService = authService;
            _config = config.Value;
            _configEmail = configEmail.Value;
            _logger = logger;
        }

        public async Task<DataObject> AddDefinition(ObjectDefinitionAdd o)
        {
            User user = await _authService.GetUserSupabase();

            if (user == null)
                throw new UnauthorizedAccessException("Manglende eller feil autorisering");

            if (string.IsNullOrEmpty(user.OrganizationNumber))
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
                    metadata.ForvaltningsObjektPropertiesMetadata.Add(new ForvaltningsObjektPropertiesMetadata { Name = item.Name, DataType = item.DataType, ColumnName = "c_" + col, OrganizationNumber = user.OrganizationNumber, AllowedValues = item.AllowedValues });
                    col++;
                }
                _context.ForvaltningsObjektMetadata.Add(metadata);
                _context.SaveChanges();

                metadata.TableName = "t_" + metadata.Id.ToString();
                _context.SaveChanges();

                string sqlConstraints = "";

                string sql = "CREATE TABLE " + metadata.TableName + " (id SERIAL PRIMARY KEY,"; //Todo use uuid data type?
                foreach(var property in metadata.ForvaltningsObjektPropertiesMetadata)
                {
                    if (property.DataType.Contains("bool"))
                        sql = sql + " " + property.ColumnName + " boolean,";
                    else if (property.DataType.Contains("numeric"))
                        sql = sql + " " + property.ColumnName + " numeric,";
                    else if (property.DataType.Contains("timestamp"))
                        sql = sql + " " + property.ColumnName + " timestamp with time zone,";
                    else
                        sql = sql + " " + property.ColumnName + " text,";


                    if (property.AllowedValues != null && property.AllowedValues.Any()) 
                    { 
                        sqlConstraints = sqlConstraints + "ALTER TABLE " + metadata.TableName + " ADD CONSTRAINT allowed_" + metadata.TableName + "_" + property.ColumnName;
                        sqlConstraints = sqlConstraints + " CHECK("+ property.ColumnName + " = ANY('{"+ string.Join(",", property.AllowedValues) + "}'::text[]));";
                    }
                }
                sql = sql + " geometry geometry  ";
                sql = sql + ", updatedate timestamp with time zone  ";
                sql = sql + ", editor text  ";
                sql = sql + ", owner_org text  ";
                sql = sql + ", contributor_org text[]  ";

                sql = sql + " ); ";

                sql = sql + sqlConstraints;

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

            var created =  new DataObject { definition = new ObjectDefinition { Id = metadata.Id } };  //await GetMetadataObject(metadata.Id);
            

            return created;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError("Database error", ex);
            }
            catch (Exception e)
            {
                _logger.LogError("Error", e);
            }
            return null;
        }

        public async Task<DataObject?> EditDefinition(int id, ObjectDefinitionEdit objekt)
        {
            User user = await _authService.GetUserSupabase();

            if (user == null)
                throw new UnauthorizedAccessException("Manglende eller feil autorisering");

            if (string.IsNullOrEmpty(user.OrganizationNumber))
                throw new UnauthorizedAccessException("Brukeren har ikke tilgang");

            try
            {
                var current = _context.ForvaltningsObjektMetadata.Where(x => x.Id == id && x.Organization == user.OrganizationNumber).Include(i => i.ForvaltningsObjektPropertiesMetadata).FirstOrDefault();
                if (current == null) 
                {
                    throw new UnauthorizedAccessException("Bruker har ikke tilgang til objekt");
                }

                if(current.Name != objekt.Name) { 
                    current.Name = objekt.Name;
                    _context.SaveChanges();
                }

                var currentProperties = current.ForvaltningsObjektPropertiesMetadata.Select(y => y.Id).ToList();
                var changedProperties = objekt.Properties.Where(z => z.Id > 0).Select(n => n.Id).ToList();

                var itemsToRemove = currentProperties.Where(x => !changedProperties.Any(z => z.Value == x)).ToList();

                foreach(var itemToRemove in itemsToRemove) 
                {
                    //Get columnName
                    var columnName = current.ForvaltningsObjektPropertiesMetadata.Where(c => c.Id == itemToRemove).Select(co => co.ColumnName).FirstOrDefault();
                    //DROP COLUMN 
                    var sql = "ALTER TABLE "+ current.TableName + " DROP COLUMN "+ columnName + ";";
                    var con = new NpgsqlConnection(
                    connectionString: _config.ForvaltningApiDatabase);
                    con.Open();
                    using var cmd = new NpgsqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = sql;
                    await cmd.ExecuteNonQueryAsync();
                    con.Close();
                    //delete row from ForvaltningsObjektPropertiesMetadata
                    var itemToDelete = current.ForvaltningsObjektPropertiesMetadata.Where(c => c.Id == itemToRemove).Single();
                    _context.ForvaltningsObjektPropertiesMetadata.Remove(itemToDelete);
                    _context.SaveChanges();
                }

                foreach(var item in objekt.Properties) 
                {

                    var sqlDataType = "text";
                    if (item.DataType.Contains("bool"))
                        sqlDataType = "boolean";
                    else if (item.DataType.Contains("numeric"))
                        sqlDataType = "numeric";
                    else if (item.DataType.Contains("timestamp"))
                        sqlDataType = "timestamp with time zone";

                    if (item.Id == null || item.Id == 0) 
                    {

                        //Get last column number
                        var sqlC = "select CAST(replace(\"ColumnName\",'c_','') as int) as num from \"ForvaltningsObjektPropertiesMetadata\" where \"ForvaltningsObjektMetadataId\" = $1 order by CAST(replace(\"ColumnName\", 'c_', '') as int) desc";
                        var conC = new NpgsqlConnection(
                        connectionString: _config.ForvaltningApiDatabase);
                        conC.Open();
                        using var cmdC = new NpgsqlCommand();
                        cmdC.Connection = conC;
                        cmdC.CommandText = sqlC;
                        cmdC.Parameters.AddWithValue(current.Id);

                        var reader = await cmdC.ExecuteReaderAsync();

                        await reader.ReadAsync();

                        var lastColumn = reader.GetInt32(0);

                        conC.Close();


                        var columnName = "c_" + (lastColumn + 1);
                           
                        var sql = "ALTER TABLE " + current.TableName + " ADD COLUMN " + columnName + " " + sqlDataType + ";";

                        var sqlConstraints = "";

                        if (item.AllowedValues != null && item.AllowedValues.Any())
                        {
                            sqlConstraints = sqlConstraints + "ALTER TABLE " + current.TableName + " ADD CONSTRAINT allowed_" + current.TableName + "_" + columnName;
                            sqlConstraints = sqlConstraints + " CHECK(" + columnName + " = ANY('{" + string.Join(",", item.AllowedValues) + "}'::text[]));";
                        }

                        sql = sql + sqlConstraints;

                        var con = new NpgsqlConnection(
                        connectionString: _config.ForvaltningApiDatabase);
                        con.Open();
                        using var cmd = new NpgsqlCommand();
                        cmd.Connection = con;
                        cmd.CommandText = sql;
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();

                        //add columnName to metadata
                        current.ForvaltningsObjektPropertiesMetadata.Add(new ForvaltningsObjektPropertiesMetadata 
                        {
                            Name = item.Name,
                            OrganizationNumber = user.OrganizationNumber,
                            DataType = item.DataType,
                            ColumnName = columnName,
                            AllowedValues = item.AllowedValues
                        });

                        _context.SaveChanges();
                    }
                    else 
                    {
                        //https://www.postgresql.org/docs/current/sql-altertable.html

                        //Datatype has changed?
                        var dataType = current.ForvaltningsObjektPropertiesMetadata.Where(c => c.Id == item.Id).Select(co => co.DataType).FirstOrDefault();
                        var columnName = current.ForvaltningsObjektPropertiesMetadata.Where(c => c.Id == item.Id).Select(co => co.ColumnName).FirstOrDefault();
                        if (dataType != item.DataType) 
                        {
                            var sql = "ALTER TABLE " + current.TableName + " ALTER COLUMN " + columnName + " SET DATA TYPE "+ sqlDataType + ";";
                            var con = new NpgsqlConnection(
                            connectionString: _config.ForvaltningApiDatabase);
                            con.Open();
                            using var cmd = new NpgsqlCommand();
                            cmd.Connection = con;
                            cmd.CommandText = sql;
                            await cmd.ExecuteNonQueryAsync();
                            con.Close();

                            var attributes = current.ForvaltningsObjektPropertiesMetadata.Where(c => c.Id == item.Id).FirstOrDefault();
          
                            attributes.DataType = item.DataType;
                            _context.SaveChanges();

                        }
                        //Property name has changed?
                        var property = current.ForvaltningsObjektPropertiesMetadata.Where(c => c.Id == item.Id).FirstOrDefault();
                        if (item.Name != property.Name)
                        {
                            property.Name = item.Name;
                            _context.SaveChanges();
                        }
                        //Property AllowedValues has changed?

                        bool allowedValuesChanged = false;
                        var oldAllowedValues = property.AllowedValues;
                        var newAllowedValues = item.AllowedValues;
                        var difference = oldAllowedValues?.Except(newAllowedValues != null ? newAllowedValues : new List<string>());
                        if (difference != null && difference.Any())
                            allowedValuesChanged = true;
                        else if(oldAllowedValues != null && newAllowedValues != null)
                            if(oldAllowedValues.Count != newAllowedValues.Count)
                                allowedValuesChanged = true;

                        if (allowedValuesChanged)
                        {
                            property.AllowedValues = item.AllowedValues;
                            _context.SaveChanges();

                            //Change constraint
                            var sqlConstraints = "";

                            sqlConstraints = sqlConstraints + "ALTER TABLE " + current.TableName + " DROP CONSTRAINT allowed_" + current.TableName + "_" + columnName;
                            if(item.AllowedValues != null) {
                                sqlConstraints = sqlConstraints + ", ADD CONSTRAINT allowed_" + current.TableName + "_" + columnName;
                                sqlConstraints = sqlConstraints + " CHECK(" + columnName + " = ANY('{" + string.Join(",", item.AllowedValues) + "}'::text[]));";
                            }
                            var con = new NpgsqlConnection(
                            connectionString: _config.ForvaltningApiDatabase);
                            con.Open();
                            using var cmd = new NpgsqlCommand();
                            cmd.Connection = con;
                            cmd.CommandText = sqlConstraints;
                            await cmd.ExecuteNonQueryAsync();
                            con.Close();

                        }


                    }
                }


            }
            catch (NpgsqlException ex)
            {
                _logger.LogError("Database error", ex);
            }
            catch (Exception e)
            {
                _logger.LogError("Error", e);
            }

            return null;

        }

        public async Task<object?> DeleteObject(int id)
        {

            User user = await _authService.GetUserSupabase();

            if (user == null)
                throw new UnauthorizedAccessException("Manglende eller feil autorisering");

            if (string.IsNullOrEmpty(user.OrganizationNumber))
                throw new UnauthorizedAccessException("Brukeren har ikke tilgang");

            try
            {
                var objekt = _context.ForvaltningsObjektMetadata.Where(x => x.Id == id && x.Organization == user.OrganizationNumber).Include(i => i.ForvaltningsObjektPropertiesMetadata).Single();
                if (objekt == null)
                {
                    throw new UnauthorizedAccessException("Bruker har ikke tilgang til objekt");
                }

                //DROP TABLE 
                var sql = "DROP TABLE " + objekt.TableName + ";";
                var con = new NpgsqlConnection(
                connectionString: _config.ForvaltningApiDatabase);
                con.Open();
                using var cmd = new NpgsqlCommand();
                cmd.Connection = con;
                cmd.CommandText = sql;
                await cmd.ExecuteNonQueryAsync();
                con.Close();

                //Remove metadata table rows
                _context.ForvaltningsObjektMetadata.Remove(objekt);
                _context.SaveChanges();
  
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError("Database error", ex);
            }
            catch (Exception e)
            {
                _logger.LogError("Error", e);
            }

            return null;

        }

        //public async Task<DataObject> AddObject(int id, ObjectItem item)
        //{
        //    User user = await _authService.GetUser();

        //    if (user == null)
        //        throw new UnauthorizedAccessException("Manglende eller feil autorisering");

        //    var objekt = _context.ForvaltningsObjektMetadata.Where(x => x.Id == id).Include(i => i.ForvaltningsObjektPropertiesMetadata).FirstOrDefault();

        //    if (!user.IsAdmin)
        //        if(objekt.Organization != user.OrganizationNumber)
        //            throw new UnauthorizedAccessException("Brukeren har ikke tilgang");

        //    var properties = objekt.ForvaltningsObjektPropertiesMetadata.ToList();

        //    var columnsList = properties.Select(x => x.ColumnName).ToList();
        //    columnsList.Add("geometry");
        //    columnsList.Add("editor");
        //    columnsList.Add("owner_org"); 
        //    var columns = string.Join<string>(",", columnsList);

        //    var parameterList = new List<string>();
        //    foreach (var col in columnsList)
        //        parameterList.Add("@"+ col + "");


        //    var parameters = string.Join<string>(",", parameterList);

        //    var data = JsonConvert.DeserializeObject<dynamic>(item.objekt.ToString());

        //    var table = objekt.TableName;

        //    var sql = $"INSERT INTO {table} ({columns}, updatedate) VALUES ({parameters}, CURRENT_TIMESTAMP );SELECT CAST(lastval() AS integer)";

        //    var con = new NpgsqlConnection(
        //    connectionString: _config.ForvaltningApiDatabase);
        //    con.Open();

        //    using var cmd = new NpgsqlCommand();

        //    foreach (var column in columnsList)
        //    {
        //        string field = column;
        //        if (column == "geometry")
        //        {
        //            var value = data["geometry"].ToString();
        //            if (value == "")
        //                value = DBNull.Value;
        //            cmd.Parameters.AddWithValue("@" + "geometry", value);
        //        }
        //        else if (column == "editor")
        //        {
        //            var value = user.Username;
        //            cmd.Parameters.AddWithValue("@" + "editor", value);
        //        }
        //        else if (column == "owner_org")
        //        {
        //            var value = user.OrganizationNumber;
        //            cmd.Parameters.AddWithValue("@" + "owner_org", int.Parse(value));
        //        }
        //        else {
        //            string objectName = properties.Where(p => p.ColumnName == field).Select(s => s.Name).FirstOrDefault();
        //            string objectDataType = properties.Where(p => p.ColumnName == field).Select(s => s.DataType).FirstOrDefault();
        //            var value = data[objectName].ToString();
        //            if (value == "True")
        //                value = true;
        //            else if (value == "False")
        //                value = false;
        //            if(objectDataType == "numeric")
        //                value =  Double.Parse(value);

        //            cmd.Parameters.AddWithValue("@" + field, value);
        //        }       
        //    }
        //    try { 
        //        cmd.Connection = con;
        //        cmd.CommandText = sql;

        //        int newID = (int)cmd.ExecuteScalar();
        //        var newObject = await GetMetadataObject(id);

        //        con.Close();

        //        return newObject;
        //    }
        //    catch (NpgsqlException ex) 
        //    { 
        //    }

        //    return null;

        //}

        //public async Task<DataObject> GetMetadataObject(int id)
        //{
        //    DataObject dataObject = new DataObject();
        //    var objekt = _context.ForvaltningsObjektMetadata.Where(x => x.Id == id).Include(i => i.ForvaltningsObjektPropertiesMetadata).FirstOrDefault();
        //    if(objekt != null) 
        //    {
        //        var columnsList = objekt.ForvaltningsObjektPropertiesMetadata.Select(x => x.ColumnName).ToList();
        //        var columns = string.Join<string>(",", columnsList);
        //        var con = new NpgsqlConnection(
        //        connectionString: _config.ForvaltningApiDatabase);
        //        con.Open();
        //        using var cmd = new NpgsqlCommand();
        //        cmd.Connection = con;

        //        cmd.CommandText = $"SELECT id, {columns}, ST_AsGeoJSON((geometry),15,0)::json As geometry FROM {objekt.TableName}";

        //        await using var reader = await cmd.ExecuteReaderAsync();

        //        ObjectDefinition objectDefinition = new ObjectDefinition();

        //        objectDefinition.Properties = new List<ObjectDefinitionProperty>();

        //        objectDefinition.Properties.Add(new ObjectDefinitionProperty { Name = "id", DataType = "SERIAL PRIMARY KEY" });

        //        objectDefinition.Id = objekt.Id;
        //        objectDefinition.Name = objekt.Name;
        //        objectDefinition.TableName = objekt.TableName;
        //        foreach (var property in objekt.ForvaltningsObjektPropertiesMetadata)
        //            objectDefinition.Properties.Add(new ObjectDefinitionProperty { Name = property.Name, DataType = property.DataType, ColumnName = property.ColumnName });

        //        objectDefinition.Properties.Add(new ObjectDefinitionProperty { Name = "geometry", DataType = "geometry" } );

        //        List<object> items = new List<object>();
        //        try
        //        {
        //            while (await reader.ReadAsync())
        //            {
        //                var data = new System.Dynamic.ExpandoObject() as IDictionary<string, Object>;

        //                for (int i = 0; i < reader.FieldCount; i++)
        //                {
        //                    var columnName = objekt.ForvaltningsObjektPropertiesMetadata.Where(c => c.ColumnName == reader.GetName(i)).Select(s => s.Name).FirstOrDefault();
        //                    var columnDatatype = reader.GetDataTypeName(i);
        //                    if (columnName == null) 
        //                    {
        //                        if(columnDatatype == "integer") 
        //                        {
        //                            var idField = reader["id"].ToString();
        //                            data.Add("id", Convert.ToInt16(idField));
        //                        }
        //                        else 
        //                        { 
        //                            var geometry = reader["geometry"].ToString();
        //                            if(geometry != null && geometry != "{}") {
        //                                var geo = geometry;// JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(geometry);
        //                                if(geo != null) 
        //                                    data.Add("geometry", geo);
        //                            }
        //                        }
        //                    }
        //                    else { 
        //                        var cellValue = reader[i];
        //                        data.Add(columnName, cellValue);
        //                    }

        //                }

        //                items.Add(data);
        //            }

        //            dataObject.definition = objectDefinition;
        //            dataObject.objekt = items;

        //            return dataObject;
        //        }
        //        catch (NpgsqlException ex)
        //        {
        //        }
        //    }

        //    return null;
        //}

        //public async Task<List<Geonorge.Forvaltning.Models.Api.ForvaltningsObjektMetadata>> GetMetadataObjects()
        //{

        //    User user = await _authService.GetUserSupabase();

        //    var objectsList = _context.ForvaltningsObjektMetadata.Where(o => o.Organization == user.OrganizationNumber).ToList();
        //    List<Geonorge.Forvaltning.Models.Api.ForvaltningsObjektMetadata> objects = new List<Models.Api.ForvaltningsObjektMetadata>();
        //    if (objects != null) 
        //    {
        //        foreach (var objekt in objectsList)
        //            objects.Add( new Models.Api.ForvaltningsObjektMetadata { Id = objekt.Id, Name = objekt.Name });

        //    }
        //    return objects;
        //}

        public async Task<object?> RequestAuthorize()
        {
            User user = await _authService.GetUserSupabase();

            if (user == null)
                throw new UnauthorizedAccessException("Manglende eller feil autorisering");

            if (user != null && string.IsNullOrEmpty(user.OrganizationNumber)) 
            {
                try
                {
                    MimeMessage message = new MimeMessage();
                    MailboxAddress from = MailboxAddress.Parse(user.Email);
                    message.From.Add(from);

                    MailboxAddress to = MailboxAddress.Parse(_configEmail.WebmasterEmail);
                    message.To.Add(to);

                    message.Subject = "Forespørsel autorisasjon forvaltning.geonorge.no";


                    BodyBuilder bodyBuilder = new BodyBuilder();
                    bodyBuilder.HtmlBody = user.Name + " ønsker tilgang til datasett under forvaltning.geonorge.no";

                    message.Body = bodyBuilder.ToMessageBody();

                    SmtpClient client = new SmtpClient();
                    client.Connect(_configEmail.SmtpHost);

                    client.Send(message);
                    client.Disconnect(true);
                    client.Dispose();

                    _logger.LogInformation("Epost sendt til: " + _configEmail.WebmasterEmail + ", bruker " + user.Email + " ønsker tilgang");
                }

                catch (Exception ex)
                {
                    _logger.LogError("Error:", ex);
                    throw new Exception("Feil ved sending epost");
                }
            }

            return "Forespørsel sendt";
        }
    }

    public interface IObjectService
    {
        Task<DataObject> AddDefinition(ObjectDefinitionAdd o);
        Task<object?> DeleteObject(int id);
        Task<DataObject?> EditDefinition(int id, ObjectDefinitionEdit objekt);

        //Task<DataObject> AddObject(int id, ObjectItem o);
        //Task<List<Geonorge.Forvaltning.Models.Api.ForvaltningsObjektMetadata>> GetMetadataObjects();
        //Task<DataObject> GetMetadataObject(int id);
        Task<object?> RequestAuthorize();
    }
}
