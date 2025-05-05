using Exiled.API.Interfaces;

namespace SCPStats
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = true;
        public bool ExcludeDNTUsers { get; set; } = true;
        public string ServerId { get; set; } = "1";
    }
}
