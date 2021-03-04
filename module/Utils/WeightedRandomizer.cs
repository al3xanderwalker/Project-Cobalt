using System;
using System.Collections.Generic;
using System.Linq;

namespace Project_Cobalt.Utils
{
    	/// <summary>
	/// Static class to improve readability
	/// Example:
	///	<code>
	/// var selected = WeightedRandomizer.From(weights).TakeOne();
	/// </code>
	/// 
	/// </summary>
	public static class WeightedRandomizer
	{
		public static WeightedRandomizer<TR> From<TR>(Dictionary<TR, int> spawnRate)
		{
			return new WeightedRandomizer<TR>(spawnRate);
		}
	}

	public class WeightedRandomizer<T>
	{
		private static Random _random = new Random();
		private Dictionary<T, int> _weights;
 
		/// <summary>
		/// Instead of calling this constructor directly,
		/// consider calling a static method on the WeightedRandomizer (non-generic) class
		/// for a more readable method call, i.e.:
		/// 
		/// <code>
		/// var selected = WeightedRandomizer.From(weights).TakeOne();
		/// </code>
		/// 
		/// </summary>
		/// <param name="weights"></param>
		public WeightedRandomizer(Dictionary<T, int> weights)
		{
			_weights = weights;
		}

		/// <summary>
		/// Randomizes one item
		/// </summary>
		/// <param name="spawnRate">An ordered list withe the current spawn rates. The list will be updated so that selected items will have a smaller chance of being repeated.</param>
		/// <returns>The randomized item.</returns>
		public T TakeOne()
		{
			// Sorts the spawn rate list
			var sortedSpawnRate = Sort(_weights);

			// Sums all spawn rates
			var sum = _weights.Sum(spawn => spawn.Value);

			// Randomizes a number from Zero to Sum
			var roll = _random.Next(0, sum);

			// Finds chosen item based on spawn rate
			var selected = sortedSpawnRate[sortedSpawnRate.Count - 1].Key;
			foreach (var spawn in sortedSpawnRate)
			{
				if (roll < spawn.Value)
				{
					selected = spawn.Key;
					break;
				}
				roll -= spawn.Value;
			}

			// Returns the selected item
			return selected;
		}

		private static List<KeyValuePair<T, int>> Sort(Dictionary<T, int> weights)
		{
			var list = new List<KeyValuePair<T, int>>(weights);

			// Sorts the Spawn Rate List for randomization later
			list.Sort(
				(firstPair, nextPair) => firstPair.Value.CompareTo(nextPair.Value)
			);

			return list;
		}
	}
}