using System;

namespace Shadowsocks.Util.Capturers
{
    public class Source
    {
        public string Url { get;set; }
        public string Selector { get; set; }
        public string IpSelector { get; set; }
        public string PwdSelector { get; set; }
        public string PortSelector { get; set; }
        public string MethodSelector { get; set; }
    }
}
