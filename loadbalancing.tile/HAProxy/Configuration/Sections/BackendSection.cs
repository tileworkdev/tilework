namespace Tilework.LoadBalancing.Haproxy;

public class BackendSection : ConfigSection
{
    [Statement("mode")]
    public Mode Mode { get; set; }

    [Statement("balance")]
    public BalanceMode Balance { get; set; }

    [Statement("server")]
    public List<Server> Servers { get; set; }

    public BackendSection() : base("backend")
    {
    }
}