using System.ComponentModel.DataAnnotations;

namespace NITNC_D1_Server.DataContext;

public class KiyomoriSchedule
{
    [Key]
    public int Id { get; set; }
    public string SubjectName { get; set; }
    public DateTime Date { get; set; }
    public int StartHour { get; set; }
    public int EndHour { get; set; }
    public bool IsAlerted { get; set; }
}