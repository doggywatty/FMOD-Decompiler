using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankToFSPro;

public class EventFolder
{
    // used in main
    public static List<string> AllEvents = new List<string> { };

    public static void ExtractEventFolders() 
    {
        // figure out all subfolders from an event path and make an XML for each subfolder
        // aka event:/music/soundtest/pause
        //            ^folder ^folder  ^event (ignore event)


    }
}
