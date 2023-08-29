public class DbTestConfiguration
{

    public static string SectionName => "Database";
    public string SUPABASE_URL { get; set; }
    public string SUPABASE_KEY { get; set; }
    public string ConnectionString { get; set; }
}