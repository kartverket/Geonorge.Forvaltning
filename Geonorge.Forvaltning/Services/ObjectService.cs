using Geonorge.Forvaltning.Models.Api.User;
using Geonorge.Forvaltning.Models.Entity;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

        public async Task<object> AddDefinition(object o)
        {
            User user = await _authService.GetUser();

            if (user == null)
                throw new UnauthorizedAccessException("Brukeren har ikke tilgang");


            throw new NotImplementedException();
        }

        public async Task<List<object>> GetMetadataObject(int id)
        {
            var objekt = _context.ForvaltningsObjektMetadata.Where(x => x.Id == id).Include(i => i.ForvaltningsObjektPropertiesMetadata).FirstOrDefault();
            if(objekt != null) 
            {
                //var data = await _context.Database.ExecuteSqlRawAsync(@"SELECT * FROM " + objekt.Name + "");
                var columnsList = objekt.ForvaltningsObjektPropertiesMetadata.Select(x => x.Name).ToList();
                var columns = string.Join<string>(",", columnsList);
                var con = new NpgsqlConnection(
                connectionString: _config.ConnectionString);
                con.Open();
                using var cmd = new NpgsqlCommand();
                cmd.Connection = con;

                cmd.CommandText = $"SELECT {columns} FROM {objekt.Name}";

                await using var reader = await cmd.ExecuteReaderAsync();

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
        Task<object> AddDefinition(object o);
        Task<List<Geonorge.Forvaltning.Models.Api.ForvaltningsObjektMetadata>> GetMetadataObjects();
        Task<List<object>> GetMetadataObject(int id);
    }
}
