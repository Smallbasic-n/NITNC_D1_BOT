using System.ComponentModel.DataAnnotations;

namespace NITNC_D1_Server.DataContext;

public class KiyomoriAssignment
{
    [Key]
    public int Id { get; set; }
    public string Detail { get; set; }
    public DateTime Deadline { get; set; }
    
    public int WorkId { get; set; }
    public KiyomoriWorking KiyomoriWorking { get; set; } = null!;
    public override string ToString()
    {
        return $"{KiyomoriWorking.KiyomoriSubject.SubjectName}, {KiyomoriWorking.WorkName}, {Detail}";
    }
}

public class KiyomoriSubject
{
    [Key]
    public int Id { get; set; }
    public string SubjectName { get; set; }
    public DayEnums Day { get; set; }
    
    public List<KiyomoriWorking> KiyomoriWorking { get; set; }=new List<KiyomoriWorking>();
    public override string ToString()
    {
        return SubjectName;
    }
}

public enum DayEnums
{
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
    Sunday = 7
}

public class KiyomoriWorking
{
    [Key]
    public int Id { get; set; }
    public string WorkName { get; set; }
    public int SubjectId { get; set; }
    public KiyomoriSubject KiyomoriSubject { get; set; }= null!;
    
    public List<KiyomoriAssignment> KiyomoriAssignments { get; set; }=new List<KiyomoriAssignment>();
    public override string ToString()
    {
        return KiyomoriSubject.SubjectName+", "+WorkName;
    }
}