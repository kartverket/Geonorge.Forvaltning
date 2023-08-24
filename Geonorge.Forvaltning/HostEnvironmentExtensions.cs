namespace Geonorge.Forvaltning
{
    public static class HostEnvironmentExtensions
    {
        public const string LocalEnvironment = "Local";

        public static bool IsLocal(this IHostEnvironment hostEnvironment)
        {
            return hostEnvironment.IsEnvironment(LocalEnvironment);
        }
    }
}
