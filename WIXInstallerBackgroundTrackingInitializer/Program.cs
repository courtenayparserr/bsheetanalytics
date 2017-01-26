using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WIXInstallerBackgroundTrackingInitializer
{
    class Program
    {
        static void Main(string[] args)
        {
            string output = RunProcess(args);


            Console.WriteLine("Output: " + output);
            Console.ReadLine();
        }

        static string RunProcess(string[] args)
        {
            Console.WriteLine("Entering RUN");
            string returnValue = string.Empty;

            if (args == null || args.Length == 0)
            {
                returnValue= "There was no parameters found";
            }
            else
            {
                
                string argument = args[0];

                try
                {
                    Console.WriteLine("base 64 argument {0}", argument);
                    argument = Encoding.UTF8.GetString(Convert.FromBase64String(argument));
                    Console.WriteLine("argument {0}",argument);
                    //System.Windows.Forms.MessageBox.Show(argument);
                    WIXInstallerBackgroundTrackingModels.WIXInstallerDatabaseInitializeInfo info = Newtonsoft.Json.JsonConvert.DeserializeObject<WIXInstallerBackgroundTrackingModels.WIXInstallerDatabaseInitializeInfo>(argument);
                    //System.Windows.Forms.MessageBox.Show("parsed");
                    
                    if (info != null)
                    {//
                        Console.WriteLine("info is not null");

                        if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(info.DBLocation)))
                        {
                            Console.WriteLine("directory exists");

                            if (string.Compare(info.Password, "hg7jkd822lowDFmvb74") == 0)
                            {
                                Console.WriteLine("password ok");
                                SQLiteCommunication sqlite = new SQLiteCommunication(info.DBLocation);

                                //System.Windows.Forms.MessageBox.Show("CREATE");
                                if (sqlite.CreateDatabase())
                                {
                                    Console.WriteLine("database created");
                                    ////System.Windows.Forms.MessageBox.Show("INSERT");
                                    sqlite.StoreUserEmail(info.Email, info.UserGUID);
                                    //System.Windows.Forms.MessageBox.Show("FINNISH");
                                    Console.WriteLine("database info stored");
                                }
                                else
                                {
                                    returnValue = "Database creation failed";
                                }
                            }
                            else
                            {
                                returnValue = "password is invalid";
                            }
                        }
                        else
                        {
                            returnValue = "No database directory";
                        }

                    }
                    else
                    {
                        returnValue = "parameter is null";
                    }

                }
                catch (Exception ex)
                {
                    returnValue = ex.ToString();
                }


            }
            return returnValue;
        }
    }
}
