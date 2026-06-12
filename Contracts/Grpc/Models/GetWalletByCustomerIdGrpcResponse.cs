using ProtoBuf;

namespace Contracts.Grpc.Models;

[ProtoContract]
public class GetWalletByCustomerIdGrpcResponse
{
    [ProtoMember(1)]
    public long WalletId { get; set; }

    [ProtoMember(2)]
    public long CustomerId { get; set; }

    [ProtoMember(3)]
    public decimal Balance { get; set; }
}
