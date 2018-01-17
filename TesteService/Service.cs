using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;
using System.Text;

namespace TesteService
{
    public partial class ServidorDados_Service : ServiceBase
    {
        /// <summary>
        /// The Main Thread: This is where your Service is Run.
        /// </summary>
        static void Main(string[] args)
        {
            Configuration.LoadFromDiskOrCreate();
            if (System.Environment.UserInteractive)
            {
                foreach (var v in args)
                {
                    switch (v.ToLower())
                    {
                        case "-h":
                        case "/h":
                        case "-help":
                        case "--help":
                            help();
                            return;
                        case "-c":
                        case "/c":
                        case "config":
                            Configuration.Reset();
                            Configuration.SaveToDisk();
                            System.Windows.Forms.MessageBox.Show("The configuration file was reseted");
                            return;

                        case "-r":
                        case "/r":
                        case "-run":
                        case "--run":
                            var svc = new ServidorDados_Service();
                            svc.doStuff();
                            return;

                        case "/i":
                        case "-i":
                        case "instalar":
                        case "install":
                            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                            System.Windows.Forms.MessageBox.Show("Serviço instalado.");
                            return;
                        case "/u":
                        case "-u":
                        case "desinstalar":
                        case "uninstall":
                            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                            System.Windows.Forms.MessageBox.Show("Serviço desinstalado.");
                            return;
                    }
                }

                System.Windows.Forms.MessageBox.Show("Inicialização incorreta.");
                return;
            }
            else
            {
                ServiceBase.Run(new ServidorDados_Service());
            }
        }
        static void help()
        {
            System.Windows.Forms.MessageBox.Show(@"Parmas:
-h   Help
-r   Run
-c   Restet and create a new config file
-i   Install service
-u   Unistall service");
        }
        //========================================================//
        //Thread thdReinicio;
        private string[] args;

        System.Threading.Thread thdDoStuff;

        /// <summary>
        /// Public Constructor for WindowsService.
        /// - Put all of your Initialization code here.
        /// </summary>
        public ServidorDados_Service()
        {
            this.ServiceName = Configuration.Instance.SerivceName;

            this.EventLog.Log = "Application";
            // These Flags set whether or not to handle that specific
            //  type of event. Set to true if you need it, false otherwise.
            this.CanHandlePowerEvent = true;
            this.CanHandleSessionChangeEvent = true;
            this.CanShutdown = true;
            this.CanStop = true;
            this.CanPauseAndContinue = false;
        }

        /// <summary>
        /// Dispose of objects that need it here.
        /// </summary>
        /// <param name="disposing">Whether
        ///    or not disposing is going on.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        /// OnStart(): Put startup code here
        ///  - Start threads, get inital data, etc.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            Start();
            base.OnStart(args);
        }
        private void Start()
        {
            thdDoStuff = new System.Threading.Thread(doStuff);
            thdDoStuff.SetApartmentState(System.Threading.ApartmentState.MTA);
            thdDoStuff.Start();
        }
        bool running = true;
        private void doStuff()
        {
            string killFile = Helper.GetCurrDir() + "\\delete-me to kill.txt";
            if (Configuration.Instance.RunProgramName.Length > 0)
            {
                try
                {
                    System.IO.File.WriteAllText(killFile, "Delete-me to kill the process. It will try to gracefully stop for 5 seconds");
                }
                catch { }
                try
                {
                    if (!System.IO.File.Exists(killFile)) killFile = null;
                }
                catch { }
                using (var outputStream = System.IO.File.AppendText(Helper.GetCurrDir() + "\\output.txt"))
                {
                    outputStream.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + " Initializing...");
                    outputStream.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + " Program: " + Configuration.Instance.RunProgramName);
                    outputStream.Flush();

                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    psi.Arguments = Configuration.Instance.RunProgramParams;
                    psi.FileName = Configuration.Instance.RunProgramName;
                    psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    if (Configuration.Instance.RunProgramPath != null && Configuration.Instance.RunProgramPath.Length > 0)
                    {
                        psi.WorkingDirectory = Configuration.Instance.RunProgramPath;
                    }
                    if (Configuration.Instance.GracefullyCloseCommand != null && Configuration.Instance.GracefullyCloseCommand.Length > 0)
                    {
                        psi.RedirectStandardInput = true;
                    }
                    psi.RedirectStandardOutput = true;

                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo = psi;
                    p.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler((sender, e) =>
                    {
                        // Prepend line numbers to each line of the output.
                        if (!String.IsNullOrEmpty(e.Data))
                        {
                            outputStream.WriteLine(e.Data);
                            outputStream.Flush();
                        }
                    });
                    p.Start();
                    running = true;
                    p.BeginOutputReadLine();
                    while (running || p.HasExited)
                    {
                        System.Threading.Thread.Sleep(250);
                        if (killFile != null)
                        {
                            if (!System.IO.File.Exists(killFile))
                            {
                                outputStream.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + " ==Kill by file");
                                break;
                            }
                        }
                    }

