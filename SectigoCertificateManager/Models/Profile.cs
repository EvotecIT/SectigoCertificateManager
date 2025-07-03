namespace SectigoCertificateManager.Models;

using System.Collections.Generic;
public sealed class Profile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IReadOnlyList<int> Terms { get; set; } = [];
    public IReadOnlyDictionary<string, IReadOnlyList<string>> KeyTypes { get; set; } = new Dictionary<string, IReadOnlyList<string>>();
    public bool UseSecondaryOrgName { get; set; }
}
