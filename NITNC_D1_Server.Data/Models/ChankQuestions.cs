using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

namespace NITNC_D1_Server.Data.Models;

public class ChankQuestions
{
    [Key]
    [Name("Id")]
    public int Id { get; set; }
    [Name("Japanese")]
    public string Japanese { get; set; }
    [Name("English")]
    public string English { get; set; }
    [Name("Answer")]
    public string Answer { get; set; }
    public int Step { get; set; }
}