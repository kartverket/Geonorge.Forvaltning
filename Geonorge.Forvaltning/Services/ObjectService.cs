using Geonorge.Forvaltning.Models;
using Geonorge.Forvaltning.Models.Api;
using Geonorge.Forvaltning.Models.Api.User;
using Geonorge.Forvaltning.Models.Entity;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using Npgsql;
using System.Data;
using System.Security.Cryptography;


namespace Geonorge.Forvaltning.Services
{
    public class ObjectService : IObjectService
    {
        private readonly ApplicationContext _context;
        private readonly IAuthService _authService;
        private readonly DbConfiguration _config;
        private readonly EmailConfiguration _configEmail;
        private readonly ILogger<ObjectService> _logger;
        private readonly NpgsqlConnection _connection;

        private List<string> _countyGovernors = new List<string>()
                    {
                        "974762994",
                        "974761645",
                        "974764067",
                        "974764687",
                        "974761319",
                        "974763230",
                        "967311014",
                        "974764350",
                        "974762501",
                        "974760665",
                        "921627009"
                    };

    public ObjectService(
            ApplicationContext context,
            NpgsqlConnection connection,
            IAuthService authService, 
            IOptions<DbConfiguration> config, 
            IOptions<EmailConfiguration> configEmail, 
            ILogger<ObjectService> logger)
        {
            _context = context;
            _authService = authService;
            _config = config.Value;
            _configEmail = configEmail.Value;
            _logger = logger;
            _connection = connection;
        }

        public static void ValidateOrganizationNumber(string number)
        {
            if (string.IsNullOrEmpty(number) || (!string.IsNullOrEmpty(number) && (number.Length != 9 || !int.TryParse(number, out _))))
                throw new UnauthorizedAccessException("Feil organisasjonsnummer");

        }

        public static void ValidateOrganizationNumbers(List<string> numbers)
        {
            if (numbers != null && numbers.Count > 0)
            {
                foreach (var number in numbers)
                {
                    ValidateOrganizationNumber(number);
                }
            }
        }

        public async Task<DataObject> AddDefinition(ObjectDefinitionAdd o)
        {
            User user = await _authService.GetUserFromSupabaseAsync();

            if (user == null)
                throw new UnauthorizedAccessException("Manglende eller feil autorisering");

            if (string.IsNullOrEmpty(user.OrganizationNumber))
                throw new UnauthorizedAccessException("Brukeren har ikke tilgang");

            ValidateOrganizationNumber(user.OrganizationNumber);

            try
            {
                Models.Entity.ForvaltningsObjektMetadata metadata = new Models.Entity.ForvaltningsObjektMetadata();
                metadata.Organization = user.OrganizationNumber;
                metadata.Name = o.Name;
                metadata.Description = o.Description;
                metadata.IsOpenData = o.IsOpenData;
                metadata.AttachedForvaltningObjektMetadataIds = o.AttachedForvaltningObjektMetadataIds;
                metadata.TableName = "";
                metadata.ForvaltningsObjektPropertiesMetadata = new List<ForvaltningsObjektPropertiesMetadata>();

                int col = 1;
                foreach (var item in o.Properties)
                {
                    if(item.AllowedValues != null && !item.AllowedValues.Any())
                    {
                        item.AllowedValues = null;
                    }
                    metadata.ForvaltningsObjektPropertiesMetadata.Add(new ForvaltningsObjektPropertiesMetadata { Name = item.Name, DataType = item.DataType, ColumnName = "c_" + col, OrganizationNumber = user.OrganizationNumber, AllowedValues = item.AllowedValues });
                    col++;
                }
                _context.ForvaltningsObjektMetadata.Add(metadata);
                _context.SaveChanges();

                metadata.TableName = "t_" + metadata.Id.ToString();
                _context.SaveChanges();

                string sqlConstraints = "";

                string sql = "CREATE TABLE " + metadata.TableName + " (id SERIAL PRIMARY KEY,"; //Todo use uuid data type?
                foreach (var property in metadata.ForvaltningsObjektPropertiesMetadata)
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
                        sqlConstraints = sqlConstraints + " CHECK(" + property.ColumnName + " = ANY('{" + string.Join(",", property.AllowedValues) + "}'::text[]));";
                    }
                }
                sql = sql + " geometry geometry  ";
                sql = sql + ", updatedate timestamp with time zone  ";
                sql = sql + ", editor text  ";
                sql = sql + ", owner_org text  ";
                sql = sql + ", contributor_org text[]  ";
                sql = sql + ", viewer_org text[]  ";

