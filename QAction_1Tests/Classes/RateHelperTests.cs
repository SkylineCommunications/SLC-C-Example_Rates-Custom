namespace Skyline.Protocol.Rates.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;

    [TestClass()]
    public class RateHelperTests
    {
        [TestMethod()]
        public void FromJsonString_Empty()
        {
            var input = String.Empty;

            var result = RateHelper.FromJsonString(input, minDelta: new TimeSpan(0, 1, 30), maxDelta: new TimeSpan(0, 10, 0));

            Assert.IsTrue(!result.Counters.Any());
        }

        [TestMethod()]
        public void FromJsonString_Valid()
        {
            var input = @"{""minDelta"":""00:01:30"",""maxDelta"":""00:10:00"",""counters"":[{""Counter"":132,""Time"":""2021-12-03T11:57:00.6515895+00:00""},{""Counter"":135,""Time"":""2021-12-03T11:58:00.6737114+00:00""},{""Counter"":138,""Time"":""2021-12-03T11:59:00.6566613+00:00""}]}";

            var result = RateHelper.FromJsonString(input, minDelta: new TimeSpan(0, 1, 30), maxDelta: new TimeSpan(0, 10, 0));

            Assert.IsTrue(result.Counters.Any());
        }


        [TestMethod()]
        public void ToJsonString_Empty()
        {
            var input = String.Empty;

            var helper = RateHelper.FromJsonString(input, minDelta: new TimeSpan(0, 1, 30), maxDelta: new TimeSpan(0, 10, 0));
            var result = helper.ToJsonString();

            Assert.IsTrue(!String.IsNullOrWhiteSpace(result));
        }

        [TestMethod()]
        public void ToJsonString_Valid()
        {
            var input = @"{""minDelta"":""00:01:30"",""maxDelta"":""00:10:00"",""counters"":[{""Counter"":132,""Time"":""2021-12-03T11:57:00.6515895+00:00""},{""Counter"":135,""Time"":""2021-12-03T11:58:00.6737114+00:00""},{""Counter"":138,""Time"":""2021-12-03T11:59:00.6566613+00:00""}]}";

            var helper = RateHelper.FromJsonString(input, minDelta: new TimeSpan(0, 1, 30), maxDelta: new TimeSpan(0, 10, 0));
            var result = helper.ToJsonString();

            Assert.IsTrue(!String.IsNullOrWhiteSpace(result));
        }

        [TestMethod()]
        public void Reset_Valid()
        {
            var input = @"{""minDelta"":""00:01:30"",""maxDelta"":""00:10:00"",""counters"":[{""Counter"":132,""Time"":""2021-12-03T11:57:00.6515895+00:00""},{""Counter"":135,""Time"":""2021-12-03T11:58:00.6737114+00:00""},{""Counter"":138,""Time"":""2021-12-03T11:59:00.6566613+00:00""}]}";

            var result = RateHelper.FromJsonString(input, minDelta: new TimeSpan(0, 1, 30), maxDelta: new TimeSpan(0, 10, 0));
            result.Reset();

            Assert.IsTrue(!result.Counters.Any());
        }

        [TestMethod()]
        public void Calculate_ValidOctet()
        {
            var input = @"{""minDelta"":""00:01:30"",""maxDelta"":""00:10:00"",""counters"":[{""Counter"":132,""Time"":""2021-12-03T11:57:00.6515895+00:00""},{""Counter"":135,""Time"":""2021-12-03T11:58:00.6737114+00:00""},{""Counter"":138,""Time"":""2021-12-03T11:59:00.6566613+00:00""}]}";

            var helper = RateHelper.FromJsonString(input, minDelta: new TimeSpan(0, 1, 30), maxDelta: new TimeSpan(0, 10, 0));
            var result = helper.Calculate(UInt32.MaxValue, DateTime.Now, true);

            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void Calculate_ValidNotOctet()
        {
            var input = @"{""minDelta"":""00:01:30"",""maxDelta"":""00:10:00"",""counters"":[{""Counter"":132,""Time"":""2021-12-03T11:57:00.6515895+00:00""},{""Counter"":135,""Time"":""2021-12-03T11:58:00.6737114+00:00""},{""Counter"":138,""Time"":""2021-12-03T11:59:00.6566613+00:00""}]}";

            var helper = RateHelper.FromJsonString(input, minDelta: new TimeSpan(0, 1, 30), maxDelta: new TimeSpan(0, 10, 0));
            var result = helper.Calculate(UInt32.MaxValue, DateTime.Now, false);

            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void Calculate_Valid()
        {
            var input = @"{""minDelta"":""00:01:30"",""maxDelta"":""00:10:00"",""counters"":[{""Counter"":132,""Time"":""2021-12-03T11:57:00.6515895+00:00""},{""Counter"":135,""Time"":""2021-12-03T11:58:00.6737114+00:00""},{""Counter"":138,""Time"":""2021-12-03T11:59:00.6566613+00:00""}]}";

            var helper = RateHelper.FromJsonString(input, minDelta: new TimeSpan(0, 1, 30), maxDelta: new TimeSpan(0, 10, 0));
            var result = helper.Calculate(UInt32.MaxValue, DateTime.Now, faultyReturn: -1);

            Assert.IsNotNull(result);
        }
    }
}