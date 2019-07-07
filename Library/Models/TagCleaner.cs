using System;
using System.Collections.Generic;

namespace EPlayer.Library.Models
{
	public static class TagCleaner
	{
		private static readonly string[] Keywords = new string[]
		{
			".com",
			".ir",
			".org",
			"www.",
			"@",
			".me",
			".biz",
			".net",
			".us",
			".az"
		};

		private static void Clean(IList<string> collection)
		{
			for (var i = 0; i < collection.Count; i++)
				Clean(collection[i]);
		}
		private static void Clean(string tag)
		{
			for (var i = 0; i < Keywords.Length; i++)
				if (tag.IncaseContains(Keywords[i]))
					DeleteWord(tag, Keywords[i]);
		}

		private static string DeleteWord(string text, string word)
		{
			var output = "";
			var lit1 = text.ToLower().IndexOf(word);
			if (text.StartsWith(word)) lit1 = 0;
			if (lit1 == -1) return text;
			var temp1 = text.Substring(0, lit1);
			if (temp1 == string.Empty) temp1 = " ";
			if (temp1.LastIndexOf(' ') == -1) return " ";
			var temp2 = temp1.Substring(0, temp1.LastIndexOf(' '));
			output += temp2;
			var lit2 = text.ToLower().LastIndexOf(word);
			temp1 = text.Substring(lit2);
			temp2 = !temp1.EndsWith(temp1) ? temp1.Substring(temp1.IndexOf(' ')) : "";
			output += temp2;
			return output;
		}

		public static void CleanTag(Song song)
		{
			/*
			Clean(song.Album);
			Clean(song.Title);
			Clean(song.Composers);
			Clean(song.Conductors);
			Clean(song.Genre);
			Clean(song.Writers);
			await song.SavePropertiesAsync();*/
			//TODO: Implement this shit
		}
	}
}
