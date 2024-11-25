using Newtonsoft.Json;


public class CosmosItem
{
    [JsonProperty("id")]  // This is required by Cosmos DB
    public string Id { get; set; }

    [JsonProperty("containerNumber")]  // This is the partition key
    public string ContainerNumber { get; set; }

    public string TradeType { get; set; }
    public string Status { get; set; }
    public string VesselName { get; set; }
    public string VesselCode { get; set; }
    public string Voyage { get; set; }
    public string Origin { get; set; }
    public string Destination { get; set; }
    public string Line { get; set; }
    public string SizeType { get; set; }
    public decimal Fees { get; set; }
    public bool CartAdded { get; set; }  // New field to track cart addition

}
