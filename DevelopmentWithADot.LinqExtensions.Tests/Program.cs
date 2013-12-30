using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevelopmentWithADot.LinqExtensions.Tests
{
	class Program
	{
		static void Main(string[] args)
		{
			var dates = Enumerable.Range(0, 10).Select(x => DateTime.Now.AddDays((new Random().Next(1) > 0.5 ? -1 : 1) * new Random().Next(x))).AsQueryable();

			var days = dates.Select("Day").OfType<Object>().ToList();

			var groups = dates.GroupBy("Month", "Day").OfType<Object>().ToList();
		}
	}
}
