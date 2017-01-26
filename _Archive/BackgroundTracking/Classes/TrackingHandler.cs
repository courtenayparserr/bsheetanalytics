using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Automation;
using NDde.Client;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using System.ComponentModel;

namespace BackgroundTracking.Classes
{
    public class TrackingHandler
    {

        #region PROPERTIES

        private List<WebPage> pPages = new List<WebPage>();
        //private List<WebPage> pChromePages = new List<WebPage>();
        private List<ApplicationProcess> pApplications = new List<ApplicationProcess>();

        private ApplicationProcess pActiveApplication = null;
        private WebPage pActiveWebPage = null;

        private CancellationTokenSource wtoken;
        public CancellationTokenSource Token
        {
            get
            {
                return wtoken;
            }

            set
            {
                wtoken = value;
            }
        }

        ITargetBlock<DateTimeOffset> task;

        public delegate void SecondPassedHandler(string _Text);
        public event SecondPassedHandler SecondPassedReached;

        #endregion

        #region CONSTRUCTORS

        public TrackingHandler()
        {
            
        }

        #endregion

        #region EVENTS



        #endregion

        #region METHODS

        private void InitializeThis()
        {
            //StartTimer();
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private void SetActiveProcessName()
        {
            
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            Process p = Process.GetProcessById((int)pid);

            if (p != null)
            {
                if (pActiveApplication != null && p.ProcessName == pActiveApplication.Name)
                {
                    pActiveApplication.Activate();
                }
                else
                {
                    if (pActiveApplication != null)
                    {
                        pActiveApplication.Deactivate();
                    }

                    pActiveApplication = pApplications.FirstOrDefault(x => x.Name == p.ProcessName);

                    if (pActiveApplication == null)
                    {
                        pActiveApplication = new ApplicationProcess(p.ProcessName, p.Handle);
                    }

                    pActiveApplication.Activate();
                    pApplications.Add(pActiveApplication);
                }
                
            }
            else
            {
                if (pActiveApplication != null)
                {
                    pActiveApplication.Deactivate();
                    pActiveApplication = null;
                }
            }

        }

        private void SetActiveURL()
        {
         
            if (pActiveApplication != null)
            {

                string url = string.Empty;

                if (pActiveApplication.Name == "firefox")
                {
                    url = GetActiveURLFromFirefox(pActiveApplication.Handle.ToInt32());
                }
                else if (pActiveApplication.Name == "chrome")
                {
                    url = GetActiveURLFromChrome();
                    
                }
                else
                {

                    if (pActiveWebPage != null)
                    {
                        pActiveWebPage.Deactivate();
                    }

                    pActiveWebPage = null;

                }

                if (!string.IsNullOrEmpty(url))
                {

                    if (pActiveWebPage != null && string.Compare(pActiveWebPage.URL, url) == 0)
                    {
                        pActiveWebPage.Activate();
                    }
                    else
                    {

                        if (pActiveWebPage != null)
                        {
                            pActiveWebPage.Deactivate();
                        }

                        pActiveWebPage = pPages.FirstOrDefault(x => string.Compare(x.URL,url) == 0);

                        if (pActiveWebPage == null)
                        {
                            pActiveWebPage = new WebPage(url);
                        }
                        
                        pActiveWebPage.Activate();
                        pPages.Add(pActiveWebPage);
                    }
                }

            }
            else
            {
                pActiveWebPage = null;
            }
        }

        private string GetActiveURLFromFirefox(int _ProcessID)
        {
            DdeClient dde = new DdeClient("Firefox", "WWW_GetWindowInfo");
            dde.Connect();
            string url = dde.Request("URL", int.MaxValue);
            url = url.Replace("\"", "").Replace("\0", "");
            dde.Disconnect();
            return url;
        }

        private string GetActiveURLFromChrome()
        {
            Process procsChrome = null;
            foreach (Process proc in Process.GetProcessesByName("chrome"))
            {
                if (proc.MainWindowHandle != IntPtr.Zero)
                {
                    procsChrome = proc;
                }
            }

            if (procsChrome == null)
            {
                return string.Empty;
            }

            AutomationElement elm = AutomationElement.FromHandle(procsChrome.MainWindowHandle);
            AutomationElement elmUrlBar = elm.FindFirst(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.NameProperty, "Address and search bar"));

            if (elmUrlBar != null)
            {
                AutomationPattern[] patterns = elmUrlBar.GetSupportedPatterns();
                if (patterns.Length > 0)
                {
                    ValuePattern val = (ValuePattern)elmUrlBar.GetCurrentPattern(patterns[0]);
                    return val.Current.Value;
                    
                }
            }
            return string.Empty;
        }

