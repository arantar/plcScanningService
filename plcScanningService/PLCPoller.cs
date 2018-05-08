#define TRACE
using Microsoft.VisualBasic.FileIO;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

internal class PLCPoller
{
    private string ipaddr;
    private int polltime;
    private int maxlabels;
    public string name;
    private string path;
    public string serialization_path;
    private Timer stateTimer;
    public LabelData labels;
    private bool busy;

    public PLCPoller(string in_ipaddr, int in_polltime, string in_name, string in_path, string srlzpath, int labelslimit) {
        ipaddr = in_ipaddr;
        polltime = in_polltime;
        name = in_name;
        path = in_path;
        maxlabels = labelslimit;
        serialization_path = srlzpath;
        busy = false;
        if (File.Exists(serialization_path + "\\" + name + ".save")) {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(serialization_path + "\\" + name + ".save", FileMode.Open, FileAccess.Read, FileShare.Read);
            try {
                labels = (LabelData)formatter.Deserialize(stream);
                stream.Close();
            }
            catch {
                labels = new LabelData();
                labels.name = name;
            }
        }
        else {
            labels = new LabelData();
            labels.name = name;
        }
        stateTimer = new Timer(TmrCallback, null, 0, polltime);
    }

    public void TmrCallback(object StateInfo) {
        if (!busy) {
            Poll();
        }
    }

    public bool Poll() {
        busy = true;
        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://" + ipaddr + "/FileBrowser/Download?Path=/DataLogs/log.csv");
        httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;
        httpWebRequest.Referer = "http://" + ipaddr + "/Portal/Portal.mwsl?PriNav=FileBrowser&Path=/DataLogs/";
        try {
            Stream responseStream = httpWebRequest.GetResponse().GetResponseStream();
            if (responseStream == null) {
                Trace.WriteLine("PLC " + name + ": unable to read from the webserver");
                busy = false;
                return false;
            }
            MemoryStream memoryStream = new MemoryStream();
            byte[] array = new byte[4096];
            int count;
            while ((count = responseStream.Read(array, 0, array.Length)) > 0) {
                memoryStream.Write(array, 0, count);
            }
            memoryStream.Position = 0L;
            ParseData(memoryStream);
            responseStream.Close();
        }
        catch (Exception ex) {
            busy = false;
            Trace.WriteLine("PLC " + name + ":" + ex.Message);
            return false;
        }
        busy = false;
        return true;
    }

    public bool ParseData(Stream data) {
        long num = 0L;
        string[] formats = new string[4]
        {
            "MM/dd/yyyy H:mm:ss",
            "M/dd/yyyy H:mm:ss",
            "MM/dd/yyyy HH:mm:ss",
            "M/dd/yyyy HH:mm:ss"
        };
        TextFieldParser textFieldParser = new TextFieldParser(data);
        textFieldParser.TextFieldType = FieldType.Delimited;
        textFieldParser.SetDelimiters(",");
        textFieldParser.TrimWhiteSpace = true;
        Directory.CreateDirectory(path + "\\" + name);
        try {
            DateTime dateTime;
            while (!textFieldParser.EndOfData) {
                string[] array = textFieldParser.ReadFields();
                bool num2 = DateTime.TryParseExact(array[1] + " " + array[2], formats, CultureInfo.InvariantCulture, DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite | DateTimeStyles.AllowInnerWhite | DateTimeStyles.AssumeUniversal, out dateTime) & long.TryParse(array[3], out num);
                dateTime.ToLocalTime();
                string text = dateTime.Date.ToString("yyyyMMdd");
                string text2 = dateTime.TimeOfDay.ToString("hhmmss");
                if (num2) {
                    int num3 = labels.data.IndexOfKey(num);
                    DateTime t = (num3 <= -1) ? dateTime : ((DateTime)labels.data.GetByIndex(num3));
                    if (!labels.data.ContainsKey(num)) {
                        labels.add(dateTime, num);
                        if (num == 1) {
                            StreamWriter streamWriter = File.CreateText(path + "\\" + name + "\\PN@_PC@000001_GR@01_DA@" + text + "_TI@" + text2 + ".csv");
                            streamWriter.Write("PN@_PC@000001_GR@01_DA@" + text + "_TI@" + text2);
                            streamWriter.Close();
                        }
                        else {
                            StreamWriter streamWriter2 = File.CreateText(path + "\\" + name + "\\PN@" + num + "_PC@000001_GR@01_DA@" + text + "_TI@" + text2 + ".csv");
                            streamWriter2.Write("PN@" + num + "_PC@000001_GR@01_DA@" + text + "_TI@" + text2);
                            streamWriter2.Close();
                        }
                    }
                    if (labels.data.ContainsKey(num) && dateTime > t) {
                        labels.data.SetByIndex(labels.data.IndexOfKey(num), dateTime);
                        if (num == 1) {
                            StreamWriter streamWriter3 = File.CreateText(path + "\\" + name + "\\PN@_PC@000001_GR@01_DA@" + text + "_TI@" + text2 + ".csv");
                            streamWriter3.Write("PN@_PC@000001_GR@01_DA@" + text + "_TI@" + text2);
                            streamWriter3.Close();
                        }
                        else {
                            StreamWriter streamWriter4 = File.CreateText(path + "\\" + name + "\\PN@" + num + "_PC@000001_GR@01_DA@" + text + "_TI@" + text2 + ".csv");
                            streamWriter4.Write("PN@" + num + "_PC@000001_GR@01_DA@" + text + "_TI@" + text2);
                            streamWriter4.Close();
                        }
                    }
                }
            }
            data.Position = 0L;
            TextFieldParser textFieldParser2 = new TextFieldParser(data);
            textFieldParser2.TextFieldType = FieldType.Delimited;
            textFieldParser2.SetDelimiters(",");
            textFieldParser2.TrimWhiteSpace = true;
            if (labels.data.Count > maxlabels) {
                labels.data.Clear();
                while (!textFieldParser2.EndOfData) {
                    string[] array2 = textFieldParser2.ReadFields();
                    if (DateTime.TryParseExact(array2[1] + " " + array2[2], formats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dateTime) & long.TryParse(array2[3], out num)) {
                        labels.add(dateTime, num);
                    }
                }
            }
        }
        catch (Exception ex) {
            Trace.WriteLine("PLC " + name + ":" + ex.Message);
        }
        return true;
    }
}
