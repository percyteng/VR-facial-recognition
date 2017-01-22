namespace VRFace.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.OData;
    using Microsoft.Azure.Mobile.Server;
    using VRFace.DataObjects;
    using VRFace.Models;
    using System.Net;
    using Cloud;
    using Microsoft.Azure.Mobile.Server.Config;
    
    public class UserController : TableController<User>
    {
        // GET tables/User
        public IQueryable<User> GetAllUsers()
        {
            return this.Query();
        }

        // GET tables/User/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<User> GetUser(string id)
        {
            return this.Lookup(id);
        }

        // GET tables/User/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public User Email(string email)
        {
            return this.GetAllUsers().First(u => u.Email == email);
        }

        // PATCH tables/User/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<User> PatchUser(string id, Delta<User> patch)
        {
            return this.UpdateAsync(id, patch);
        }

        // POST tables/User
        public async Task<IHttpActionResult> PostUser(User item)
        {

            if (item.FirstName.Length == 0 || item.Images.Length == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var current = await this.InsertAsync(item);

            await PersonManager.CreatePerson(item);

            return base.CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/User/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteUser(string id)
        {
            return this.DeleteAsync(id);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            var context = new MobileServiceContext();
            this.DomainManager = new EntityDomainManager<User>(context, Request);
        }
    }
}