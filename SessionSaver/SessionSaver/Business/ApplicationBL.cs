using System.Collections.Generic;
using SessionSaver.Model;
using SessionSaver.DataAccess;

namespace SessionSaver.Business
{
    public class ApplicationBL
    {
        public List<Application> GetUserApplications()
        {
            return ApplicationDA.GetUserApplications();
        }
    }
}
