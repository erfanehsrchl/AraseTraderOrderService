using Contracts.Enums;
using ProtoBuf;

namespace Contracts.Grpc.Models;

[ProtoContract]
public class GetOrderByTrackingIdGrpcResponse
{
    [ProtoMember(1)]
    public Guid TrackingId { get; set; }

    [ProtoMember(2)]
    public long CustomerId { get; set; }

    [ProtoMember(3)]
    public OrderSideContract Side { get; set; }

    [ProtoMember(4)]
    public decimal Amount { get; set; }

    [ProtoMember(5)]
    public OrderStatusContract Status { get; set; }

    [ProtoMember(6)]
    public string? FailureReason { get; set; }

    [ProtoMember(7)]
    public DateTime CreatedAt { get; set; }

    [ProtoMember(8)]
    public DateTime UpdatedAt { get; set; }
}
