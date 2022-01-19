namespace Skyline.Protocol.Rates
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Newtonsoft.Json;

	#region RateHelpers
	/// <summary>
	/// Class <c>RateHelper</c> helps calculating rates of all sorts (bit rates, counter rates, etc) based on counters.
	/// This class is meant to be used as base class for more specific RateHelpers depending on the range of counters (uint, ulong, etc).
	/// </summary>
	[Serializable]
	public class RateHelper<T, U> where T : RateCounter<U>/*, new()*/
	{
		[JsonProperty]
		protected readonly TimeSpan minDelta;

		[JsonProperty]
		protected readonly TimeSpan maxDelta;

		[JsonProperty]
		protected readonly List<T> counters = new List<T>();

		private protected RateHelper(TimeSpan minDelta, TimeSpan maxDelta)
		{
			this.minDelta = minDelta;
			this.maxDelta = maxDelta;
		}

		protected bool TryCalculate(dynamic newCounter, DateTime time, double faultyReturn, out double rate)
		{
			rate = faultyReturn;

			// Sanity checks
			if (counters.Any() && (time <= counters[counters.Count - 1].Time || time - counters[counters.Count - 1].Time > maxDelta))
			{
				Reset();
			}

			if (newCounter < 0)
			{
				return false;
			}

			// Find previous counter to be used
			int oldCounterPos = -1;
			for (int i = counters.Count - 1; i > -1; i--)
			{
				if (time - counters[i].Time >= minDelta)
				{
					oldCounterPos = i;
					break;
				}
			}

			// Calculate
			if (oldCounterPos > -1)
			{
				unchecked
				{
					// Note that the use of var without casting here only works cause currently, generic type U can only be uint or ulong and :
					// - subtracting 2 uint implicitly returns a uint and handles wrap around nicely.
					// - subtracting 2 ulong implicitly returns a ulong and handles wrap around nicely.
					// If generic type U could be of other types, then an explicit casting could be required. Example:
					// - subtracting 2 ushort implicitly first converts both values to int, subtract them and returns an int and won't handle wrap around properly.
					//    In such case, an explicit cast to ushort would be required for the wrap around to be properly handled.
					var counterIncrease = newCounter - counters[oldCounterPos].Counter;

					rate = counterIncrease / (time - counters[oldCounterPos].Time).TotalMinutes;
				}

				counters.RemoveRange(0, oldCounterPos);
			}
			else
			{
				rate = faultyReturn;
			}

			return true;
		}

		/// <summary>
		/// This resets the currently buffered data of this <c>RateHelper</c> instance. 
		/// </summary>
		public void Reset()
		{
			counters.Clear();
		}

		/// <summary>
		/// This serializes the currently buffered data of this <c>RateHelper</c> instance.
		/// </summary>
		/// <returns>A JSON string containing the serialized data of this <c>RateHelper</c> instance.</returns>
		public string ToJsonString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}

	/// <summary>
	/// Class <c>RateHelper</c> helps calculating rates of all sorts (bit rates, counter rates, etc) based on 32 bit counters.
	/// </summary>
	[Serializable]
	public class RateHelper32 : RateHelper<RateCounter32, uint>
	{
		public RateHelper32(TimeSpan minDelta, TimeSpan maxDelta) : base(minDelta, maxDelta) { }

		/// <summary>
		/// Calculates a rate using provided <paramref name="newCounter"/> against previous counters buffered in this <c>RateHelper32</c> instance. 
		/// </summary>
		/// <param name="newCounter">The latest known counter value.</param>
		/// <param name="time">The DateTime at which <paramref name="newCounter"/> value was taken.</param>
		/// <param name="faultyReturn">The value to be returned in case a correct rate could not be calculated.</param>
		/// <returns></returns>
		public double Calculate(uint newCounter, DateTime time, double faultyReturn = -1)
		{
			if (base.TryCalculate(newCounter, time, faultyReturn, out double rate))
			{
				counters.Add(new RateCounter32(newCounter, time));
			}

			return rate;
		}

		/// <summary>
		/// Deserializes a JSON <typeparamref name="string" /> to a <c>RateHelper32</c> instance.
		/// </summary>
		/// <param name="rateHelperSerialized">Serialized <c>RateHelper32</c> instance.</param>
		/// <param name="minDelta">Minimum timespan necessary between 2 counters when calculating a rate. Counters will be buffered until this minimum delta is met.</param>
		/// <param name="minDelta">Maximum timespan allowed between 2 counters when calculating a rate.</param>
		/// <returns></returns>
		public static RateHelper32 FromJsonString(string rateHelperSerialized, TimeSpan minDelta, TimeSpan maxDelta)
		{
			return !String.IsNullOrWhiteSpace(rateHelperSerialized) ?
				JsonConvert.DeserializeObject<RateHelper32>(rateHelperSerialized) :
				new RateHelper32(minDelta, maxDelta);
		}
	}

	/// <summary>
	/// Class <c>RateHelper</c> helps calculating rates of all sorts (bit rates, counter rates, etc) based on 64 bit counters.
	/// </summary>
	[Serializable]
	public class RateHelper64 : RateHelper<RateCounter64, ulong>
	{
		public RateHelper64(TimeSpan minDelta, TimeSpan maxDelta) : base(minDelta, maxDelta) { }

		/// <summary>
		/// Calculates a rate using provided <paramref name="newCounter"/> against previous counters buffered in this <c>RateHelper32</c> instance. 
		/// </summary>
		/// <param name="newCounter">The latest known counter value.</param>
		/// <param name="time">The DateTime at which <paramref name="newCounter"/> value was taken.</param>
		/// <param name="faultyReturn">The value to be returned in case a correct rate could not be calculated.</param>
		/// <returns></returns>
		public double Calculate(ulong newCounter, DateTime time, double faultyReturn = -1)
		{
			if (base.TryCalculate(newCounter, time, faultyReturn, out double rate))
			{
				counters.Add(new RateCounter64(newCounter, time));
			}

			return rate;
		}

		/// <summary>
		/// Deserializes a JSON string to a <c>RateHelper64</c> instance.
		/// </summary>
		/// <param name="rateHelperSerialized">Serialized <c>RateHelper64</c> instance.</param>
		/// <param name="minDelta">Minimum timespan necessary between 2 counters when calculating a rate. Counters will be buffered until this minimum delta is met.</param>
		/// <param name="minDelta">Maximum timespan allowed between 2 counters when calculating a rate.</param>
		/// <returns></returns>
		public static RateHelper64 FromJsonString(string rateHelperSerialized, TimeSpan minDelta, TimeSpan maxDelta)
		{
			return !String.IsNullOrWhiteSpace(rateHelperSerialized) ?
				JsonConvert.DeserializeObject<RateHelper64>(rateHelperSerialized) :
				new RateHelper64(minDelta, maxDelta);
		}
	}
	#endregion

	#region RateCounters
	public class RateCounter<U>
	{
		public DateTime Time { get; set; }
		public U Counter { get; set; }

		protected RateCounter(DateTime time)
		{
			Time = time;
		}
	}

	public class RateCounter32 : RateCounter<uint>
	{
		public RateCounter32(uint counter, DateTime time) : base(time)
		{
			Counter = counter;
		}
	}

	public class RateCounter64 : RateCounter<ulong>
	{
		public RateCounter64(ulong counter, DateTime time) : base(time)
		{
			Counter = counter;
		}
	}
	#endregion
}