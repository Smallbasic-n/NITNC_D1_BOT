using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

namespace ImagawaYoshimoto.Data;

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