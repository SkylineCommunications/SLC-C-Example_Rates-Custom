namespace Skyline.Protocol.Rates
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Newtonsoft.Json;

	#region RateHelpers
	/// <summary>
	/// Class <see cref="RateHelper"/> class helps calculating rates of all sorts (bit rates, counter rates, etc) based on counters.
	/// This class is meant to be used as base class for more specific RateHelpers depending on the range of counters (<see cref="System.UInt32"/>, <see cref="System.UInt64"/>, etc).
	/// </summary>
	[Serializable]
	public class RateHelper<T, U> where T : RateCounter<U>
	{
		[JsonProperty]
		protected readonly TimeSpan minDelta;

		[JsonProperty]
		protected readonly TimeSpan maxDelta;

		[JsonProperty]
		protected readonly List<T> counters = new List<T>();

		[JsonConstructor]
		private protected RateHelper(TimeSpan minDelta, TimeSpan maxDelta)
		{
			this.minDelta = minDelta;
			this.maxDelta = maxDelta;
		}

		/// <summary>
		/// Resets the currently buffered data of this <see cref="RateHelper"/> instance.
		/// </summary>
		public void Reset()
		{
			counters.Clear();
		}

		/// <summary>
		/// Serializes the currently buffered data of this <see cref="RateHelper"/> instance.
		/// </summary>
		/// <returns>A JSON string containing the serialized data of this <see cref="RateHelper"/> instance.</returns>
		public string ToJsonString()
		{
			return JsonConvert.SerializeObject(this);
		}

		protected double CalculateBasedOnDateTime(T newRateCounter, double faultyReturn)
		{
			// Sanity Checks
			if (counters.Any() && (newRateCounter.DateTime <= counters[counters.Count - 1].DateTime || newRateCounter.DateTime - counters[counters.Count - 1].DateTime > maxDelta))
			{
				Reset();
			}

			// Find previous counter to be used
			int oldCounterPos = -1;
			TimeSpan rateTimeSpan = new TimeSpan();
			for (int i = counters.Count - 1; i > -1; i--)
			{
				rateTimeSpan = newRateCounter.DateTime - counters[i].DateTime;
				if (rateTimeSpan >= minDelta)
				{
					oldCounterPos = i;
				}
			}

			return Calculate(newRateCounter, oldCounterPos, rateTimeSpan, faultyReturn);
		}

		protected double CalculateBasedOnTimeSpan(T newRateCounter, double faultyReturn)
		{
			// Sanity Checks
			if (counters.Any() && newRateCounter.TimeSpan > maxDelta)
			{
				Reset();
			}

			// Find previous counter to be used
			int oldCounterPos = -1;
			TimeSpan rateTimeSpan = newRateCounter.TimeSpan;
			for (int i = counters.Count - 1; i > -1; i--)
			{
				rateTimeSpan += counters[i].TimeSpan;
				if (rateTimeSpan >= minDelta)
				{
					oldCounterPos = i;
				}
			}

			return Calculate(newRateCounter, oldCounterPos, rateTimeSpan, faultyReturn);
		}

		protected static void ValidateMinAndMaxDeltas(TimeSpan minDelta, TimeSpan maxDelta)
		{
			if (minDelta < TimeSpan.Zero)
			{
				throw new ArgumentException("minDeta can't be a negative TimeSpan", "minDelta");
			}

			if (maxDelta < TimeSpan.Zero)
			{
				throw new ArgumentException("maxDelta can't be a negative TimeSpan", "maxDelta");
			}

			if (maxDelta <= minDelta)
			{
				throw new ArgumentException("maxDelta should be bigger than minDelta");
			}
		}

		private double Calculate(T newRateCounter, int oldCounterPos, TimeSpan rateTimeSpan, double faultyReturn)
		{
			// Calculate
			double rate;
			if (oldCounterPos > -1)
			{
				rate = CalculateRate(newRateCounter.Counter, counters[oldCounterPos].Counter, rateTimeSpan);
				counters.RemoveRange(0, oldCounterPos);
			}
			else
			{
				rate = faultyReturn;
			}

			// Add new counter
			counters.Add(newRateCounter);

			return rate;
		}

		private static double CalculateRate(dynamic newCounter, dynamic oldCounter, TimeSpan timeSpan)
		{
			unchecked
			{
				// Note that the use of var without casting here only works cause currently, generic type U can only be uint or ulong and :
				// - subtracting 2 uint implicitly returns a uint and handles wrap around nicely.
				// - subtracting 2 ulong implicitly returns a ulong and handles wrap around nicely.
				// If generic type U could be of other types, then an explicit casting could be required. Example:
				// - subtracting 2 ushort implicitly first converts both values to int, subtract them and returns an int and won't handle wrap around properly.
				//    In such case, an explicit cast to ushort would be required for the wrap around to be properly handled.

				var counterIncrease = newCounter - oldCounter;
				return counterIncrease / timeSpan.TotalMinutes;
			}
		}
	}


	/// <summary>
	/// Allows calculating rates of all sorts (bit rates, counter rates, etc) based on <see cref="System.UInt32"/> counters.
	/// </summary>
	[Serializable]
	public class RateHelper32 : RateHelper<RateCounter32, uint>
	{
		[JsonConstructor]
		private RateHelper32(TimeSpan minDelta, TimeSpan maxDelta) : base(minDelta, maxDelta) { }

		/// <summary>
		/// Calculates the rate using provided <paramref name="newCounter"/> against previous counters buffered in this <see cref="RateHelper32"/> instance.
		/// </summary>
		/// <param name="newCounter">The latest known counter value.</param>
		/// <param name="time">The <see cref="System.DateTime"/> at which <paramref name="newCounter"/> value was taken.</param>
		/// <param name="faultyReturn">The value to be returned in case a correct rate could not be calculated.</param>
		/// <returns>The calculated rate or the value specified in <paramref name="faultyReturn"/> in case the rate can not be calculated.</returns>
		public double Calculate(uint newCounter, DateTime time, double faultyReturn = -1)
		{
			var rateCounter = new RateCounter32(newCounter, time);
			return CalculateBasedOnDateTime(rateCounter, faultyReturn);
		}

		/// <summary>
		/// Calculates the rate using provided <paramref name="newCounter"/> against previous counters buffered in this <see cref="RateHelper32"/> instance.
		/// </summary>
		/// <param name="newCounter">The latest known counter value.</param>
		/// <param name="delta">The elapse <see cref="System.TimeSpan"/> between this new counter and previous one.</param>
		/// <param name="faultyReturn">The value to be returned in case a correct rate could not be calculated.</param>
		/// <returns>The calculated rate or the value specified in <paramref name="faultyReturn"/> in case the rate can not be calculated.</returns>
		public double Calculate(uint newCounter, TimeSpan delta, double faultyReturn = -1)
		{
			var rateCounter = new RateCounter32(newCounter, delta);
			return CalculateBasedOnTimeSpan(rateCounter, faultyReturn);
		}

		/// <summary>
		/// Deserializes a JSON <see cref="System.String"/> to a <see cref="RateHelper32"/> instance.
		/// </summary>
		/// <param name="rateHelperSerialized">Serialized <see cref="RateHelper32"/> instance.</param>
		/// <param name="minDelta">Minimum <see cref="System.TimeSpan"/> necessary between 2 counters when calculating a rate. Counters will be buffered until this minimum delta is met.</param>
		/// <param name="minDelta">Maximum <see cref="System.TimeSpan"/> allowed between 2 counters when calculating a rate.</param>
		/// <returns>A new instance of the <see cref="RateHelper32"/> class with all data found in <paramref name="rateHelperSerialized"/>.</returns>
		public static RateHelper32 FromJsonString(string rateHelperSerialized, TimeSpan minDelta, TimeSpan maxDelta)
		{
			ValidateMinAndMaxDeltas(minDelta, maxDelta);

			return !String.IsNullOrWhiteSpace(rateHelperSerialized) ?
				JsonConvert.DeserializeObject<RateHelper32>(rateHelperSerialized) :
				new RateHelper32(minDelta, maxDelta);
		}
	}

	/// <summary>
	/// Allows calculating rates of all sorts (bit rates, counter rates, etc) based on <see cref="System.UInt64"/> counters.
	/// </summary>
	[Serializable]
	public class RateHelper64 : RateHelper<RateCounter64, ulong>
	{
		[JsonConstructor]
		private RateHelper64(TimeSpan minDelta, TimeSpan maxDelta) : base(minDelta, maxDelta) { }

		/// <summary>
		/// Calculates the rate using provided <paramref name="newCounter"/> against previous counters buffered in this <see cref="RateHelper64"/> instance.
		/// </summary>
		/// <param name="newCounter">The latest known counter value.</param>
		/// <param name="time">The <see cref="System.DateTime"/> at which <paramref name="newCounter"/> value was taken.</param>
		/// <param name="faultyReturn">The value to be returned in case a correct rate could not be calculated.</param>
		/// <returns>The calculated rate or the value specified in <paramref name="faultyReturn"/> in case the rate can not be calculated.</returns>
		public double Calculate(ulong newCounter, DateTime time, double faultyReturn = -1)
		{
			var rateCounter = new RateCounter64(newCounter, time);
			return CalculateBasedOnDateTime(rateCounter, faultyReturn);
		}

		/// <summary>
		/// Calculates the rate using provided <paramref name="newCounter"/> against previous counters buffered in this <see cref="RateHelper64"/> instance.
		/// </summary>
		/// <param name="newCounter">The latest known counter value.</param>
		/// <param name="delta">The elapse <see cref="System.TimeSpan"/> between this new counter and previous one.</param>
		/// <param name="faultyReturn">The value to be returned in case a correct rate could not be calculated.</param>
		/// <returns>The calculated rate or the value specified in <paramref name="faultyReturn"/> in case the rate can not be calculated.</returns>
		public double Calculate(ulong newCounter, TimeSpan delta, double faultyReturn = -1)
		{
			var rateCounter = new RateCounter64(newCounter, delta);
			return CalculateBasedOnTimeSpan(rateCounter, faultyReturn);
		}

		/// <summary>
		/// Deserializes a JSON <see cref="System.String"/> to a <see cref="RateHelper64"/> instance.
		/// </summary>
		/// <param name="rateHelperSerialized">Serialized <see cref="RateHelper64"/> instance.</param>
		/// <param name="minDelta">Minimum <see cref="System.TimeSpan"/> necessary between 2 counters when calculating a rate. Counters will be buffered until this minimum delta is met.</param>
		/// <param name="minDelta">Maximum <see cref="System.TimeSpan"/> allowed between 2 counters when calculating a rate.</param>
		/// <returns>A new instance of the <see cref="RateHelper64"/> class with all data found in <paramref name="rateHelperSerialized"/>.</returns>
		public static RateHelper64 FromJsonString(string rateHelperSerialized, TimeSpan minDelta, TimeSpan maxDelta)
		{
			ValidateMinAndMaxDeltas(minDelta, maxDelta);

			return !String.IsNullOrWhiteSpace(rateHelperSerialized) ?
				JsonConvert.DeserializeObject<RateHelper64>(rateHelperSerialized) :
				new RateHelper64(minDelta, maxDelta);
		}
	}
	#endregion

	#region RateCounters
	public class RateCounter<U>
	{
		public DateTime DateTime { get; set; }
		public TimeSpan TimeSpan { get; set; }

		public U Counter { get; set; }

		internal RateCounter(DateTime dateTime)
		{
			DateTime = dateTime;
		}

		internal RateCounter(TimeSpan timeSpan)
		{
			TimeSpan = timeSpan;
		}
	}

	public class RateCounter32 : RateCounter<uint>
	{
		internal RateCounter32(uint counter, DateTime dateTime) : base(dateTime)
		{
			Counter = counter;
		}

		internal RateCounter32(uint counter, TimeSpan timeSpan) : base(timeSpan)
		{
			Counter = counter;
		}
	}

	public class RateCounter64 : RateCounter<ulong>
	{
		internal RateCounter64(ulong counter, DateTime dateTime) : base(dateTime)
		{
			Counter = counter;
		}

		internal RateCounter64(ulong counter, TimeSpan timeSpan) : base(timeSpan)
		{
			Counter = counter;
		}
	}
	#endregion
}