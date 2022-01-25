using System;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Common.Tests;

public class PrimitiveTests : TestsBase
{
    public class TestPrimitive<TValue> : Primitive<TValue>
    {
        public TestPrimitive(TValue value) : base(value)
        {
        }
    }

    public class StringTestPrimitive : TestPrimitive<string>
    {
        public StringTestPrimitive(string value) : base(value)
        {
        }
    }

    [Fact]
    public void ShouldAllowDefaultValueType()
    {
        int value = default;
        Assert.Equal(value, new TestPrimitive<int>(value).Value);
    }

    [Fact]
    public void ShouldThrow_WhenValueIsNull()
    {
        string value = default!;

        Assert.Throws<ArgumentNullException>(
            () => new TestPrimitive<string>(value));
    }

    [Fact]
    public void ShouldNotThrow_WhenExplicitelyNullableStructIsNull()
    {
        // TODO: Do not allow nullable structs, throw ArgumentNull exception.
        int? value = null;

        Assert.Null(new TestPrimitive<int?>(value).Value);
    }

    [Theory, AutoMoqData]
    public void ShouldSetAndGetValue(string value)
    {
        Assert.Equal(value, new TestPrimitive<string>(value).Value);
    }

    [Theory, AutoMoqData]
    public void ShouldConvertToStringAsValue(
        TestPrimitive<string> stringSut,
        TestPrimitive<int> intSut)
    {
        Assert.Equal(stringSut.Value, stringSut.ToString());
        Assert.Equal(intSut.Value.ToString(), intSut.ToString());
    }

    [Theory, AutoMoqData]
    public void ShouldGetTheSameHashCodeAsValue(TestPrimitive<string> sut)
    {
        Assert.Equal(sut.Value.GetHashCode(), sut.GetHashCode());
    }

    [Theory, AutoMoqData]
    public void ShouldCompareByValue(TestPrimitive<string> sut)
    {
        var sameSut = new TestPrimitive<string>(sut.Value);
        var differentSut = new TestPrimitive<string>(sut.Value + Create<string>());

        Assert.False(ReferenceEquals(sut, sameSut));
        Assert.Equal(sut, sameSut);
        Assert.NotEqual(sut, differentSut);
    }

    [Theory, AutoMoqData]
    public void ShouldNotConsiderDifferentTypesEqual(TestPrimitive<string> sut)
    {
        var anotherTypeSut = new StringTestPrimitive(sut.Value);

        Assert.Equal(sut.Value, anotherTypeSut.Value);
        Assert.NotEqual(sut, anotherTypeSut);
    }

    [Theory, AutoMoqData]
    public void GetHashCode_ReturnsTheSameResultForDifferentTypes(TestPrimitive<string> sut)
    {
        // TODO: Change this behavior or DO NOT EVER put the same primitives in the same collection.
        var anotherTypeSut = new StringTestPrimitive(sut.Value);

        Assert.Equal(sut.GetHashCode(), anotherTypeSut.GetHashCode());
    }

    [Theory, AutoMoqData]
    public void ShouldOverrideEqualityOperators(TestPrimitive<string> sut)
    {
        var sameSut = new TestPrimitive<string>(sut.Value);
        var differentSut = new TestPrimitive<string>(sut.Value + Create<string>());

        Assert.True(sut == sameSut);
        Assert.True(sameSut == sut);
        Assert.False(sut == differentSut);
        Assert.False(differentSut == sut);

        Assert.False(sut != sameSut);
        Assert.False(sameSut != sut);
        Assert.True(sut != differentSut);
        Assert.True(differentSut != sut);

        Assert.False(sut == null);
        Assert.False(null == sut);
        Assert.True((TestPrimitive<string>)null! == (TestPrimitive<string>)null!);

        Assert.True(sut != null);
        Assert.True(null != sut);
        Assert.False((TestPrimitive<string>)null! != (TestPrimitive<string>)null!);
    }

    [Theory, AutoMoqData]
    public void ShouldImplicitlyConvertToValue(
        TestPrimitive<string> stringSut,
        TestPrimitive<int> intSut)
    {
        string stringValue = stringSut;
        int intValue = intSut;

        Assert.Equal(stringSut.Value, stringValue);
        Assert.Equal(intSut.Value, intValue);
    }

    [Fact]
    public void EqualityShouldBeSealed()
    {
        Assert.True(typeof(Primitive<>).GetMethod(nameof(Primitive<int>.Equals))?.IsFinal);
        Assert.True(typeof(Primitive<>).GetMethod(nameof(Primitive<int>.GetHashCode))?.IsFinal);
    }

    [Fact]
    public void ToStringShouldBeSealed()
    {
        Assert.True(typeof(Primitive<>).GetMethod(nameof(Identity.ToString))?.IsFinal);
    }
}
