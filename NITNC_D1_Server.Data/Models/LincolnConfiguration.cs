using System.ComponentModel.DataAnnotations;

namespace NITNC_D1_Server.Data.Models;

public class LincolnConfiguration
{
    [Key]
    public int Id { get; set; }
    public int RangeStep { get; set; }
    public int RangeStart { get; set; }
    public int RangeEnds { get; set; }
    public ulong ChankAnswered { get; set; }
    public ulong ChankSolved { get; set; }
    public ulong ChankIssued { get; set; }
    public ulong FactbookAnswered { get; set; }
    public ulong FactbookSolved { get; set; }
    public ulong FactbookIssued { get; set; }
}