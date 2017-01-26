using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WIXInstallerBackgroundTrackingModels
{
    public class WIXInstallerDatabaseInitializeInfo
    {
        public string Password { get; set; }
        public string DBLocation { get; set; }
        public string Email { get; set; }
        public string UserGUID { get; set; }
    }
}
