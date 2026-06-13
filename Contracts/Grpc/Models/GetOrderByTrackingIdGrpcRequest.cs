using ProtoBuf;

namespace Contracts.Grpc.Models;

[ProtoContract]
public class GetOrderByTrackingIdGrpcRequest
{
    [ProtoMember(1)]
    public Guid TrackingId { get; set; }
}
