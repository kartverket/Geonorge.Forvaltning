using Geonorge.Forvaltning.Models.Api.User;
using Geonorge.Forvaltning.Models.Entity;
using Microsoft.Extensions.Options;

namespace Geonorge.Forvaltning.Services
{
    public class ObjectService : IObjectService
    {
        private readonly ApplicationContext _context;
        private readonly IAuthService _authService;

        public ObjectService(ApplicationContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<object> AddDefinition(object o)
        {
            User user = await _authService.GetUser();

            if (user == null)
                throw new UnauthorizedAccessException("Brukeren har ikke tilgang");


            throw new NotImplementedException();
        }

        public Task<object> GetMetadataObject()
        {
            throw new NotImplementedException();
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
        Task<object> GetMetadataObject();
    }
}
