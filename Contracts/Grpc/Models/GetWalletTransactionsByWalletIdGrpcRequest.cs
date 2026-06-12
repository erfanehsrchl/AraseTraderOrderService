using ProtoBuf;

namespace Contracts.Grpc.Models;

[ProtoContract]
public class GetWalletTransactionsByWalletIdGrpcRequest
{
    [ProtoMember(1)]
    public long WalletId { get; set; }
}
