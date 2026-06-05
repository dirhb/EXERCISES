using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryWSClient
{
	public class LibraryHttpClient
	{

		private static HttpClient CreateClient()
		{
			SocketsHttpHandler handler = new SocketsHttpHandler();
			handler.PooledConnectionLifetime = TimeSpan.FromMinutes(10);
			handler.ConnectTimeout = TimeSpan.FromSeconds(15);
			return new HttpClient(handler);
		}


		private static readonly HttpClient httpClient = CreateClient();

		private LibraryHttpClient() { }
		public static HttpClient Instance
		{
			get
			{
				return httpClient;
			}
		}
	}
}