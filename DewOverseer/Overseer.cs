using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;


namespace DewOverseer
{
	class Overseer
	{
		private IConfiguration config;
		

		public Overseer()
		{
			Thread KeepSocketAlive = new Thread(new ThreadStart(KeepAlive));
			KeepSocketAlive.Start();
			config = new ConfigurationBuilder().AddJsonFile("config.json", false).Build();
			ServerConnection session = new ServerConnection(string.Format("ws://{0}:{1}/{2}", config["address"], config["port"], config["password"]));
			session.ServerMessage += new ServerEventHandler(this.OnMessage);
			session.ServerClose += new ServerCloseHandler(this.OnClose);
		}

		private void OnMessage(object sender, string message)
		{
			if (message.StartsWith("accept"))
			{
				Console.WriteLine("Connected");
				return;
			}

			dynamic Message = JsonConvert.DeserializeObject(message);
			if (Message.serviceTag != null)
			{//Player
				Task.Run(async () =>
				{
					//await Alert($"[{Message.serviceTag}]{Message.name}", $"{Message.uid}", $"Reported On Server {Message.server}");
				});
			} else
			{//Message
				Task.Run(async () =>
				{
					//await Alert($"[{Message.serviceTag}]{Message.name}", $"{Message.uid}", $"Reported On Server {Message.server}");
				});
			}
			Console.WriteLine(message.ToString());
		}

		async private Task<String> Alert(string name, string UID, string message)
		{
			using (HttpClient client = new HttpClient())
			using (HttpResponseMessage content = await client.PostAsync(config["webhook"], new StringContent(new AlertMessage(name, UID, message).ToString(), System.Text.Encoding.UTF8, "application/json")))
				return await content.Content.ReadAsStringAsync();
		}

		private void OnClose(object sender, int code)
		{
			Console.WriteLine("Connection Closed");
		}

		public void KeepAlive()
		{
			Thread.Sleep(2000);
			while (true)
			{
				Thread.Sleep(120000);
			}
		}
	}
	class AlertMessage
	{
		private readonly string Name;
		private readonly string UID;
		private readonly string Message;

		public AlertMessage(string name, string uID, string message)
		{
			Name = name;
			UID = uID;
			Message = message;
		}
		public override string ToString()
		{
			return $@"{{
				""content"": ""Test Post"",
				""embeds"": [{{
					""color"": 15427412,
					""title"": ""Player Kicked"",
					""fields"": [
						{{""inline"": true, ""name"":""Name"",""value"": ""{Name}"" }},
						{{""inline"": true, ""name"":""UID"",""value"": ""{UID}""}},
						{{""inline"": true, ""name"":""Reason"",""value"": ""{Message}""}}
					],
					""type"": ""rich""
				}}]
			}}";
		}
	}
}
