namespace DnsResolver.Application.Commands.CompareDns;

public record CompareDnsCommand(
    string Domain,
    string RecordType,
    List<string> IspIds
);
