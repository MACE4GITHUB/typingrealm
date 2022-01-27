using System;
using System.Linq;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.Tests;

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
        // Consider not allowing nullable structs, throw ArgumentNull exception.
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
    public void GetHashCode_ShouldBeDifferent_ForDifferentTypes(TestPrimitive<string> sut)
    {
        var anotherTypeSut = new StringTestPrimitive(sut.Value);

        Assert.NotEqual(sut.GetHashCode(), anotherTypeSut.GetHashCode());
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
        var equalsMethods = typeof(Primitive<>).GetMethods()
            .Where(method => method.Name == nameof(Primitive<int>.Equals)
                && method.IsVirtual);

        var getHashCodeMethods = typeof(Primitive<>).GetMethods()
            .Where(method => method.Name == nameof(Primitive<int>.GetHashCode)
                && method.IsVirtual);

        Assert.True(equalsMethods.Any());
        Assert.True(getHashCodeMethods.Any());

        foreach (var equalsMethod in equalsMethods)
        {
            Assert.True(equalsMethod.IsFinal);
        }

        foreach (var getHashCodeMethod in getHashCodeMethods)
        {
            Assert.True(getHashCodeMethod.IsFinal);
        }
    }

    [Fact]
    public void ToStringShouldBeSealed()
    {
        Assert.True(typeof(Primitive<>).GetMethod(nameof(Identity.ToString))?.IsFinal);
    }

    [Fact]
    public void ShouldHaveEqualityComparer_Equals()
    {
        var sut1 = new StringTestPrimitive("value");
        var sut2 = new StringTestPrimitive("value");
        var sut3 = new StringTestPrimitive("other");

        Assert.True(sut1.Equals(sut1, sut2));
        Assert.True(sut3.Equals(sut1, sut2));
        Assert.False(sut3.Equals(sut3, sut2));
    }

    [Fact]
    public void ShouldHaveEqualityComparer_Equals_AndCheckForNulls()
    {
        var sut1 = new StringTestPrimitive("value");
        var sut3 = new StringTestPrimitive("other");

        Assert.False(sut1.Equals(sut1, null));
        Assert.False(sut1!.Equals(null, sut1));
        Assert.True(sut3.Equals((StringTestPrimitive?)null, (StringTestPrimitive?)null));
    }

    [Fact]
    public void ShouldHaveEqualityComparer_GetHashCode()
    {
        var sut1 = new StringTestPrimitive("value");
        var sut2 = new StringTestPrimitive("value");
        var sut3 = new StringTestPrimitive("other");

        Assert.Equal(sut1.GetHashCode(), sut1.GetHashCode(sut2));
        Assert.Equal(sut1.GetHashCode(), sut3.GetHashCode(sut2));
        Assert.NotEqual(sut1.GetHashCode(), sut1.GetHashCode(sut3));
    }
}
