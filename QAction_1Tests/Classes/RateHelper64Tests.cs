using System;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Skyline.Protocol.Rates;

namespace Skyline.Protocol.Rates.Tests
{
	[TestClass()]
	public class RateHelper64Tests
	{
		private readonly TimeSpan minDelta = new TimeSpan(0, 1, 0);
		private readonly TimeSpan maxDelta = new TimeSpan(1, 0, 0);
		private const double faultyReturn = -1;

		#region CalculateTests

		[TestMethod()]
		public void Calculate_Invalid_BackInTime()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper = RateHelper64.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(10, start);

			// Act
			double rate = helper.Calculate(20, start.AddSeconds(-10));

			// Assert
			double expectedRate = faultyReturn;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Invalid_TooLate()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper = RateHelper64.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(10, start);

			// Act
			double rate = helper.Calculate(20, start.AddHours(2));

			// Assert
			double expectedRate = faultyReturn;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Invalid_TooSoon()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper = RateHelper64.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(10, start);

			// Act
			double rate = helper.Calculate(20, start.AddSeconds(5));

			// Assert
			double expectedRate = faultyReturn;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToOlderCounter()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper = RateHelper64.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(5, start);
			helper.Calculate(10, start.AddSeconds(90));
			helper.Calculate(20, start.AddSeconds(91));
			helper.Calculate(30, start.AddSeconds(92));
			helper.Calculate(40, start.AddSeconds(93));

			// Act
			double rate = helper.Calculate(50, start.AddSeconds(100));

			// Assert
			double expectedRate = (50.0 - 5.0) / (100 / 60.0);
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_ToPreviousCounter()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper = RateHelper64.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(5, start);

			// Act
			double rate = helper.Calculate(50, start.AddSeconds(100));

			// Assert
			double expectedRate = (50.0 - 5.0) / (100 / 60.0);
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void Calculate_Valid_WithOverflow()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper = RateHelper64.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(UInt64.MaxValue - 10, start);

			// Act
			double rate = helper.Calculate(9, start.AddSeconds(100));

			// Assert
			double expectedRate = 20 / (100 / 60.0);
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		#endregion

		#region SerializeTests

		[TestMethod()]
		public void Serialize_Invalid_DifferentCounter()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper1 = RateHelper64.FromJsonString("", minDelta, maxDelta);
			helper1.Calculate(5, start);
			helper1.Calculate(10, start.AddSeconds(10));

			string serializedTemp = helper1.ToJsonString();
			var helper2 = RateHelper64.FromJsonString(serializedTemp, minDelta, maxDelta);

			// Different counter, same timing
			helper1.Calculate(20, start.AddSeconds(19));
			helper2.Calculate(21, start.AddSeconds(19));

			// Act
			string serialized1 = helper1.ToJsonString();
			string serialized2 = helper2.ToJsonString();

			// Assert
			serialized1.Should().NotBeEquivalentTo(serialized2);
		}

		[TestMethod()]
		public void Serialize_Invalid_DifferentTiming()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper1 = RateHelper64.FromJsonString("", minDelta, maxDelta);
			helper1.Calculate(5, start);
			helper1.Calculate(10, start.AddSeconds(10));

			string serializedTemp = helper1.ToJsonString();
			var helper2 = RateHelper64.FromJsonString(serializedTemp, minDelta, maxDelta);

			// same counter, different timing
			helper1.Calculate(20, start.AddSeconds(19));
			helper2.Calculate(20, start.AddSeconds(21));

			// Act
			string serialized1 = helper1.ToJsonString();
			string serialized2 = helper2.ToJsonString();

			// Assert
			serialized1.Should().NotBeEquivalentTo(serialized2);
		}

		[TestMethod()]
		public void Serialize_Valid()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper1 = RateHelper64.FromJsonString("", minDelta, maxDelta);
			helper1.Calculate(5, start);
			helper1.Calculate(10, start.AddSeconds(10));

			string serializedTemp = helper1.ToJsonString();
			var helper2 = RateHelper64.FromJsonString(serializedTemp, minDelta, maxDelta);

			AddSameToBoth(helper1, helper2, 20, start.AddSeconds(19));
			AddSameToBoth(helper1, helper2, 30, start.AddSeconds(27));

			// Act
			string serialized1 = helper1.ToJsonString();
			string serialized2 = helper2.ToJsonString();

			// Assert
			serialized1.Should().BeEquivalentTo(serialized2);
		}

		#endregion

		private static void AddSameToBoth(RateHelper64 helper1, RateHelper64 helper2, ulong newCounter, DateTime time)
		{
			helper1.Calculate(newCounter, time);
			helper2.Calculate(newCounter, time);
		}
	}
}