                    if (!p.HasExited && Configuration.Instance.GracefullyCloseCommand != null && Configuration.Instance.GracefullyCloseCommand.Length > 0)
                    {
                        try
                        {
                            p.StandardInput.WriteLine(Configuration.Instance.GracefullyCloseCommand);
                            outputStream.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + " ==Stop signal sent");                            
                        }
                        catch (Exception ex) { outputStream.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + " ==Stop signal FAILED to sent: " + ex.Message); }

                        int seconds = Configuration.Instance.GracefullyCloseTimeoutSeconds;
                        if (seconds < 0) seconds = 5;
                        if (seconds > 30) seconds = 30;
                        for (int i = 0; i < seconds * 10; i++)
                        {
                            System.Threading.Thread.Sleep(100);
                            if (p.HasExited) break;
                        }
                    }

                    if (!p.HasExited)
                    {
                        p.Kill();
                    }
                    p.Close();
                    outputStream.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + " ==END");
                }
            }
        }
        /// <summary>
        /// OnStop(): Put your stop code here
        /// - Stop threads, set final data, etc.
        /// </summary>
        protected override void OnStop()
        {
            running = false;
            base.OnStop();
        }

        /// <summary>
        /// OnShutdown(): Called when the System is shutting down
        /// - Put code here when you need special handling
        ///   of code that deals with a system shutdown, such
        ///   as saving special data before shutdown.
        /// </summary>
        protected override void OnShutdown()
        {
            running = false;
            base.OnShutdown();
        }

        /// <summary>
        /// OnCustomCommand(): If you need to send a command to your
        ///   service without the need for Remoting or Sockets, use
        ///   this method to do custom methods.
        /// </summary>
        /// <param name="command">Arbitrary Integer between 128 & 256</param>
        protected override void OnCustomCommand(int command)
        {
            //  A custom command can be sent to a service by using this method:
            //#  int command = 128; //Some Arbitrary number between 128 & 256
            //#  ServiceController sc = new ServiceController("NameOfService");
            //#  sc.ExecuteCommand(command);
            base.OnCustomCommand(command);
        }

        /// <summary>
        /// OnPowerEvent(): Useful for detecting power status changes,
        ///   such as going into Suspend mode or Low Battery for laptops.
        /// </summary>
        /// <param name="powerStatus">The Power Broadcast Status
        /// (BatteryLow, Suspend, etc.)</param>
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return base.OnPowerEvent(powerStatus);
        }

        /// <summary>
        /// OnSessionChange(): To handle a change event
        ///   from a Terminal Server session.
        ///   Useful if you need to determine
        ///   when a user logs in remotely or logs off,
        ///   or when someone logs into the console.
        /// </summary>
        /// <param name="changeDescription">The Session Change
        /// Event that occured.</param>
        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            base.OnSessionChange(changeDescription);
        }
    }
}