using System;
using System.Collections.Generic;
using System.Text;
using Cliver;

namespace Example
{
 
    class GeneralSettings : Cliver.UserSettings//UserSettings based class is serialized in the user directory
    {
        public Dictionary<string, User> Users = new Dictionary<string, User>();
    }

    public class User
    {
        public string Name;//serialazable
        public string Email;//serialazable
        public bool Active = true;//serialazable

        public void Notify(string message)
        {
            Program.Email(Settings.Smtp.Host, Settings.Smtp.Port, Settings.Smtp.Password, message);
        }
    }
}
