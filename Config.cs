using Exiled.API.Interfaces;

namespace SCPStats
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool ExcludeDNTUsers { get; set; } = true;
        public bool ShowDeveloperBadge { get; set; } = true;
    }
}
