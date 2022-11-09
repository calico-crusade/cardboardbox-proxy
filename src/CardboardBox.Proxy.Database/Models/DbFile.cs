namespace CardboardBox.Proxy.Database
{
	public class DbFile
	{
		[JsonPropertyName("id")]
		public long Id { get; set; }

		[JsonPropertyName("url")]
		public string Url { get; set; } = string.Empty;

		[JsonPropertyName("name")]
		public string Name { get; set; } = string.Empty;

		[JsonPropertyName("hash")]
		public string Hash { get; set; } = string.Empty;

		[JsonPropertyName("mimeType")]
		public string MimeType { get; set; } = string.Empty;

		[JsonPropertyName("groupName")]
		public string GroupName { get; set; } = string.Empty;

		[JsonPropertyName("expires")]
		public DateTime? Expires { get; set; }

		[JsonPropertyName("createdAt")]
		public DateTime CreatedAt { get; set; }

		[JsonPropertyName("updatedAt")]
		public DateTime UpdatedAt { get; set; }

		[JsonPropertyName("deletedAt")]
		public DateTime? DeletedAt { get; set; }
	}
}
