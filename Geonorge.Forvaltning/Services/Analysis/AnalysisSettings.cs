namespace Geonorge.Forvaltning.Services;

public class AnalysisSettings
{
    public static readonly string SectionName = "Analysis";
    public List<AnalysisDataset> Datasets { get; set; }

    public class AnalysisDataset
    {
        public int Id { get; set; }
        public int AnalysisDatasetId { get; set; }
        public List<string> Organizations { get; set; }
    }
}
