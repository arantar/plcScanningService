using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceProcess;
using System.Xml;

public class Service1 : ServiceBase
{
    private List<PLCPoller> plcs;
    private IContainer components;

    //public Service1() {
    //    InitializeComponent();
    //}

    //public void OnDebug() {
    //    OnStart(null);
    //}

    protected override void OnStart(string[] args) {
        EventLogTraceListener listener = new EventLogTraceListener("Scanner");
        Trace.Listeners.Add(listener);
        plcs = new List<PLCPoller>();
        string text = "";
        string text2 = "";
        string srlzpath = "";
        int num = 0;
        int labelslimit = 0;
        string in_path = "";
        XmlDocument xmlDocument = new XmlDocument();
        object value = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Scanner", "config", null);
        if (value != null) {
            xmlDocument.Load(value.ToString());
        }
        else {
            xmlDocument.Load("C:\\temp\\config.xml");
        }
        foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes) {
            if (childNode.Name == "settings") {
                foreach (XmlNode item in childNode) {
                    if (item.Name == "serpath") {
                        srlzpath = item.InnerText;
                    }
                    if (item.Name == "maxlabels") {
                        labelslimit = int.Parse(item.InnerText);
                    }
                    if (item.Name == "path") {
                        in_path = item.InnerText;
                    }
                }
            }
            if (childNode.Name == "node") {
                foreach (XmlNode item2 in childNode) {
                    if (item2.Name == "ip") {
                        text = item2.InnerText;
                    }
                    if (item2.Name == "name") {
                        text2 = item2.InnerText;
                    }
                    if (item2.Name == "polltime") {
                        num = int.Parse(item2.InnerText);
                    }
                }
                if (text != "" && text2 != "" && num != 0) {
                    plcs.Add(new PLCPoller(text, num, text2, in_path, srlzpath, labelslimit));
                }
                text = "";
                text2 = "";
                num = 0;
            }
        }
    }

    protected override void OnStop() {
        foreach (PLCPoller plc in plcs) {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            Stream stream = new FileStream(plc.serialization_path + "\\" + plc.name + ".save", FileMode.Create, FileAccess.Write, FileShare.None);
            ((IFormatter)binaryFormatter).Serialize(stream, (object)plc.labels);
            stream.Close();
        }
    }

    protected override void Dispose(bool disposing) {
        if (disposing && components != null) {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent() {
        components = new Container();
        base.ServiceName = "Service1";
    }
}