                sql = sql + " ); ";


                sqlConstraints = sqlConstraints + "ALTER TABLE " + metadata.TableName + " ADD CONSTRAINT allowed_owner_org_" + metadata.TableName;
                sqlConstraints = sqlConstraints + " CHECK(owner_org = '" + metadata.Organization + "');";


                sql = sql + sqlConstraints;

                sql = sql + "alter table " + metadata.TableName + " enable row level security;";
                sql = sql + "CREATE POLICY \"Owner\" ON \"public\".\"" + metadata.TableName + "\" AS PERMISSIVE FOR ALL TO public USING ((EXISTS ( SELECT * FROM users WHERE (users.organization = " + metadata.TableName + ".owner_org) AND (" + metadata.TableName + ".owner_org = '" + metadata.Organization + "'::text)  ))) WITH CHECK ((EXISTS ( SELECT * FROM users WHERE (users.organization = " + metadata.TableName + ".owner_org) AND (" + metadata.TableName + ".owner_org = '" + metadata.Organization + "'::text) )));";
                sql = sql + "CREATE POLICY \"Contributor\" ON \"public\".\"" + metadata.TableName + "\" AS PERMISSIVE FOR ALL TO public USING ((EXISTS ( SELECT users.id, users.created_at, users.email, users.organization, users.editor, users.role FROM users, \"ForvaltningsObjektMetadata\"  WHERE ((users.organization = ANY (" + metadata.TableName + ".contributor_org)) AND ((\"ForvaltningsObjektMetadata\".\"Id\" = " + metadata.Id + ") AND (users.organization = ANY (\"ForvaltningsObjektMetadata\".\"Contributors\"))))))) WITH CHECK ((EXISTS ( SELECT users.id, users.created_at, users.email, users.organization, users.editor, users.role FROM users, \"ForvaltningsObjektMetadata\"  WHERE ((users.organization = ANY (" + metadata.TableName + ".contributor_org)) AND ((\"ForvaltningsObjektMetadata\".\"Id\" = " + metadata.Id + ") AND (users.organization = ANY (\"ForvaltningsObjektMetadata\".\"Contributors\")))))));";
                sql = sql + "CREATE POLICY \"Viewer\" ON \"public\".\"" + metadata.TableName + "\" AS PERMISSIVE FOR SELECT TO public USING ((EXISTS ( SELECT users.id, users.created_at, users.email, users.organization, users.editor, users.role FROM users, \"ForvaltningsObjektMetadata\"  WHERE ((users.organization = ANY (" + metadata.TableName + ".viewer_org)) AND ((\"ForvaltningsObjektMetadata\".\"Id\" = " + metadata.Id + ") AND (users.organization = ANY (\"ForvaltningsObjektMetadata\".\"Viewers\")))))));";
                
                _connection.Open();
                using var cmd = new NpgsqlCommand();
                cmd.Connection = _connection;
                cmd.CommandText = sql;
                await cmd.ExecuteNonQueryAsync();
                _connection.Close();

                var created = new DataObject { definition = new ObjectDefinition { Id = metadata.Id } };  //await GetMetadataObject(metadata.Id);


