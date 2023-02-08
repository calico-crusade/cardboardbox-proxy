using Microsoft.AspNetCore.Mvc;

namespace CardboardBox.Proxy.Controllers
{
	using Database;
	using ImageSim;

	[ApiController]
	public class FileController : ControllerBase
	{
		private readonly IProxyService _proxy;
		private readonly IProxyDbService _db;
		private readonly IImageSimService _img;

		public FileController(
			IProxyService proxy, 
			IProxyDbService db,
			IImageSimService img)
		{
			_proxy = proxy;
			_db = db;
			_img = img;
		}

		[HttpGet, Route("proxy")]
		public async Task<IActionResult> Get(
			[FromQuery] string path, 
			[FromQuery] string group = ProxyService.DEFAULT_GROUP, 
			[FromQuery] DateTime? expires = null,
			[FromQuery] string? referer = null,
			[FromQuery] bool noCache = false)
		{
			var data = await _proxy.GetFile(path, group, expires, false, referer, noCache);
			if (data == null) return NotFound();

			return File(data.Stream, data.MimeType, data.Name);
		}

		[HttpGet, Route("proxy/files")]
		public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int size = 100)
		{
			var data = await _db.GetFiles(page, size);
			return Ok(data);
		}
	}
}