        public void CyclicCheck()
        {

            //int counter = 0;

            while (true)
            {
                try
                {
                    SetActiveProcessName();

                    SetActiveURL();

                    //string iconText = (pActiveApplication != null ? pActiveApplication.Name + (pActiveWebPage != null ? " (" + pActiveWebPage.URL + ")" : string.Empty) : string.Empty);
                    //if (iconText.Length >= 64)
                    //{
                    //    iconText = iconText.Substring(0, 63);
                    //}

                    //SecondPassedReached?.Invoke(iconText);
                    break;
                }
                catch (Win32Exception ex)
                {
                    //counter += 1;                    
                    //continue;
                }
                catch (Exception ex)
                {


                }
            }

        }

        public void SaveAndClose()
        {
            if (pActiveApplication != null)
            {
                pActiveApplication.Deactivate();
            }
            if (pActiveWebPage != null)
            {
                pActiveWebPage.Deactivate();
            }

        }
        

        #region TIMER

        private void SubstractOneSecond()
        {
            CyclicCheck();
        }

        ITargetBlock<DateTimeOffset> CreateNeverEndingTask(Action<DateTimeOffset> action, CancellationToken cancellationToken)
        {
            // Validate parameters.
            if (action == null) throw new ArgumentNullException("action");

            // Declare the block variable, it needs to be captured.
            ActionBlock<DateTimeOffset> block = null;

            // Create the block, it will call itself, so
            // you need to separate the declaration and
            // the assignment.
            // Async so you can wait easily when the
            // delay comes.
            block = new ActionBlock<DateTimeOffset>(async now => {
                // Perform the action.
                action(now);

                // Wait.
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).
                    // Doing this here because synchronization context more than
                    // likely *doesn't* need to be captured for the continuation
                    // here.  As a matter of fact, that would be downright
                    // dangerous.
                    ConfigureAwait(false);

                // Post the action back to the block.
                block.Post(DateTimeOffset.Now);
            }, new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken
            });

