using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using System.Text.RegularExpressions;

namespace BackgroundTrackingCustomActions
{
    public class CustomActions
    {
        #region CHECK USER EMAIL

        [CustomAction]
        public static ActionResult CheckUserEmail(Session session)
        {
            session.Log("Begin CheckUserEmail");

            ActionResult returnValue = ActionResult.NotExecuted;

            string email = session["EMAIL"];

            CustomActionResult result = CheckUserEmailTask(email);
            //StatusCode = 0 - none
            //StatusCode = 1 - email is already registered
            //statusCode = 2 - Email not registered
            //statusCode = 3 - email not correct


            if (result.Success)
            {
                

                if ((result.StatusCode == 3) || (result.StatusCode == 2))
                {
                    session["DIALOGUEOK"] = "0";
                }
                else
                {
                    session["DIALOGUEOK"] = "1";
                    session["UserGUID"] = result.Data["UserGUID"];
                }

                returnValue = ActionResult.Success;
            }
            else
            {                
                   returnValue = ActionResult.Failure;

            }
            //session.Message(InstallMessage.Error|(InstallMessage)MessageButtons.OK, new Record { FormatString = "Product updated. To upgrade Project execute initHeating.ps1 }" });


            return returnValue;

        }
        public static CustomActionResult CheckUserEmailTask(string _Email)
        {

            APICommunication api = new APICommunication();

            //Prompt.ShowWaitDialog(_Email, "E-mail");

            CustomActionResult returnValue = new CustomActionResult();

            string email = _Email;// strKey;
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(email);
            if (match.Success)
            {
                try
                {
                    string result = api.CheckUserEmail(email);
                    result = result.Replace("\"", string.Empty);

                    if (string.IsNullOrEmpty(result))
                    {

                        System.Windows.Forms.MessageBox.Show("We can't find you in our directory. We have logged this issue and will be in contact shortly", email);
                        returnValue.StatusCode = 2;
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Email found", email);
                        returnValue.StatusCode = 1;
                        returnValue.Data.Add("UserGUID", result);
                    }
                    returnValue.Success = true;

                }
                catch (System.Net.WebException ex)
                {

                    if (ex.Response != null)
                    {
                        System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)ex.Response;

                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            System.Windows.Forms.MessageBox.Show("Server for email check is offline");
                        }

                    }
                    returnValue.Success = true;
                    returnValue.Message = ex.ToString(); 

                }
                catch (Exception ex)
                {

                    returnValue.Success = false;
                    returnValue.Message = ex.ToString();
                }
                

            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Email is not in the correct format", email);
                returnValue.StatusCode = 3;
                returnValue.Success = true;

            }

            

            return returnValue;

        }

        #endregion

        #region STORE USER EMAIL

        [CustomAction]
        public static ActionResult InitializeDatabase(Session session)
        {

            session.Log("Begin CheckUserEmail");

            ActionResult returnValue = ActionResult.NotExecuted;

            string email = session["EMAIL"];
            string userGUID = session["UserGUID"];
            string appRoot = session["MyAppDirectory"];

            
            CustomActionResult result = InitializeDatabaseTask(email, userGUID,appRoot);
            
            
            returnValue = result.Success ? ActionResult.Success : ActionResult.Failure;

            return returnValue;

        }
        
        public static CustomActionResult InitializeDatabaseTask(string _Email, string _UserGuid, string _AppRootLocation)
        {

            CustomActionResult returnValue = new CustomActionResult();

            

            string result = string.Empty;
            try
            {
                WIXInstallerBackgroundTrackingModels.WIXInstallerDatabaseInitializeInfo info = new WIXInstallerBackgroundTrackingModels.WIXInstallerDatabaseInitializeInfo();
                info.Password = "hg7jkd822lowDFmvb74";
                info.DBLocation = _AppRootLocation + @"\test.db";
                info.Email = _Email;
                info.UserGUID = _UserGuid;

                string parameter = Newtonsoft.Json.JsonConvert.SerializeObject(info);

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = new System.Diagnostics.ProcessStartInfo(_AppRootLocation + @"\Initializer.exe", Convert.ToBase64String(Encoding.UTF8.GetBytes(parameter)));
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
                proc.WaitForExit();

                
                System.IO.File.Delete(_AppRootLocation + @"\Initializer.exe");
                System.IO.File.Delete(_AppRootLocation + @"\SQLite.Interop.dll");
                System.IO.File.Delete(_AppRootLocation + @"\Newtonsoft.Json.dll");
                System.IO.File.Delete(_AppRootLocation + @"\System.Data.SQLite.dll");
                System.IO.File.Delete(_AppRootLocation + @"\WIXInstallerBackgroundTrackingModels.dll");  


                //result = proc.StandardOutput.ReadToEnd();


            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());

            }

            returnValue.Success = string.IsNullOrEmpty( result);

            return returnValue;

        }


        #endregion
    }
}

