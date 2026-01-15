namespace DnsResolver.Domain.ValueObjects;

using System.Net;
using DnsResolver.Domain.Exceptions;

public sealed class DnsServer : IEquatable<DnsServer>
{
    public string Address { get; }
    public int Port { get; }

    private DnsServer(string address, int port)
    {
        Address = address;
        Port = port;
    }

    public static DnsServer Create(string address, int port = 53)
    {
        if (!IPAddress.TryParse(address, out _))
            throw new DomainException($"无效的 DNS 服务器地址: {address}");

        if (port < 1 || port > 65535)
            throw new DomainException($"无效的端口号: {port}");

        return new DnsServer(address, port);
    }

    public bool Equals(DnsServer? other) =>
        other is not null && Address == other.Address && Port == other.Port;
    public override bool Equals(object? obj) => obj is DnsServer other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Address, Port);
    public override string ToString() => Port == 53 ? Address : $"{Address}:{Port}";
}
