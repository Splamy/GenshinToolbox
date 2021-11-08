using IronOcr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace GenshinToolbox
{
	public static class Ocr
	{
		public static IronTesseract NewInstance()
		{
			var ocr = new IronTesseract();
			ocr.Configuration.ReadBarCodes = false;
			ocr.Configuration.RenderSearchablePdfsAndHocr = false;
			ocr.Language = OcrLanguage.EnglishBest;
			return ocr;
		}

		private static IronTesseract? _instance;
		public static IronTesseract Instance => _instance ??= NewInstance();

		public static T FindClosest<T>(this IEnumerable<(T, string)> kvs, string result)
			=> kvs.Select(s => new KeyValuePair<T, string>(s.Item1, s.Item2)).FindClosest(result);
		public static T FindClosest<T>(this IEnumerable<KeyValuePair<T, string>> kvs, string result)
		{
			var bestDist = int.MaxValue;
			T best = default!;
			foreach (var kvp in kvs)
			{
				var dist = Lehvenshtein(kvp.Value, result);
				if (dist < bestDist)
				{
					bestDist = dist;
					best = kvp.Key;
					if (dist == 0)
						return best;
				}
			}
			return best;
		}

		public static int Lehvenshtein(string s, string t)
		{
			if (s == t)
			{
				return 0;
			}

			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// Verify arguments.
			if (n == 0)
			{
				return m;
			}

			if (m == 0)
			{
				return n;
			}

			// Initialize arrays.
			for (int i = 0; i <= n; d[i, 0] = i++) { }

			for (int j = 0; j <= m; d[0, j] = j++) { }

			// Begin looping.
			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++)
				{
					// Compute cost.
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
					d[i, j] = Math.Min(
						Math.Min(
							d[i - 1, j] + 1,
							d[i, j - 1] + 1
						),
						d[i - 1, j - 1] + cost
					);
				}
			}
			// Return cost.
			return d[n, m];
		}

		public static string Allow(this IEnumerable<string> enu) => enu.SelectMany(x => x).Allow();
		public static string Allow(this IEnumerable<char> enu) => string.Join("", enu.Distinct().OrderBy(x => x));

		private static readonly object ocrSync = new();
		public static OcrInput CreateSafe(Bitmap bmp)
		{
			for (int i = 0; i < 5; i++)
			{
				try
				{
					lock (ocrSync)
					{
						return new OcrInput(bmp);
					}
				}
				catch
				{
					Console.WriteLine("IronOCR is weird again...");
					Thread.Sleep(10);
				}
			}
			throw new Exception("Could not create ocr input");
		}
	}
}
