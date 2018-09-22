using System;
using System.Collections.Generic;
using System.Linq;
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
		private BannedWords BannedWords = new BannedWords();

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
			dynamic Message = JsonConvert.DeserializeObject(message);
			if (Message.serviceTag != null)
			{//Player
				if (BannedWords.Words.Any(s => $"[{Message.serviceTag}]{Message.name}".ToLower().Contains(s)))
				{
					Task.Run(async () =>
					{
						await Alert($"Bad Name", $"[{Message.serviceTag}]{Message.name}", $"{Message.uid}", $"{Message.server}", "N/A");
					});
					Console.WriteLine($"Reporting {message.ToString()}");
				}
			} else
			{//Message
				if (BannedWords.Words.Any(s => Message.message.ToString().ToLower().Contains(s))){
					Task.Run(async () =>
					{
						await Alert($"Bad Word",$"{Message.player}", $"{Message.UID}", $"{Message.server}", $"{Message.message}");
					});
					Console.WriteLine($"Reporting {message.ToString()}");
				}
			}
			//Console.WriteLine(message.ToString());
		}

		async private Task<String> Alert(string title, string name, string UID, string server, string message)
		{
			using (HttpClient client = new HttpClient())
			using (HttpResponseMessage content = await client.PostAsync(config["webhook"], new StringContent(new AlertMessage(title, name, UID, server, message).ToString(), System.Text.Encoding.UTF8, "application/json")))
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
		private readonly string Title;
		private readonly string Name;
		private readonly string UID;
		private readonly string Message;
		private readonly string Server;
		
		public AlertMessage(string title, string name, string uID, string server, string message)
		{
			Title = title;
			Name = name;
			UID = uID;
			Message = message;
			Server = server;
		}

		public AlertMessage(string title, string name, string uID, string server) : this(title, name, uID, server, null)
		{
		}
		public override string ToString()
		{
			return $@"{{
				""content"": "" "",
				""embeds"": [{{
					""color"": 15427412,
					""title"": ""{Title}"",
					""fields"": [
						{{""inline"": true, ""name"":""Name"",""value"": ""{Name}"" }},
						{{""inline"": true, ""name"":""UID"",""value"": ""{UID}""}},
						{{""inline"": true, ""name"":""Server"",""value"": ""{Server}""}}," +
						((Message == null) ? "" : $@"{{""inline"": false, ""name"":""Message"",""value"": ""{Message}""}}") +
					$@"],
					""type"": ""rich""
				}}]
			}}";
		}
	}
}
