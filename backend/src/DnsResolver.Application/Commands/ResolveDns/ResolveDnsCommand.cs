namespace DnsResolver.Application.Commands.ResolveDns;

public record ResolveDnsCommand(
    string Domain,
    string RecordType,
    string DnsServer
);
