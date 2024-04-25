using FluentAssertions;
using Web.Types;

namespace Web.UnitTests.Types;
internal class SnowflakeTests
{
    [Test]
    public void Snowflake_CanBeCreated()
    {
        var _ = new Snowflake(0);
    }

    [Test]
    public void Snowflake__TryParse_CanParseFromString()
    {
        Snowflake.TryParse("1233076034211942421", out var value).Should().BeTrue();
        value.Value.Should().Be(1233076034211942421);
    }

    [Test]
    [TestCase("12330760342119424217")] // Value is 1 character too long
    [TestCase("123307603421194242A")] // Value contains non-numeric characters
    public void Snowflake_TryParse_ReturnsFalseForInvalidValue(string strValue)
    {
        Snowflake.TryParse(strValue, out var value).Should().BeFalse();
        value.Value.Should().Be(0);
    }
}
