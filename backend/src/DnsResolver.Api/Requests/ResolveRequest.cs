namespace DnsResolver.Api.Requests;

public record ResolveRequest(
    string Domain,
    string RecordType,
    string DnsServer
);
