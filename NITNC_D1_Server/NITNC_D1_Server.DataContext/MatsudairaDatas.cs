using SoftFluent.ComponentModel.DataAnnotations;

namespace NITNC_D1_Server.DataContext;

public class MatsudairaDatas
{
    public ulong Id { get; set; }
    public ulong AccountId { get; set; }
    public ulong Chank { get; set; }
    public ulong FactBook { get; set; }
    [Encrypted]
    public string FirstName { get; set; }
    [Encrypted]
    public string GivenName { get; set; }
    [Encrypted]
    public string Email { get; set; }
}