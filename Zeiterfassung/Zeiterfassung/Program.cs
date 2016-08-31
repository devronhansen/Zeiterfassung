using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phidgets; //Needed for the RFID class and the PhidgetException class
using Phidgets.Events; //Needed for the phidget event handling classes
using System.Drawing;


namespace zeiterfassung
{
    class Program
    {
        static RFID rfid;

        static void Main(string[] args)
        {
            try
            {
                rfid = new RFID(); //Declare an RFID object
                //initialize our Phidgets RFID reader and hook the event handlers
                //rfid.Attach += new AttachEventHandler(rfid_Attach);
                //rfid.Detach += new DetachEventHandler(rfid_Detach);
                //rfid.Error += new ErrorEventHandler(rfid_Error);
                //rfid.Attach += new AttachEventHandler(rfid_Attach);
                rfid.Tag += new TagEventHandler(rfid_Tag);
                rfid.TagLost += new TagEventHandler(rfid_TagLost);
                rfid.open();

                //Wait for a Phidget RFID to be attached before doing anything with 
                //the object
                Console.WriteLine("waiting for attachment...");
                rfid.waitForAttachment();

                //turn on the antenna and the led to show everything is working
                rfid.Antenna = true;
                rfid.LED = true;

                //keep waiting and outputting events until keyboard input is entered
                Console.WriteLine("Press any key to end...");
                Console.Read();

                //turn off the led
                rfid.LED = false;

                //close the phidget and dispose of the object
                rfid.close();
                rfid = null;
                Console.WriteLine("ok");
            }
            catch (PhidgetException ex)
            {
                Console.WriteLine(ex.Description);
            }
        }

        static void rfid_TagLost(object sender, TagEventArgs e)
        {
            connectToDB db = new connectToDB();
            db.updateDB(e.Tag, DateTime.Now);
            rfid.LED = false;
            rfid.outputs[0] = false;
            rfid.outputs[1] = true;
        }

        static void rfid_Tag(object sender, TagEventArgs e)
        {
            rfid.LED = false;
            rfid.outputs[0] = true;
            System.Threading.Thread.Sleep(500);
            rfid.outputs[0] = false;
        }
    }
}
