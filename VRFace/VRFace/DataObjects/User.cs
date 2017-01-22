namespace VRFace.DataObjects
{
    using Microsoft.Azure.Mobile.Server;

    public class User : EntityData
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Facebook { get; set; }

        public string LinkedIn { get; set; }

        public string Email { get; set; }

        public string[] Interests { get; set; }

        public string GroupId { get; set; }

        public string Company { get; set; }

        public string Tagline { get; set; }

        public string[] Images { get; set; }
    }
}