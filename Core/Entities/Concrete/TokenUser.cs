using System.Collections.Generic;
using System;
using System.Text.Json.Serialization;

namespace Core.Entities.Concrete;

public class TokenUser
{
    [JsonPropertyName("UserId")]
    public int UserId { get; set; }

    [JsonPropertyName("UserGuid")]
    public Guid UserGuid { get; set; }

    [JsonPropertyName("Username")]
    public string? Username { get; set; }

    [JsonPropertyName("CompanyName")]
    public string? CompanyName { get; set; }

    [JsonPropertyName("Roles")]
    public List<string> Roles { get; set; } = new();
}