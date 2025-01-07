using System.ComponentModel.DataAnnotations;

namespace NITNC_D1_Server.Data.Models;

public class MatsudairaRoles
{
    [Key]
    public int Id { get; set; }
    public ulong DiscordRoleId { get; set; }
}