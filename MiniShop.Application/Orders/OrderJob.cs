namespace MiniShop.Application.Orders;

/// <summary>Jedinica posla za pozadinsku obradu: samo Id narudzbenice.</summary>
public sealed record OrderJob(int OrderId);