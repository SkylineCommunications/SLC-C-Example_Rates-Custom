using System;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Skyline.Protocol.Rates.Tests
{
	[TestClass()]
	public class RateHelper64Tests
	{
		private readonly TimeSpan minDelta = new TimeSpan(0, 1, 0);
		private readonly TimeSpan maxDelta = new TimeSpan(1, 0, 0);
		private const double faultyReturn = -1;

		#region CalculateWithDateTime

		[TestMethod()]
		public void CalculateWithDateTimeInvalid_BackInTime()
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
		public void CalculateWithDateTimeInvalid_TooLate()
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
		public void CalculateWithDateTimeInvalid_TooSoon()
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
		public void CalculateWithDateTimeValid_ToOlderCounter()
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
			double expectedRate = (50.0 - 5.0) / 100d;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void CalculateWithDateTimeValid_ToPreviousCounter()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper = RateHelper64.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(5, start);

			// Act
			double rate = helper.Calculate(50, start.AddSeconds(100));

			// Assert
			double expectedRate = (50.0 - 5.0) / 100d;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void CalculateWithDateTimeValid_WithOverflow()
		{
			// Arrange
			DateTime start = new DateTime(2000, 1, 1);
			var helper = RateHelper64.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(UInt64.MaxValue - 10, start);

			// Act
			double rate = helper.Calculate(9, start.AddSeconds(100));

			// Assert
			double expectedRate = 20 / 100d;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		#endregion

		#region CalculateWithTimeSpan

		[TestMethod()]
		public void CalculateWithTimeSpanInvalid_BackInTime()
		{
			// Arrange
			var helper = RateHelper64.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(10, new TimeSpan(0, 0, 10));

			// Act
			double rate = helper.Calculate(20, new TimeSpan(0, 0, -10));

			// Assert
			double expectedRate = faultyReturn;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void CalculateWithTimeSpanInvalid_TooLate()
		{
			// Arrange
			var helper = RateHelper64.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(10, new TimeSpan(0, 0, 10));

			// Act
			double rate = helper.Calculate(20, new TimeSpan(1, 1, 0));

			// Assert
			double expectedRate = faultyReturn;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void CalculateWithTimeSpanInvalid_TooSoon()
		{
			// Arrange
			var helper = RateHelper64.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(10, new TimeSpan(0, 0, 0));

			// Act
			double rate = helper.Calculate(20, new TimeSpan(0, 0, 5));

			// Assert
			double expectedRate = faultyReturn;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void CalculateWithTimeSpanValid_ToOlderCounter()
		{
			// Arrange
			var helper = RateHelper64.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(5, new TimeSpan(0, 0, 0));
			helper.Calculate(10, new TimeSpan(0, 0, 90));   // 1m30s
			helper.Calculate(20, new TimeSpan(0, 0, 1));    // 1m31s
			helper.Calculate(30, new TimeSpan(0, 0, 1));    // 1m32s
			helper.Calculate(40, new TimeSpan(0, 0, 1));    // 1m33s

			// Act
			double rate = helper.Calculate(50, new TimeSpan(0, 0, 7));  // 1m40s

			// Assert
			double expectedRate = (50.0 - 5.0) / (7 + 1 + 1 + 1 + 90);
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void CalculateWithTimeSpanValid_ToPreviousCounter()
		{
			// Arrange
			var helper = RateHelper64.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(5, new TimeSpan(0, 0, 10));

			// Act
			double rate = helper.Calculate(50, new TimeSpan(0, 0, 100));

			// Assert
			double expectedRate = (50.0 - 5.0) / 100d;
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void CalculateWithTimeSpanValid_ToPreviousCounter_PerDay()
		{
			// Arrange
			var helper = RateHelper64.FromJsonString("", minDelta, maxDelta, RateBase.Day);

			helper.Calculate(5, new TimeSpan(0, 0, 10));

			// Act
			double rate = helper.Calculate(50, new TimeSpan(0, 0, 100));

			// Assert
			UInt64 counterIncrease = 50 - 5;
			double expectedRate = counterIncrease / (100d / 60 / 60 / 24);
			////Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
			rate.Should().BeApproximately(expectedRate, Math.Pow(10, -9));
		}

		[TestMethod()]
		public void CalculateWithTimeSpanValid_ToPreviousCounter_PerHour()
		{
			// Arrange
			var helper = RateHelper64.FromJsonString("", minDelta, maxDelta, RateBase.Hour);

			helper.Calculate(5, new TimeSpan(0, 0, 10));

			// Act
			double rate = helper.Calculate(50, new TimeSpan(0, 0, 100));

			// Assert
			double expectedRate = (50.0 - 5.0) / (100d / 60 / 60);
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void CalculateWithTimeSpanValid_ToPreviousCounter_PerMinute()
		{
			// Arrange
			var helper = RateHelper64.FromJsonString("", minDelta, maxDelta, RateBase.Minute);

			helper.Calculate(5, new TimeSpan(0, 0, 10));

			// Act
			double rate = helper.Calculate(50, new TimeSpan(0, 0, 100));

			// Assert
			double expectedRate = (50.0 - 5.0) / (100d / 60);
			Assert.IsTrue(rate == expectedRate, "rate '" + rate + "' != expectedRate '" + expectedRate + "'");
		}

		[TestMethod()]
		public void CalculateWithTimeSpanValid_WithOverflow()
		{
			// Arrange
			var helper = RateHelper64.FromJsonString("", minDelta, maxDelta);

			helper.Calculate(UInt64.MaxValue - 10, new TimeSpan(0, 0, 0));

			// Act
			double rate = helper.Calculate(9, new TimeSpan(0, 0, 100));

			// Assert
			double expectedRate = 20 / 100d;
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
		public void Serialize_Invalid_DifferentDateTime()
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
		public void Serialize_Invalid_DifferentTimeSpan()
		{
			// Arrange
			var helper1 = RateHelper64.FromJsonString("", minDelta, maxDelta);
			helper1.Calculate(5, new TimeSpan(0, 0, 0));
			helper1.Calculate(10, new TimeSpan(0, 0, 10));

			string serializedTemp = helper1.ToJsonString();
			var helper2 = RateHelper64.FromJsonString(serializedTemp, minDelta, maxDelta);

			// same counter, different timing
			helper1.Calculate(20, new TimeSpan(0, 0, 9));
			helper2.Calculate(20, new TimeSpan(0, 0, 2));

			// Act
			string serialized1 = helper1.ToJsonString();
			string serialized2 = helper2.ToJsonString();

			// Assert
			serialized1.Should().NotBeEquivalentTo(serialized2);
		}

		[TestMethod()]
		public void Serialize_Valid_DateTime()
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

		[TestMethod()]
		public void Serialize_Valid_TimeSpan()
		{
			// Arrange
			var helper1 = RateHelper64.FromJsonString("", minDelta, maxDelta);
			helper1.Calculate(5, new TimeSpan(0, 0, 0));
			helper1.Calculate(10, new TimeSpan(0, 0, 10));

			string serializedTemp = helper1.ToJsonString();
			var helper2 = RateHelper64.FromJsonString(serializedTemp, minDelta, maxDelta);

			AddSameToBoth(helper1, helper2, 20, new TimeSpan(0, 0, 9));
			AddSameToBoth(helper1, helper2, 30, new TimeSpan(0, 0, 8));

			// Act
			string serialized1 = helper1.ToJsonString();
			string serialized2 = helper2.ToJsonString();

			// Assert
			serialized1.Should().BeEquivalentTo(serialized2);
		}

		#endregion

		#region HelperMethods
		private static void AddSameToBoth(RateHelper64 helper1, RateHelper64 helper2, ulong newCounter, DateTime time)
		{
			helper1.Calculate(newCounter, time);
			helper2.Calculate(newCounter, time);
		}

		private static void AddSameToBoth(RateHelper64 helper1, RateHelper64 helper2, ulong newCounter, TimeSpan time)
		{
			helper1.Calculate(newCounter, time);
			helper2.Calculate(newCounter, time);
		}
		#endregion
	}
}