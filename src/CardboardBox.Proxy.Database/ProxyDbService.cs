using Dapper;

namespace CardboardBox.Proxy.Database
{
	public interface IProxyDbService
	{
		Task<DbFile?> GetFileByUrl(string url);
		Task<DbFile?> GetFileByHash(string hash);
		Task<long> Upsert(DbFile file);
		Task<PaginatedResult<DbFile>> GetFiles(int page, int size);
		Task CreateTable();
	}

	public class ProxyDbService : IProxyDbService
	{
		private static bool _tableCreated = false;

		private readonly ISqlService _sql;

		public ProxyDbService(ISqlService sql)
		{
			_sql = sql;
		}

		public Task<DbFile?> GetFileByUrl(string url)
		{
			const string QUERY = "SELECT * FROM file_cache WHERE LOWER(url) = LOWER(:url)";
			return _sql.Fetch<DbFile?>(QUERY, new { url });
		}

		public Task<DbFile?> GetFileByHash(string hash)
		{
			const string QUERY = "SELECT * FROM file_cache WHERE hash = :hash";
			return _sql.Fetch<DbFile?>(QUERY, new { hash });
		}

		public Task<long> Upsert(DbFile file)
		{
			const string QUERY = @"INSERT INTO file_cache (
	url,
	name,
	hash,
	mime_type,
	group_name,
	expires,
	created_at,
	updated_at,
	deleted_at
) VALUES (
	:Url,
	:Name,
	:Hash,
	:MimeType,
	:GroupName,
	:Expires,
	CURRENT_TIMESTAMP,
	CURRENT_TIMESTAMP,
	:DeletedAt
) ON CONFLICT (hash) DO UPDATE SET
	url = :Url,
	name = :Name,
	hash = :Hash,
	mime_type = :MimeType,
	group_name = :GroupName,
	expires = :Expires,
	updated_at = CURRENT_TIMESTAMP,
	deleted_at = :DeletedAt
RETURNING id";
			return _sql.ExecuteScalar<long>(QUERY, file);
		}

		public async Task<PaginatedResult<DbFile>> GetFiles(int page, int size)
		{
			const string QUERY = @"SELECT * FROM file_cache WHERE deleted_at IS NULL ORDER BY updated_at DESC LIMIT :size OFFSET :offset;
SELECT COUNT(*) FROM file_cache WHERE deleted_at IS NULL;";

			using var con = _sql.CreateConnection();
			using var rdr = await con.QueryMultipleAsync(QUERY, new
			{
				size, 
				offset = (page - 1) * size
			});

			var res = (await rdr.ReadAsync<DbFile>()).ToArray();
			var total = await rdr.ReadSingleAsync<long>();

			var pages = (long)Math.Ceiling((double)total / size);
			return new PaginatedResult<DbFile>(pages, total, res);
		}

		public Task CreateTable()
		{
			if (_tableCreated) return Task.CompletedTask;

			_tableCreated = true;
			const string QUERY = @"CREATE TABLE IF NOT EXISTS file_cache (
    id BIGSERIAL PRIMARY KEY,

    url text not null,
	name text not null,
    hash text not null,
    mime_type text not null,
    group_name text not null,
    expires timestamp,

    created_at timestamp not null default CURRENT_TIMESTAMP,
    updated_at timestamp not null default CURRENT_TIMESTAMP,
    deleted_at timestamp,

    CONSTRAINT uiq_file_cache_hash UNIQUE(hash)
);";
			return _sql.Execute(QUERY);
		}
	}
}
