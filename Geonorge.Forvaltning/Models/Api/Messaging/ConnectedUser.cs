﻿namespace Geonorge.Forvaltning.Models.Api.Messaging;

public class ConnectedUser
{
    public string ConnectionId { get; set; }
    public string Username { get; set; }
    public int DatasetId { get; set; }
    public double[] Coordinate { get; set; }
    public string Color { get; set; }
}
