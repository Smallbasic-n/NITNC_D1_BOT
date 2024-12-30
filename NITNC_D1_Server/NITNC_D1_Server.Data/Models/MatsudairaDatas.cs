using System.ComponentModel.DataAnnotations;
using SoftFluent.ComponentModel.DataAnnotations;

namespace NITNC_D1_Server.Data.Models;

public class MatsudairaDatas
{
    [Key]
    public int Id { get; set; }
    public ulong AccountId { get; set; }
    public ulong Chank { get; set; }
    public ulong FactBook { get; set; }
    [Encrypted]
    public string FirstName { get; set; }
    [Encrypted]
    public string GivenName { get; set; }
    [Encrypted]
    public string RohmeFirstName { get; set; }
    [Encrypted]
    public string RohmeGivenName { get; set; }
    
    public string Email { get; set; }
}