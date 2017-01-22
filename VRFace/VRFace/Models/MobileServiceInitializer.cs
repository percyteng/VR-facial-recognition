namespace VRFace.Models
{
    using DataObjects;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web;

    public class MobileServiceInitializer : CreateDatabaseIfNotExists<MobileServiceContext>
    {
        protected override void Seed(MobileServiceContext context)
        {
            List<User> todoItems = new List<User>
            {
                new User { Id = Guid.NewGuid().ToString(), Company = "Solsteace", Email = "akshay.nair@mail.utoronto.ca",
                            FirstName = "Akshay", LastName = "Nair", Tagline = "Software Developer",
                            Interests = new string[] { "Back-end", "Software" }, LinkedIn = "nairakshay",
                            Images = new string[] { "fakepath.jpg" } },
            };

            foreach (User todoItem in todoItems)
            {
                context.Set<User>().Add(todoItem);
            }

            base.Seed(context);
        }
    }
}