namespace DnsResolver.Api.Requests;

public record CompareRequest(
    string Domain,
    string RecordType,
    List<string> IspList
);
