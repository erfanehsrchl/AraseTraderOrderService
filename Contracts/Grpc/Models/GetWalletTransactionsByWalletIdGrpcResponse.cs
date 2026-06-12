using ProtoBuf;

namespace Contracts.Grpc.Models;

[ProtoContract]
public class GetWalletTransactionsByWalletIdGrpcResponse
{
    [ProtoMember(1)]
    public List<WalletTransactionGrpcDto> Transactions { get; set; } = [];
}