                return created;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError("Database error", ex);
                throw new Exception("Database error");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError("UnauthorizedAccessException", ex);
                throw new UnauthorizedAccessException("Ingen tilgang");
            }
            catch (Exception e)
            {
                _logger.LogError("Error", e);
                throw new Exception("En feil oppstod");
            }
        }

        public async Task<DataObject?> EditDefinition(int id, ObjectDefinitionEdit objekt)
        {
            User user = await _authService.GetUserFromSupabaseAsync();

            if (user == null)
                throw new UnauthorizedAccessException("Manglende eller feil autorisering");

            if (string.IsNullOrEmpty(user.OrganizationNumber))
                throw new UnauthorizedAccessException("Brukeren har ikke tilgang");

            try
            {
                var current = await _context.ForvaltningsObjektMetadata
                    .Include(metadata => metadata.ForvaltningsObjektPropertiesMetadata)
                        .ThenInclude(propMetadata => propMetadata.AccessByProperties)
                    .SingleOrDefaultAsync(metadata => metadata.Id == id && metadata.Organization == user.OrganizationNumber);

                if (current == null)
                    throw new UnauthorizedAccessException("Bruker har ikke tilgang til objekt");

                await UpdateForvaltningObjektMetadata(current, objekt);

                var currentProperties = current.ForvaltningsObjektPropertiesMetadata.Select(y => y.Id).ToList();
                var changedProperties = objekt.Properties.Where(z => z.Id > 0).Select(n => n.Id).ToList();

                var itemsToRemove = currentProperties.Where(x => !changedProperties.Any(z => z.Value == x)).ToList();

                foreach (var itemToRemove in itemsToRemove)
                {
                    //Get columnName
                    var columnName = current.ForvaltningsObjektPropertiesMetadata.Where(c => c.Id == itemToRemove).Select(co => co.ColumnName).FirstOrDefault();
                    //DROP COLUMN 
                    var sql = "ALTER TABLE " + current.TableName + " DROP COLUMN " + columnName + ";";
                    _connection.Open();
                    using var cmd = new NpgsqlCommand();
                    cmd.Connection = _connection;
                    cmd.CommandText = sql;
                    await cmd.ExecuteNonQueryAsync();
                    _connection.Close();
                    //delete row from ForvaltningsObjektPropertiesMetadata
                    var itemToDelete = current.ForvaltningsObjektPropertiesMetadata.Where(c => c.Id == itemToRemove).Single();
                    _context.ForvaltningsObjektPropertiesMetadata.Remove(itemToDelete);
                    _context.SaveChanges();
                }

                foreach (var item in objekt.Properties)
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
                        _connection.Open();
                        using var cmdC = new NpgsqlCommand();
                        cmdC.Connection = _connection;
                        cmdC.CommandText = sqlC;
                        cmdC.Parameters.AddWithValue(current.Id);

                        var reader = await cmdC.ExecuteReaderAsync();

                        await reader.ReadAsync();

                        var lastColumn = reader.GetInt32(0);

                        _connection.Close();


                        var columnName = "c_" + (lastColumn + 1);

                        var sql = "ALTER TABLE " + current.TableName + " ADD COLUMN " + columnName + " " + sqlDataType + ";";

                        var sqlConstraints = "";

                        if (item.AllowedValues != null && item.AllowedValues.Any())
                        {
                            sqlConstraints = sqlConstraints + "ALTER TABLE " + current.TableName + " ADD CONSTRAINT allowed_" + current.TableName + "_" + columnName;
                            sqlConstraints = sqlConstraints + " CHECK(" + columnName + " = ANY('{" + string.Join(",", item.AllowedValues) + "}'::text[]));";
                        }

                        sql = sql + sqlConstraints;

                        _connection.Open();
                        using var cmd = new NpgsqlCommand();
                        cmd.Connection = _connection;
                        cmd.CommandText = sql;
                        await cmd.ExecuteNonQueryAsync();
                        _connection.Close();

                        if (item.AllowedValues != null && !item.AllowedValues.Any())
                        {
                            item.AllowedValues = null;
                        }

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
                            var sql = "ALTER TABLE " + current.TableName + " ALTER COLUMN " + columnName + " SET DATA TYPE " + sqlDataType + ";";

                            _connection.Open();
                            using var cmd = new NpgsqlCommand();
                            cmd.Connection = _connection;
                            cmd.CommandText = sql;
                            await cmd.ExecuteNonQueryAsync();
                            _connection.Close();

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
                        var oldAllowedValues = property.AllowedValues != null ? property.AllowedValues : new List<string>();
                        var newAllowedValues = item.AllowedValues;
                        var difference = oldAllowedValues?.Except(newAllowedValues != null ? newAllowedValues : new List<string>());
                        if (difference != null && difference.Any())
                            allowedValuesChanged = true;
                        else if (oldAllowedValues != null && newAllowedValues != null)
                            if (oldAllowedValues.Count != newAllowedValues.Count)
                                allowedValuesChanged = true;

                        if (allowedValuesChanged)
                        {
                            //Change constraint
                            var sqlConstraints = "";

                            sqlConstraints = sqlConstraints + "ALTER TABLE " + current.TableName + " DROP CONSTRAINT IF EXISTS allowed_" + current.TableName + "_" + columnName;
                            if (item.AllowedValues != null)
                            {
                                sqlConstraints = sqlConstraints + ", ADD CONSTRAINT allowed_" + current.TableName + "_" + columnName;
                                sqlConstraints = sqlConstraints + " CHECK(" + columnName + " = ANY('{" + string.Join(",", item.AllowedValues) + "}'::text[]));";
                            }
                            _connection.Open();
                            using var cmd = new NpgsqlCommand();
                            cmd.Connection = _connection;
                            cmd.CommandText = sqlConstraints;
                            await cmd.ExecuteNonQueryAsync();
                            _connection.Close();

                            property.AllowedValues = item.AllowedValues;
                            _context.SaveChanges();

                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError("Database error", ex);
                throw new Exception("Database error");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError("UnauthorizedAccessException", ex);
                throw new UnauthorizedAccessException("Ingen tilgang");
            }
            catch (Exception e)
            {
                _logger.LogError("Error", e);
                throw new Exception("En feil oppstod");
            }

            return null;

        }

        public async Task DeleteObjectAsync(int id)
        {
            var user = await _authService.GetUserFromSupabaseAsync() ??
                throw new UnauthorizedAccessException("Manglende eller feil autorisering");

            if (string.IsNullOrEmpty(user.OrganizationNumber))
                throw new UnauthorizedAccessException("Bruker har ikke tilgang");

            var objekt = await _context.ForvaltningsObjektMetadata
                .SingleOrDefaultAsync(metadata => metadata.Id == id && metadata.Organization == user.OrganizationNumber);

            if (objekt == null)
                throw new UnauthorizedAccessException("Bruker har ikke tilgang til objekt");

            try
            {
                var propertiesMetadatas = await _context.ForvaltningsObjektPropertiesMetadata
                    .Where(metadata => metadata.ForvaltningsObjektMetadataId == objekt.Id)
                    .ToListAsync();

                _context.ForvaltningsObjektPropertiesMetadata.RemoveRange(propertiesMetadatas);
                _context.ForvaltningsObjektMetadata.Remove(objekt);

                await _context.SaveChangesAsync();

                var sql = $"DROP TABLE {objekt.TableName};";

                _connection.Open();

                using var command = new NpgsqlCommand();
                command.Connection = _connection;
                command.CommandText = sql;

                await command.ExecuteNonQueryAsync();
                _connection.Close();
            }
            catch (NpgsqlException exception)
            {
                _logger.LogError("Database error: {exception}", exception);
                throw new Exception("Database error");
            }
            catch (UnauthorizedAccessException exception)
            {
                _logger.LogError("UnauthorizedAccessException: {exception}", exception);
                throw new UnauthorizedAccessException("Ingen tilgang");
            }
            catch (Exception exception)
            {
                _logger.LogError("Error {exception}", exception);
                throw new Exception("En feil oppstod");
            }
        }

        public async Task RequestAuthorizationAsync()
        {
            var user = await _authService.GetUserFromSupabaseAsync() ??
                throw new UnauthorizedAccessException("Manglende eller feil autorisering");

            if (!string.IsNullOrEmpty(user?.OrganizationNumber))
                throw new Exception("Brukeren er allerede autorisert");

            try
            {
                using var message = new MimeMessage();

                var from = MailboxAddress.Parse(user.Email);
                var to = MailboxAddress.Parse(_configEmail.WebmasterEmail);

                message.From.Add(from);
                message.To.Add(to);
                message.Subject = "Forespørsel om autorisasjon forvaltning.geonorge.no";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $"{user.Name} ønsker tilgang til datasett under forvaltning.geonorge.no."
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                client.Connect(_configEmail.SmtpHost);
                client.Send(message);
                client.Disconnect(true);

                _logger.LogInformation("E-post sendt til: {webmasterEmail}. Bruker {userEmail} ønsker tilgang", _configEmail.WebmasterEmail, user.Email);
            }
            catch (Exception exception)
            {
                _logger.LogError("Error: {exception}", exception);
                throw new Exception("Feil ved sending av e-post");
            }
        }

        public async Task<object?> Access(ObjectAccess access)
        {

            User user = await _authService.GetUserFromSupabaseAsync();

            if (user == null)
                throw new UnauthorizedAccessException("Manglende eller feil autorisering");

            if (string.IsNullOrEmpty(user.OrganizationNumber))
                throw new UnauthorizedAccessException("Brukeren har ikke tilgang");

            foreach (var prop in access.AccessByProperties)
            {
                ValidateOrganizationNumbers(prop.Contributors);
                Helper.CheckString(prop.Value);
            }

            ValidateOrganizationNumbers(access.Contributors);

            ValidateOrganizationNumbers(access.Viewers);

            try
            {
                var objekt = _context.ForvaltningsObjektMetadata.Where(x => x.Id == access.objekt && x.Organization == user.OrganizationNumber).Include(i => i.ForvaltningsObjektPropertiesMetadata).FirstOrDefault();
                if (objekt == null)
                {
                    throw new UnauthorizedAccessException("Bruker har ikke tilgang til objekt");
                }

                objekt.Contributors = access.Contributors;
                _context.SaveChanges();

                foreach (var prop in objekt.ForvaltningsObjektPropertiesMetadata)
                {
                    prop.Contributors = access.Contributors;
                    _context.SaveChanges();
                }

                objekt.Viewers = access.Viewers;
                _context.SaveChanges();

                foreach (var prop in objekt.ForvaltningsObjektPropertiesMetadata)
                {
                    prop.Viewers = access.Viewers;
                    _context.SaveChanges();
                }

                var hasPropertyAccess = false;

                foreach (var prop in access.AccessByProperties)
                {
                    if (prop != null)
                        hasPropertyAccess = true;
                }


                //Remove rights contributor
                var sql = "UPDATE " + objekt.TableName + " SET contributor_org = NULL;";
                _connection.Open();
                using var cmd = new NpgsqlCommand();
                cmd.Connection = _connection;
                cmd.CommandText = sql;
                await cmd.ExecuteNonQueryAsync();
                _connection.Close();

                //Remove rights viewer
                sql = "UPDATE " + objekt.TableName + " SET viewer_org = NULL;";
                _connection.Open();
                using var cmdViewer = new NpgsqlCommand();
                cmdViewer.Connection = _connection;
                cmdViewer.CommandText = sql;
                await cmdViewer.ExecuteNonQueryAsync();
                _connection.Close();


                //Remove old policy

                List<string> removeIds = new List<string>();

                sql = "Select \"Id\" FROM \"AccessByProperties\"  where \"ForvaltningsObjektPropertiesMetadataId\" IN (SELECT \"Id\" FROM \"ForvaltningsObjektPropertiesMetadata\" WHERE \"ForvaltningsObjektMetadataId\" = " + objekt.Id + ")";

                _connection.Open();
                using var cmd2 = new NpgsqlCommand();
                cmd2.Connection = _connection;
                cmd2.CommandText = sql;
                using var reader = await cmd2.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    removeIds.Add(reader["Id"].ToString());
                }

                _connection.Close();

                foreach (var id in removeIds)
                {
                    sql = "DROP POLICY IF EXISTS \"Property" + id + "\" ON " + objekt.TableName;
                    _connection.Open();
                    using var cmd3 = new NpgsqlCommand();
                    cmd3.Connection = _connection;
                    cmd3.CommandText = sql;
                    await cmd3.ExecuteNonQueryAsync();
                    _connection.Close();
                }

                //Remove AccessByProperties config
                sql = "DELETE FROM \"AccessByProperties\" where \"ForvaltningsObjektPropertiesMetadataId\" IN (SELECT \"Id\" FROM \"ForvaltningsObjektPropertiesMetadata\" WHERE \"ForvaltningsObjektMetadataId\" = " + objekt.Id + ")";

                _connection.Open();
                using var cmd4 = new NpgsqlCommand();
                cmd4.Connection = _connection;
                cmd4.CommandText = sql;
                await cmd4.ExecuteNonQueryAsync();
                _connection.Close();

                List<string> contributors = new List<string>();

                //Set new rights

                if (access.Viewers != null && access.Viewers.Count > 0)
                {

                    sql = "UPDATE " + objekt.TableName + " SET viewer_org = '{" + string.Join(",", objekt.Viewers) + "}'::text[];";

                    _connection.Open();
                    using var cmd2b = new NpgsqlCommand();
                    cmd2b.Connection = _connection;
                    cmd2b.CommandText = sql;
                    await cmd2b.ExecuteNonQueryAsync();
                    _connection.Close();
                }


                if (!hasPropertyAccess && access.Contributors != null && access.Contributors.Count > 0)
                {

                    sql = "UPDATE " + objekt.TableName + " SET contributor_org = '{" + string.Join(",", objekt.Contributors) + "}'::text[];";

                    _connection.Open();
                    using var cmd2b = new NpgsqlCommand();
                    cmd2b.Connection = _connection;
                    cmd2b.CommandText = sql;
                    await cmd2b.ExecuteNonQueryAsync();
                    _connection.Close();
                }

                else
                {
                    foreach (var prop in access.AccessByProperties)
                    {
                        var property = objekt.ForvaltningsObjektPropertiesMetadata.Where(f => f.Id == prop.PropertyId).FirstOrDefault();

                        if (property != null)
                        {

                            if (property.AccessByProperties == null)
                                property.AccessByProperties = new List<AccessByProperties>();

                            contributors.AddRange(prop.Contributors);

                            //Insert into config AccessByProperties
                            var accessProperty = new Models.Entity.AccessByProperties { Value = prop.Value, Contributors = prop.Contributors, Organization = objekt.Organization };
                            property.AccessByProperties.Add(accessProperty);
                            _context.SaveChanges();

                            //CREATE POLICY
                            var policyValue = "'" + prop.Value + "'";
                            object sqlValue = prop.Value;
                            if (property.DataType.Contains("bool"))
                            {
                                policyValue = prop.Value;
                                sqlValue = Boolean.Parse(prop.Value);
                            }
                            else if (property.DataType.Contains("numeric"))
                            {
                                policyValue = prop.Value;
                                sqlValue = Convert.ToDouble(prop.Value);
                            }
                            else if (property.DataType.Contains("timestamp"))
                            {
                                policyValue = "'" + prop.Value + "'";
                                sqlValue = Convert.ToDateTime(prop.Value);
                            }

                            sql = "CREATE POLICY \"Property" + accessProperty.Id + "\" ON \"public\".\"" + objekt.TableName + "\" AS PERMISSIVE FOR ALL TO public USING ((EXISTS ( SELECT users.id, users.created_at, users.email, users.organization, users.editor, users.role FROM users WHERE ((users.organization = ANY (" + objekt.TableName + ".contributor_org)) AND (" + objekt.TableName + "." + property.ColumnName + " = " + policyValue + ") AND (" + objekt.TableName + ".contributor_org = '{" + string.Join(",", prop.Contributors) + "}'))))) WITH CHECK ((EXISTS ( SELECT users.id, users.created_at, users.email, users.organization, users.editor, users.role FROM users WHERE ((users.organization = ANY (" + objekt.TableName + ".contributor_org)) AND (" + objekt.TableName + "." + property.ColumnName + " = " + policyValue + ") AND (" + objekt.TableName + ".contributor_org = '{" + string.Join(",", prop.Contributors) + "}')))));";

                            _connection.Open();
                            using var cmd3 = new NpgsqlCommand();
                            cmd3.Connection = _connection;
                            cmd3.CommandText = sql;
                            await cmd3.ExecuteNonQueryAsync();
                            _connection.Close();

                            //Update table with contributor_org
                            sql = "UPDATE " + objekt.TableName + " SET contributor_org = '{" + string.Join(",", accessProperty.Contributors) + "}' Where " + property.ColumnName + "=@value;";

                            _connection.Open();
                            using var cmd5 = new NpgsqlCommand();
                            cmd5.Parameters.AddWithValue("@value", sqlValue);
                            cmd5.Connection = _connection;
                            cmd5.CommandText = sql;
                            await cmd5.ExecuteNonQueryAsync();
                            _connection.Close();

                        }

                    }

                    foreach (var egenskap in objekt.ForvaltningsObjektPropertiesMetadata)
                    {
                        egenskap.Contributors = contributors.Distinct().ToList();
                        _context.SaveChanges();
                    }
                }

            }
            catch (NpgsqlException ex)
            {
                _logger.LogError("Database error", ex);
                throw new Exception("Database error");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError("UnauthorizedAccessException", ex);
                throw new UnauthorizedAccessException("Ingen tilgang");
            }
            catch (Exception e)
            {
                _logger.LogError("Error", e);
                throw new Exception("En feil oppstod");
            }

            return null;
        }

        private async Task UpdateForvaltningObjektMetadata(ForvaltningsObjektMetadata current, ObjectDefinitionEdit objekt)
        {
            var hasChanges = false;

            if (current.Name != objekt.Name)
            {
                current.Name = objekt.Name;
                hasChanges = true;
            }

            if (current.Description != objekt.Description)
            {
                current.Description = objekt.Description;
                hasChanges = true;
            }

            if (current.IsOpenData != objekt.IsOpenData)
            {
                current.IsOpenData = objekt.IsOpenData;
                hasChanges = true;
            }

            var currentAttachedIds = current.AttachedForvaltningObjektMetadataIds;
            var attachedIds = objekt.AttachedForvaltningObjektMetadataIds;

            if (currentAttachedIds != null && attachedIds != null &&
                (currentAttachedIds.Count != attachedIds.Count || currentAttachedIds.Except(attachedIds).Any()) ||
                currentAttachedIds == null && attachedIds != null ||
                currentAttachedIds != null && attachedIds == null)
            {
                current.AttachedForvaltningObjektMetadataIds = objekt.AttachedForvaltningObjektMetadataIds;
                hasChanges = true;
            }

            if (hasChanges)
                await _context.SaveChangesAsync();
        }

        public async Task<object> EditTag(int datasetId, int id, string tag)
        {
            var user = await _authService.GetUserFromSupabaseAsync();

            if (user == null)
                throw new UnauthorizedAccessException("Manglende eller feil autorisering");

            try
            {
                var owner =  _context.ForvaltningsObjektMetadata
                .Any(metadata => metadata.Id == datasetId && metadata.Organization == user.OrganizationNumber);

                if (!owner)
                    if(!_countyGovernors.Contains(user.OrganizationNumber))
                        throw new UnauthorizedAccessException("Bruker har ikke tilgang til objekt");

                var table = "t_" + datasetId;
                var sql = $"UPDATE {table} set tag = @tag where id= @id";
                _connection.Open();
                using var cmd = new NpgsqlCommand();
                cmd.Connection = _connection;
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@tag", tag);
                cmd.Parameters.AddWithValue("@id", id);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError("Error: {exception}", exception);
                throw new Exception("Feil ved lagring av data");
            }
            return null;
        }
    }

    public interface IObjectService
    {
        Task<object?> Access(ObjectAccess access);
        Task<DataObject> AddDefinition(ObjectDefinitionAdd o);
        Task DeleteObjectAsync(int id);
        Task<DataObject?> EditDefinition(int id, ObjectDefinitionEdit objekt);
        Task<object> EditTag(int datasetId, int id, string tag);
        Task RequestAuthorizationAsync();
    }
}
