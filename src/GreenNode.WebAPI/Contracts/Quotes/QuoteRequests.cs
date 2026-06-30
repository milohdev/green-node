namespace GreenNode.WebAPI.Contracts.Quotes;

public record CreateQuoteRequest(Guid TerritoryId, string Sector, int NumberOfTrees);

public record RejectQuoteRequest(string Reason);
