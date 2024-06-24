using DotBoil.Configuration;

namespace DotBoil.Mediator
{
    internal class MediatorOptions : IOptions
    {
        public string Key => "DotBoil:Mediator";

        public List<string> Pipelines { get; set; }
    }
}
