using DotBoil.Configuration;

namespace DotBoil.Caching.Configuration.Redis
{
    internal class RedisConfiguration : IOptions
    {
        public string Key => "DotBoil:Caching:Redis";

        public List<RedisEndpoint> Endpoints { get; set; }

        public string Password { get; set; }

        public RedisConfiguration()
        {
            Endpoints = new List<RedisEndpoint>();
        }
    }

    internal class RedisEndpoint
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
    }
}
