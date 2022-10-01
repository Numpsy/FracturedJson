using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using FracturedJson;

namespace Tests;

[TestClass]
public class ObjectSerializationTests
{
    [TestMethod]
    public void TestSerializeCalendar()
    {
        var input = CultureInfo.InvariantCulture.Calendar;
        var fracturedJsonOptions = new FracturedJsonOptions();
        var serialOptions = new JsonSerializerOptions()
        {
            Converters = { new JsonStringEnumConverter() }
        };

        var formatter = new Formatter() { Options = fracturedJsonOptions };
        var output = formatter.Serialize(input, 0, serialOptions);

        StringAssert.Contains(output, "\"Eras\": [1]");
        StringAssert.Contains(output, "SolarCalendar");
    }

    [TestMethod]
    public void TestSerializeNumberFormat()
    {
        var input = CultureInfo.InvariantCulture.NumberFormat;
        var opts = new FracturedJsonOptions();

        var formatter = new Formatter() { Options = opts };
        var output = formatter.Serialize(input, 0);

        StringAssert.Contains(output, "\"CurrencySymbol\": \"\\u00A4\"");
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    [DataRow(null)]
    [DataRow("sandwich")]
    [DataRow(3.14)]
    [DataRow(98765432109876543210D)]
    [DataRow(9876543210987654321L)]
    public void TestSerializeSimple(object? val)
    {
        var opts = new FracturedJsonOptions();

        var formatter = new Formatter() { Options = opts };
        var output = formatter.Serialize(val, 0);

        var expected = JsonSerializer.Serialize(val);
        Assert.AreEqual(expected, output.TrimEnd());
    }
}
