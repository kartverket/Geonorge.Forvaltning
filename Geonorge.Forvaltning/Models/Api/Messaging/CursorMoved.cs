namespace Geonorge.Forvaltning.Models.Api.Messaging;

public class CursorMoved : Message
{
    public double[] Coordinate { get; set; }
}
