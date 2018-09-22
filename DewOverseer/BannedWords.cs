using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DewOverseer
{
	class BannedWords
	{

		private List<string> words = new List<string>();

		public BannedWords(){
			FileSystemWatcher watcher = new FileSystemWatcher();
			watcher.Path = AppDomain.CurrentDomain.BaseDirectory;
			watcher.Filter = "*BannedWords.txt";
			watcher.NotifyFilter = NotifyFilters.LastWrite;
			watcher.Changed += new FileSystemEventHandler(OnChanged);
			watcher.EnableRaisingEvents = true;
			Populate();
		}

		public List<string> Words { get => words;}

		private void Populate()
		{
			string[] lines = File.ReadAllLines($"{AppDomain.CurrentDomain.BaseDirectory}/BannedWords.txt");
			words = lines.ToList();
		}

		private void OnChanged(object source, FileSystemEventArgs e)
		{
			Console.WriteLine("Updating banned words");
			Populate();
		}
	}
}
