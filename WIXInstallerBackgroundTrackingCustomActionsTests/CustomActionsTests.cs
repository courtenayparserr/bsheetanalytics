using Microsoft.VisualStudio.TestTools.UnitTesting;
using BackgroundTrackingCustomActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundTrackingCustomActions.Tests
{
    [TestClass()]
    public class CustomActionsTests
    {
        [TestMethod()]
        public void CheckUserEmailTaskTest()
        {
            var actionResul = CustomActions.CheckUserEmailTask("djigli@gmail.com"); //"courtenay@sharepointtutorials.net"); //

            Assert.AreEqual(actionResul.StatusCode, 2);

        }

        [TestMethod()]
        public void InitializeDatabaseTaskTest()
        {
            string dataLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            dataLocation = @"D:\Google Drive\Projects\Pet Heaven\Code\GetBeam.co\WIXInstallerBackgroundTrackingInitializer\bin\Debug" ;
            var actionResult = CustomActions.InitializeDatabaseTask("djigli@gmail.com", "ef5cdd21-2f5c-44f7-b586-bda9187874d5", dataLocation);
        }
    }
}