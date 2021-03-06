// Licensed under the MIT license. See https://kieranties.mit-license.org/ for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using SimpleVersion.Pipeline.Formatting;
using Xunit;
using static SimpleVersion.Core.Tests.Utils;

namespace SimpleVersion.Core.Tests.Pipeline.Formatting
{
    public class Semver1FormatProcessFixture
    {
        private readonly Semver1FormatProcessor _sut;

        public Semver1FormatProcessFixture()
        {
            _sut = new Semver1FormatProcessor();
        }

        public static IEnumerable<object[]> LabelParts()
        {
            yield return new object[] { Array.Empty<object>(), "1.2.0", 10, "1.2.0" };
            yield return new object[] { new[] { "one" }, "1.2.0", 10, "1.2.0-one-0010" };
            yield return new object[] { new[] { "one", "two" }, "1.2.0", 106, "1.2.0-one-two-0106" };
            yield return new object[] { new[] { "*", "one", "two" }, "1.2.0", 106, "1.2.0-0106-one-two" };
            yield return new object[] { new[] { "one", "*", "two" }, "1.2.0", 106, "1.2.0-one-0106-two" };
            yield return new object[] { new[] { "one", "two*", "three" }, "1.2.0", 106, "1.2.0-one-two0106-three" };
            yield return new object[] { new[] { "one", "*two*", "three" }, "1.2.0", 106, "1.2.0-one-0106two0106-three" };
            yield return new object[] { new[] { "one", "*t*o*", "three" }, "1.2.0", 106, "1.2.0-one-0106t0106o0106-three" };
        }

        [Theory]
        [MemberData(nameof(LabelParts))]
        public void Process_LabelParts_NonRelease_Is_Formatted(
            string[] parts,
            string version,
            int height,
            string expectedPart)
        {
            // Arrange
            var context = new MockVersionContext
            {
                Configuration = GetRepositoryConfiguration(version, label: parts),
                Result = GetVersionResult(height, version, false)
            };

            var fullExpected = $"{expectedPart}-c{context.Result.Sha7}";

            // Act
            _sut.Process(context);

            // Assert
            context.Result.Formats.Should().ContainKey("Semver1");
            context.Result.Formats["Semver1"].Should().Be(fullExpected);
        }

        [Theory]
        [MemberData(nameof(LabelParts))]
        public void Process_LabelParts_Release_Is_Formatted(
            string[] parts,
            string version,
            int height,
            string expected)
        {
            // Arrange
            var context = new MockVersionContext
            {
                Configuration = GetRepositoryConfiguration(version, label: parts),
                Result = GetVersionResult(height, version, true)
            };

            // Act
            _sut.Process(context);

            // Assert
            context.Result.Formats.Should().ContainKey("Semver1");
            context.Result.Formats["Semver1"].Should().Be(expected);
        }
    }
}
