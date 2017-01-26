using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CustomAction
{
    [RunInstaller(true)]
    public partial class Installer1 : System.Configuration.Install.Installer
    {
        public Installer1()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary savedState)
        {
            APICommunication api = new APICommunication();


            string strKey = Context.Parameters["KeyValue"];
            string email = strKey;
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(email);
            if (match.Success)
            {
                bool result = api.CheckUserEmail(email);

                if (!result)
                {
                    
                    Prompt.ShowWaitDialog("We can't find you in our directory. We have logged this issue and will be in contact shortly",email);
                }
                else
                {
                    Prompt.ShowWaitDialog("User added", email);
                }

                //base.Commit(savedState);

            }
            else
            {
                Prompt.ShowWaitDialog("email is not in the correct format", email);
                
            }

            
        }

        private void Installer1_Committing(object sender, InstallEventArgs e)
        {
          

        }
    }
}
