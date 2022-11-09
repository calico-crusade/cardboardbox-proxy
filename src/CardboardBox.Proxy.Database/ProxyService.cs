namespace CardboardBox.Proxy.Database
{
	public interface IProxyService
	{
		Task<FileData?> GetFile(string url, string group = ProxyService.DEFAULT_GROUP, DateTime? expires = null);
	}

	public class ProxyService : IProxyService
	{
		public const string DEFAULT_GROUP = "files";

		private readonly IConfiguration _config;
		private readonly IProxyDbService _db;
		private readonly ILogger _logger;
		private readonly IApiService _api;

		public string CacheDirectory => _config["CacheDirectory"];

		public ProxyService(
			IConfiguration config, 
			IProxyDbService db, 
			ILogger<ProxyService> logger, 
			IApiService api)
		{
			_config = config;
			_db = db;
			_logger = logger;
			_api = api;
		}

		public async Task<FileData?> GetFile(string url, string group = DEFAULT_GROUP, DateTime? expires = null)
		{
			try
			{
				if (!Directory.Exists(CacheDirectory))
					Directory.CreateDirectory(CacheDirectory);

				var hash = url.MD5Hash();
				var path = GeneratePath(hash, group);
				var data = await _db.GetFileByHash(hash);

				var expired = (data?.Expires ?? DateTime.MaxValue) <= DateTime.Now;

				if (data != null && !expired && File.Exists(path))
					return new(ReadFile(path), data.Name, data.MimeType, data.Id);

				var io = new MemoryStream();
				var (stream, _, file, type) = await _api.GetData(url);
				await stream.CopyToAsync(io);
				io.Position = 0;

				file = DetermineFileName(file, url);

				var id = await _db.Upsert(new DbFile { Url = url, Hash = hash, Name = file, MimeType = type, GroupName = group, Expires = expires });

				using var oo = File.Create(path);
				await io.CopyToAsync(oo);

				io.Position = 0;
				return new(io, file, type, id);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error occurred while fetching resource: {url} :: {group} :: {expires}");
				return null;
			}
		}

		public string DetermineFileName(string current, string url)
		{
			if (!string.IsNullOrEmpty(current)) return current;

			var part = url.Split('/', StringSplitOptions.RemoveEmptyEntries).Last().Split('?').First();
			return part;
		}

		public string GeneratePath(string hash, string group)
		{
			var dir = !string.IsNullOrEmpty(group) ? Path.Combine(CacheDirectory, group) : CacheDirectory;
			if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

			return Path.Combine(dir, hash + ".data");
		}

		public Stream ReadFile(string path) => File.OpenRead(path);
	}

	public record class FileData(Stream Stream, string Name, string MimeType, long CacheId);
}