public class EmailConfiguration
{
    public static string SectionName => "Email";
    public string SmtpHost { get; set; }
    public string WebmasterEmail { get; set; }
}