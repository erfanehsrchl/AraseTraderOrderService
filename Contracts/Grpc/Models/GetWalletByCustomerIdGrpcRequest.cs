using ProtoBuf;

namespace Contracts.Grpc.Models;

[ProtoContract]
public class GetWalletByCustomerIdGrpcRequest
{
    [ProtoMember(1)]
    public long CustomerId { get; set; }
}
