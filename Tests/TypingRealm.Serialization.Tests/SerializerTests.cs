using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Serialization.Tests;

public class SerializerTests : TestsBase
{
#pragma warning disable CS8618
    public class TestClass
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
        public TestClass NestedClass { get; set; }
        public TestEnum Enum { get; set; }
    }

    public enum TestEnum
    {
        FirstValue = 1,
        SecondValue = 2
    }
#pragma warning restore CS8618

    [Theory, AutoMoqData]
    public void Serializer_ShouldSerializeToJson(Serializer sut)
    {
        var json = sut.Serialize(new TestClass
        {
            StringValue = "value",
            IntValue = 99,
            NestedClass = new TestClass
            {
                StringValue = "value2"
            },
            Enum = TestEnum.SecondValue
        });

        Assert.Equal(@"{""stringValue"":""value"",""intValue"":99,""nestedClass"":{""stringValue"":""value2"",""intValue"":0,""nestedClass"":null,""enum"":0},""enum"":""SecondValue""}", json);
    }

    [Theory, AutoMoqData]
    public void Serializer_ShouldDeserializeFromJson(Serializer sut)
    {
        var json = @"{""stringValue"":""value"",""intValue"":99,""nestedClass"":{""stringValue"":""value2"",""intValue"":0,""nestedClass"":null,""enum"":0},""enum"":""SecondValue""}";

        var result1 = sut.Deserialize<TestClass>(json);
        Assert.NotNull(result1);
        Assert.Equal("value", result1!.StringValue);
        Assert.Equal(99, result1.IntValue);
        Assert.NotNull(result1.NestedClass);
        Assert.Equal("value2", result1.NestedClass.StringValue);
        Assert.Equal(0, result1.NestedClass.IntValue);
        Assert.Null(result1.NestedClass.NestedClass);
        Assert.Equal((TestEnum)0, result1.NestedClass.Enum);
        Assert.Equal(TestEnum.SecondValue, result1.Enum);

        var result2Obj = sut.Deserialize(json, typeof(TestClass));
        Assert.IsType<TestClass>(result2Obj);
        var result2 = result2Obj as TestClass;

        Assert.NotNull(result2);
        Assert.Equal("value", result2!.StringValue);
        Assert.Equal(99, result2.IntValue);
        Assert.NotNull(result2.NestedClass);
        Assert.Equal("value2", result2.NestedClass.StringValue);
        Assert.Equal(0, result2.NestedClass.IntValue);
        Assert.Null(result2.NestedClass.NestedClass);
        Assert.Equal((TestEnum)0, result2.NestedClass.Enum);
        Assert.Equal(TestEnum.SecondValue, result2.Enum);
    }

    [Theory, AutoMoqData]
    public void Serializer_ShouldSerializeAndDeserializeStrings(
        Serializer sut, string value)
    {
        var data = sut.Serialize<string>(value);
        Assert.Equal($"\"{value}\"", data);

        var deserialized = sut.Deserialize<string>($"\"{value}\"");
        Assert.Equal(value, deserialized);
    }

    [Theory, AutoMoqData]
    public void Serializer_ShouldDeserializeStrings_NonGeneric(
        Serializer sut, string value)
    {
        var deserialized = sut.Deserialize($"\"{value}\"", typeof(string));
        Assert.Equal(value, deserialized);
    }

    [Theory, AutoMoqData]
    public void Serializer_ShouldDeserializeRawString(
        Serializer sut, string value)
    {
        var deserialized = sut.Deserialize<string>($"{value}");
        Assert.Equal(value, deserialized);
    }

    [Theory, AutoMoqData]
    public void Serializer_ShouldDeserializeRawString_NonGeneric(
        Serializer sut, string value)
    {
        var deserialized = sut.Deserialize($"{value}", typeof(string));
        Assert.Equal(value, deserialized);
    }

    [Theory, AutoMoqData]
    public void Serializer_ShouldSerializeAndDeserializePrimitives(
        Serializer sut, int value)
    {
        var data = sut.Serialize<int>(value);
        Assert.Equal($"{value}", data);

        var deserialized = sut.Deserialize<int>($"{value}");
        Assert.Equal(value, deserialized);
    }

    [Theory, AutoMoqData]
    public void Serializer_ShouldDeserializePrimitives_NonGeneric(
        Serializer sut, int value)
    {
        var deserialized = sut.Deserialize($"{value}", typeof(int));
        Assert.Equal(value, deserialized);
    }

    [Theory, AutoMoqData]
    public void Serializer_ShouldSerializeAndDeserializeEnums(
        Serializer sut, TestEnum value)
    {
        var data = sut.Serialize<TestEnum>(value);
        Assert.Equal($"\"{value}\"", data);

        var deserialized = sut.Deserialize<TestEnum>($"\"{value}\"");
        Assert.Equal(value, deserialized);
    }

    [Theory, AutoMoqData]
    public void Serializer_ShouldSerializeAndDeserializeEnums_NonGeneric(
        Serializer sut, TestEnum value)
    {
        var deserialized = sut.Deserialize($"\"{value}\"", typeof(TestEnum));
        Assert.Equal(value, deserialized);
    }

    [Fact]
    public void AddSerialization_ShouldRegisterTransientSerializer()
    {
        var provider = GetServiceCollection().AddSerialization().BuildServiceProvider();
        provider.AssertRegisteredTransient<ISerializer, Serializer>();
    }
}
