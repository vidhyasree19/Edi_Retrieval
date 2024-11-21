using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public class CosmosItem
{
     [Key] 
    

    public string ContainerNumber { get; set; }
    public string TradeType { get; set; }
    public string Status { get; set; }
    public string VesselName { get; set; }
    public string VesselCode { get; set; }
    public string Voyage { get; set; }
    public string Origin { get; set; }
    public string Line { get; set; }
    public string Destination { get; set; }
    public string SizeType { get; set; }
}
