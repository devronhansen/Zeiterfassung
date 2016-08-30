using System.Windows.Forms;
using Phidgets; //Needed for the RFID class and the PhidgetException class
using Phidgets.Events; //Needed for the phidget event handling classes
using System;
using System.Drawing;
using System.Text;
//todo ausgetragen_am soll immer überschrieben werden wenn es ein eingetragen_am am selben tag gibt, konsole oder wat?
namespace Zeiterfassung
{
    public partial class Form1 : Form
    {
        RFID rfid; //Declare an RFID object

        public Form1()
        {
            InitializeComponent();
            this.Bounds = new Rectangle(this.Location, new Size(298, 433));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            rfid = new RFID();

            rfid.Attach += new AttachEventHandler(rfid_Attach);
            rfid.Detach += new DetachEventHandler(rfid_Detach);

            rfid.Tag += new TagEventHandler(rfid_Tag);
            rfid.TagLost += new TagEventHandler(rfid_TagLost);

            //foreach (String proto in System.Enum.GetNames(typeof(RFID.RFIDTagProtocol)))
            //{
            //    writeProtoCmb.Items.Add(proto);
            //}
            //writeProtoCmb.SelectedIndex = 0;

            //Disabled controls until Phidget is attached
            //antennaChk.Enabled = false;
            //ledChk.Enabled = false;
            //output0Chk.Enabled = false;
            //output1chk.Enabled = false;

            openCmdLine(rfid);
        }

        void rfid_Tag(object sender, TagEventArgs e)
        {
            rfid.LED = false;
            rfid.outputs[0] = true;
            System.Threading.Thread.Sleep(500);
            rfid.outputs[0] = false;
        }

        //attach event handler..populate the details fields as well as display the attached status.  enable the checkboxes to change
        //the values of the attributes of the RFID reader such as enable or disable the antenna and onboard led.
        void rfid_Attach(object sender, AttachEventArgs e)
        {
            RFID attached = (RFID)sender;
            //attachedTxt.Text = e.Device.Attached.ToString();
            //attachedTxt.Text = attached.Attached.ToString();
            //nameTxt.Text = attached.Name;
            //serialTxt.Text = attached.SerialNumber.ToString();
            //versionTxt.Text = attached.Version.ToString();
            //outputsTxt.Text = attached.outputs.Count.ToString();

            switch (attached.ID)
            {
                case Phidget.PhidgetID.RFID_2OUTPUT_READ_WRITE:
                    this.Bounds = new Rectangle(this.Location, new Size(298, 545));
                    break;
                case Phidget.PhidgetID.RFID:
                case Phidget.PhidgetID.RFID_2OUTPUT:
                default:
                    this.Bounds = new Rectangle(this.Location, new Size(298, 433));
                    break;
            }

            //if (rfid.outputs.Count > 0)
            //{
            //    antennaChk.Checked = true;
            //    rfid.Antenna = true;
            //    antennaChk.Enabled = true;
            //    ledChk.Enabled = true;
            //    output0Chk.Enabled = true;
            //    output1chk.Enabled = true;
            //}
        }

        void rfid_TagLost(object sender, TagEventArgs e)
        {
            connectToDB db = new connectToDB();
            db.updateDB(e.Tag, DateTime.Now);
            rfid.LED = false;
            rfid.outputs[0] = false;
            rfid.outputs[1] = true;
        }

        void rfid_Detach(object sender, DetachEventArgs e)
        {
            RFID detached = (RFID)sender;
            //attachedTxt.Text = detached.Attached.ToString();
            //nameTxt.Text = "";
            //serialTxt.Text = "";
            //versionTxt.Text = "";
            //outputsTxt.Text = "";

            this.Bounds = new Rectangle(this.Location, new Size(298, 433));
            //writeBox.Visible = false;

            //if (rfid.outputs.Count > 0)
            //{
            //    antennaChk.Enabled = false;
            //    ledChk.Enabled = false;
            //    output0Chk.Enabled = false;
            //    output1chk.Enabled = false;
            //}
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            rfid.Attach -= new AttachEventHandler(rfid_Attach);
            rfid.Detach -= new DetachEventHandler(rfid_Detach);
            rfid.Tag -= new TagEventHandler(rfid_Tag);
            rfid.TagLost -= new TagEventHandler(rfid_TagLost);

            //run any events in the message queue - otherwise close will hang if there are any outstanding events
            Application.DoEvents();

            rfid.close();
        }

        #region Command line open functions
        private void openCmdLine(Phidget p)
        {
            openCmdLine(p, null);
        }
        private void openCmdLine(Phidget p, String pass)
        {
            int serial = -1;
            String logFile = null;
            int port = 5001;
            String host = null;
            bool remote = false, remoteIP = false;
            string[] args = Environment.GetCommandLineArgs();
            String appName = args[0];

            try
            { //Parse the flags
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i].StartsWith("-"))
                        switch (args[i].Remove(0, 1).ToLower())
                        {
                            case "l":
                                logFile = (args[++i]);
                                break;
                            case "n":
                                serial = int.Parse(args[++i]);
                                break;
                            case "r":
                                remote = true;
                                break;
                            case "s":
                                remote = true;
                                host = args[++i];
                                break;
                            case "p":
                                pass = args[++i];
                                break;
                            case "i":
                                remoteIP = true;
                                host = args[++i];
                                if (host.Contains(":"))
                                {
                                    port = int.Parse(host.Split(':')[1]);
                                    host = host.Split(':')[0];
                                }
                                break;
                            default:
                                goto usage;
                        }
                    else
                        goto usage;
                }
                if (logFile != null)
                    Phidget.enableLogging(Phidget.LogLevel.PHIDGET_LOG_INFO, logFile);
                if (remoteIP)
                    p.open(serial, host, port, pass);
                else if (remote)
                    p.open(serial, host, pass);
                else
                    p.open(serial);
                return; //success
            }
            catch { }
        usage:
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Invalid Command line arguments." + Environment.NewLine);
            sb.AppendLine("Usage: " + appName + " [Flags...]");
            sb.AppendLine("Flags:\t-n   serialNumber\tSerial Number, omit for any serial");
            sb.AppendLine("\t-l   logFile\tEnable phidget21 logging to logFile.");
            sb.AppendLine("\t-r\t\tOpen remotely");
            sb.AppendLine("\t-s   serverID\tServer ID, omit for any server");
            sb.AppendLine("\t-i   ipAddress:port\tIp Address and Port. Port is optional, defaults to 5001");
            sb.AppendLine("\t-p   password\tPassword, omit for no password" + Environment.NewLine);
            sb.AppendLine("Examples: ");
            sb.AppendLine(appName + " -n 50098");
            sb.AppendLine(appName + " -r");
            sb.AppendLine(appName + " -s myphidgetserver");
            sb.AppendLine(appName + " -n 45670 -i 127.0.0.1:5001 -p paswrd");
            MessageBox.Show(sb.ToString(), "Argument Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            Application.Exit();
        }
        #endregion
    }
}
