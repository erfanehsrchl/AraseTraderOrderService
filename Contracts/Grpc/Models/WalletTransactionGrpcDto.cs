using ProtoBuf;

namespace Contracts.Grpc.Models;

[ProtoContract]
public class WalletTransactionGrpcDto
{
    [ProtoMember(1)]
    public long Id { get; set; }

    [ProtoMember(2)]
    public long WalletId { get; set; }

    [ProtoMember(3)]
    public long? OrderId { get; set; }

    [ProtoMember(4)]
    public decimal Amount { get; set; }

    [ProtoMember(5)]
    public decimal BalanceBefore { get; set; }

    [ProtoMember(6)]
    public decimal BalanceAfter { get; set; }

    [ProtoMember(7)]
    public int Reason { get; set; }

    [ProtoMember(8)]
    public DateTime CreatedAt { get; set; }
}
