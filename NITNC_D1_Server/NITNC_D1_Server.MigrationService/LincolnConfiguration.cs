using System.ComponentModel.DataAnnotations;

namespace NITNC_D1_Server.DataContext;

public class LincolnConfiguration
{
    [Key]
    public int Id { get; set; }
    public int RangeStep { get; set; }
    public int RangeStart { get; set; }
    public int RangeEnds { get; set; }
}