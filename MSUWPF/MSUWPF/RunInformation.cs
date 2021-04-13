using System;
using System.Collections.Generic;
using System.Text;

namespace MSUWPF
{
    class HoraroJSONResponse
    {
        public HoraroJSONMeta meta { get; set; }
        public HoraroJSONSchedule schedule { get; set; }
    } 

    class HoraroJSONMeta
    {
        public string exported { get; set; }
        public string hint { get; set; }
        public string api { get; set; }
        public string apiLink { get; set; }
    }

    class HoraroJSONSchedule
    {
        public string name { get; set; }
        public string slug { get; set; }
        public string timezone { get; set; }
        public string start { get; set; }
        public int start_t { get; set; }
        public string website { get; set; }
        public string twitter { get; set; }
        public string twitch { get; set; }
        public string description { get; set; }
        public string setup { get; set; }
        public int setup_t { get; set; }
        public string updated { get; set; }
        public string url { get; set; }
        public string[] eventH { get; set; }
        public string[] hiddenColumns { get; set; }
        public string[] columns { get; set; }
        public HoraroItem[] items { get; set; }
    }

    class HoraroItem
    {
        public string length { get; set; }
        public int length_t { get; set; }
        public string scheduled { get; set; }
        public int scheduled_t { get; set; }
        public string[] data { get; set; }
    }

    class RunInformation
    {
        public string GameName { get; set; }
        public string Category { get; set; }
        public string Platform { get; set; }
        public string RunnerName { get; set; }
        public string RunnerPronoun { get; set; }
        public string Host { get; set; }
        public string HostPronoun { get; set; }
        public string Incentives { get; set; }
    }
}
