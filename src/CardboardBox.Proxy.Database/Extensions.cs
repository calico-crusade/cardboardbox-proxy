using Dapper.FluentMap;

namespace CardboardBox.Proxy.Database
{
	public static class Extensions
	{
		public static IServiceCollection AddProxy(this IServiceCollection services)
		{
			FluentMapper.Initialize(config =>
			{
				var conv = config
					.AddConvention<CamelCaseMap>()
					.ForEntity<DbFile>();
			});

			return services
				.AddCardboardHttp()
				.AddTransient<IProxyService, ProxyService>()
				.AddTransient<IProxyDbService, ProxyDbService>()
				.AddTransient<ISqlService, NpgsqlService>();
		}
	}
}
