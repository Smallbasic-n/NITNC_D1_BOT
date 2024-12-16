using System.ComponentModel.DataAnnotations;

namespace NITNC_D1_Server.DataContext;

public class MatsudairaRoles
{
    [Key]
    public int Id { get; set; }
    public ulong RoleId { get; set; }
}