            // Return the block.
            return block;
        }

        public  void StartTimer()
        {
            

            // Create the token source.
            wtoken = new CancellationTokenSource();

            // Set the task.
            task = CreateNeverEndingTask(now => SubstractOneSecond(), wtoken.Token);

            // Start the task.  Post the time.
            task.Post(DateTimeOffset.Now);
        }

        #endregion



        #endregion

        #region OBSOLETE

        //public void InitializeThis()
        //{


        //    CheckAllURLs();

        //    string ComputerName = "localhost";
        //    string WmiQuery;
        //    ManagementEventWatcher Watcher, endWatcher;
        //    ManagementScope Scope;

        //    Scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", ComputerName), null);
        //    Scope.Connect();

        //    WmiQuery = "Select * From __InstanceCreationEvent Within 1 " +
        //    "Where TargetInstance ISA 'Win32_Process' ";

        //    Watcher = new ManagementEventWatcher(Scope, new EventQuery(WmiQuery));
        //    Watcher.EventArrived += new EventArrivedEventHandler(this.WmiEventHandler);

        //    Watcher.Start();


        //    WmiQuery = "Select * From __InstanceDeletionEvent Within 1 " +
        //    "Where TargetInstance ISA 'Win32_Process' ";

        //    endWatcher = new ManagementEventWatcher(Scope, new EventQuery(WmiQuery));
        //    endWatcher.EventArrived += new EventArrivedEventHandler(this.WmiEventDeletionHandler);
        //    endWatcher.Start();



        //}

        //public void CyclicCheckAll()
        //{
        //    CheckAllURLs();

        //    foreach (ApplicationProcess ap in pApplications)
        //    {
        //        Process myProcess = Process.GetProcesses().Single(p => p.Id != 0 && p.Handle == ap.Handle);
        //    }

        //}

        //public string CheckAllURLs()
        //{

        //    List<string> urls = new List<string>();

        //    try
        //    {
        //        Process firefox = Process.GetProcessesByName("firefox")[0];

        //        AutomationElement rootElement = AutomationElement.FromHandle(firefox.MainWindowHandle);

        //        Condition condDocAll = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Document);

        //        var list = rootElement.FindAll(TreeScope.Descendants, condDocAll);

        //        foreach (AutomationElement docElement in list)
        //        {
        //            foreach (AutomationPattern pattern in docElement.GetSupportedPatterns())
        //            {
        //                if (docElement.GetCurrentPattern(pattern) is ValuePattern)
        //                {
        //                    //Console.WriteLine((docElement.GetCurrentPattern(pattern) as ValuePattern).Current.Value.ToString() + Environment.NewLine);
        //                    urls.Add((docElement.GetCurrentPattern(pattern) as ValuePattern).Current.Value.ToString());

        //                }
        //            }
        //        }

        //        //Condition condCustomControl = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.);
        //        //AutomationElement firstCustomControl = GetNextCustomControl(rootElement, condCustomControl);
        //        //AutomationElement secondCustomControl = GetNextCustomControl(firstCustomControl, condCustomControl);
        //        ////AutomationElement thirdCustomControl = GetNextCustomControl(secondCustomControl, condCustomControl);
        //        //foreach (AutomationElement thirdElement in secondCustomControl.FindAll(TreeScope.Children, condCustomControl))
        //        //{
        //        //    foreach (AutomationElement fourthElement in thirdElement.FindAll(TreeScope.Children, condCustomControl))
        //        //    {
        //        //        Condition condDocument = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Document);
        //        //        AutomationElement docElement = fourthElement.FindFirst(TreeScope.Children, condDocument);
        //        //        if (docElement != null)
        //        //        {
        //        //            foreach (AutomationPattern pattern in docElement.GetSupportedPatterns())
        //        //            {
        //        //                if (docElement.GetCurrentPattern(pattern) is ValuePattern)
        //        //                {
        //        //                    //Console.WriteLine((docElement.GetCurrentPattern(pattern) as ValuePattern).Current.Value.ToString() + Environment.NewLine);
        //        //                    urls.Add((docElement.GetCurrentPattern(pattern) as ValuePattern).Current.Value.ToString());
        //        //                }
        //        //            }
        //        //        }
        //        //    }
        //        //}

        //        //string url;

        //        //Process[] process = Process.GetProcessesByName("firefox");

        //        //foreach (Process firefox in process)
        //        //{
        //        //     the chrome process must have a window
        //        //    if (firefox.MainWindowHandle == IntPtr.Zero)
        //        //    {
        //        //        return null;
        //        //    }

        //        //    AutomationElement element = AutomationElement.FromHandle(firefox.MainWindowHandle);
        //        //    if (element == null)
        //        //        return null;

        //        //    search for first custom element
        //        //    AutomationElement custom1 = element.FindFirst(TreeScope.Descendants,
        //        //     new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom));

        //        //    search all custom element children
        //        //    AutomationElementCollection custom2 = custom1.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom));

        //        //    for each custom child
        //        //    foreach (AutomationElement item in custom2)
        //        //    {
        //        //        search for first custom element
        //        //        AutomationElement custom3 = item.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom));
        //        //        search for first document element
        //        //        AutomationElement doc3 = custom3.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Document));



        //        //        if (!doc3.Current.IsOffscreen)
        //        //        {
        //        //            urls.Add( ((ValuePattern)doc3.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string);

        //        //        }
        //        //    }
        //        //}



        //        Process[] procsChrome = Process.GetProcessesByName("chrome");
        //        foreach (Process chrome in procsChrome)
        //        {
        //            // the chrome process must have a window
        //            if (chrome.MainWindowHandle == IntPtr.Zero)
        //            {
        //                continue;
        //            }

        //            // find the automation element
        //            AutomationElement elm = AutomationElement.FromHandle(chrome.MainWindowHandle);
        //            AutomationElement elmUrlBar = elm.FindFirst(TreeScope.Descendants,
        //              new PropertyCondition(AutomationElement.NameProperty, "Address and search bar"));

        //            // if it can be found, get the value from the URL bar
        //            if (elmUrlBar != null)
        //            {
        //                AutomationPattern[] patterns = elmUrlBar.GetSupportedPatterns();
        //                if (patterns.Length > 0)
        //                {
        //                    ValuePattern val = (ValuePattern)elmUrlBar.GetCurrentPattern(patterns[0]);
        //                    urls.Add(val.Current.Value);
        //                }
        //            }
        //        }

        //        UpdateActiveURLs(urls);


        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    //AutomationElement root = AutomationElement.RootElement.FindFirst(TreeScope.Children,new PropertyCondition(AutomationElement.ClassNameProperty, "MozillaWindowClass"));

        //    //Condition toolBar = new AndCondition( new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ToolBar), new PropertyCondition(AutomationElement.NameProperty, "Browser tabs"));

        //    //var tool = root.FindFirst(TreeScope.Children, toolBar);

        //    //var tool2 = TreeWalker.ControlViewWalker.GetNextSibling(tool);

        //    //var children = tool2.FindAll(TreeScope.Children, Condition.TrueCondition);

        //    //foreach (AutomationElement item in children)
        //    //{
        //    //    foreach (AutomationElement i in item.FindAll(TreeScope.Children, Condition.TrueCondition))
        //    //    {
        //    //        foreach (AutomationElement ii in i.FindAll(TreeScope.Children, Condition.TrueCondition))
        //    //        {
        //    //            if (ii.Current.LocalizedControlType == "document")
        //    //            {
        //    //                if (!(ii.Current.BoundingRectangle.X.ToString().Contains("Infinity")))
        //    //                {
        //    //                    ValuePattern activeTab = ii.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
        //    //                    var activeUrl = activeTab.Current.Value;
        //    //                    return activeUrl;
        //    //                }
        //    //            }
        //    //        }
        //    //    }
        //    //}

        //    return string.Empty;
        //}

        //private AutomationElement GetNextCustomControl(AutomationElement rootElement, Condition condCustomControl)
        //{
        //    return rootElement.FindAll(TreeScope.Children, condCustomControl).Cast<AutomationElement>().ToList().Where(x => x.Current.BoundingRectangle != System.Windows.Rect.Empty).FirstOrDefault();
        //}

        //private void UpdateActiveURLs(List<string> _URLs)
        //{
        //    string[] tempURLs = new string[] { };


        //    _URLs.CopyTo(tempURLs);
        //    List<string> tempURLList = new List<string>(tempURLs);

        //    DateTime updateTime = DateTime.Now;

        //    foreach (WebPage wp in pPages)
        //    {
        //        string url = tempURLList.FirstOrDefault(x => string.Compare(wp.URL, x) == 0);

        //        if (!string.IsNullOrEmpty(url))
        //        {
        //            wp.Activate(updateTime);
        //        }
        //        else
        //        {
        //            wp.Deactivate(updateTime);
        //        }

        //        tempURLList.Remove(url);

        //    }

        //    foreach (string url in tempURLList)
        //    {
        //        WebPage wp = new WebPage(url);
        //        wp.Activate(updateTime);
        //    }



        //}

        //private void WmiEventHandler(object sender, EventArrivedEventArgs e)
        //{
        //    int temp;

        //    if (int.TryParse(((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Handle"].ToString(), out temp))
        //    {
        //        IntPtr pr = new IntPtr(temp);
        //        ApplicationProcess ap = new ApplicationProcess(((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"].ToString(), pr);
        //        pApplications.Add(ap);
        //    }

        //}

        //private void WmiEventDeletionHandler(object sender, EventArrivedEventArgs e)
        //{
        //    int temp;

        //    if (int.TryParse(((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Handle"].ToString(), out temp))
        //    {
        //        IntPtr pr = new IntPtr(temp);
        //        ApplicationProcess ap = pApplications.FirstOrDefault(x => x.Handle == pr);

        //        if (ap != null)
        //        {
        //            ap.Deactivate();
        //        }

        //    }
        //}

        #endregion

    }
}
