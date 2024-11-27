namespace Geonorge.Forvaltning.Models.Api;

public class ConnectedUser
{
    public string ConnectionId { get; set; }
    public string Username { get; set; }
    public double[] Coordinate { get; set; }
    public int? DatasetId { get; set; }
    public int? ObjectId { get; set; }
}
