namespace Geonorge.Forvaltning.Models.Api.Messaging;

public class PointerMoved : UserMessage
{
    public double[] Coordinate { get; set; }
}
