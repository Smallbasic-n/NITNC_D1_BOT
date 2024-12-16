using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

namespace NITNC_D1_Server.DataContext;

public class FactbookQuestions
{
    [Key]
    [Name("Id")]
    public int Id { get; set; }
    [Name("Japanese")]
    public string Japanese { get; set; }
    [Name("Answer")]
    public string Answer { get; set; }
    
}