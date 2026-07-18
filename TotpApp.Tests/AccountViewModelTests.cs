using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TotpApp.Models;
using TotpApp.ViewModels;

namespace TotpApp.Tests;

[TestClass]
public class AccountViewModelTests
{
    private static Account CreateAccount() => new()
    {
        Issuer = "GitHub",
        AccountName = "elf-engineer@example.com",
        SecretBase32 = "JBSWY3DPEHPK3PXP",
        Digits = 6,
        PeriodSeconds = 30,
        Algorithm = "SHA1",
    };

    [TestMethod]
    public void Refresh_SetsCurrentCode_FromTotpService()
    {
        var fakeTotpService = new FakeTotpService { CodeToReturn = "123456" };
        var sut = new AccountViewModel(CreateAccount(), fakeTotpService);

        sut.Refresh(DateTime.UtcNow);

        Assert.AreEqual("123456", sut.CurrentCode);
    }

    [TestMethod]
    public void Refresh_FullPeriodRemaining_ProgressFractionIsOne()
    {
        var fakeTotpService = new FakeTotpService { SecondsRemainingToReturn = 30 };
        var sut = new AccountViewModel(CreateAccount(), fakeTotpService);

        sut.Refresh(DateTime.UtcNow);

        Assert.AreEqual(1.0, sut.ProgressFraction, delta: 0.0001);
    }

    [TestMethod]
    public void Refresh_HalfPeriodRemaining_ProgressFractionIsHalf()
    {
        var fakeTotpService = new FakeTotpService { SecondsRemainingToReturn = 15 };
        var sut = new AccountViewModel(CreateAccount(), fakeTotpService);

        sut.Refresh(DateTime.UtcNow);

        Assert.AreEqual(0.5, sut.ProgressFraction, delta: 0.0001);
    }

    [TestMethod]
    public void Refresh_SecondsRemainingAboveFive_IsExpiringSoonIsFalse()
    {
        var fakeTotpService = new FakeTotpService { SecondsRemainingToReturn = 6 };
        var sut = new AccountViewModel(CreateAccount(), fakeTotpService);

        sut.Refresh(DateTime.UtcNow);

        Assert.IsFalse(sut.IsExpiringSoon);
    }

    [TestMethod]
    public void Refresh_SecondsRemainingAtOrBelowFive_IsExpiringSoonIsTrue()
    {
        var fakeTotpService = new FakeTotpService { SecondsRemainingToReturn = 5 };
        var sut = new AccountViewModel(CreateAccount(), fakeTotpService);

        sut.Refresh(DateTime.UtcNow);

        Assert.IsTrue(sut.IsExpiringSoon);
    }

    [TestMethod]
    public void Issuer_And_AccountName_AreExposedFromUnderlyingAccount()
    {
        var account = CreateAccount();
        var sut = new AccountViewModel(account, new FakeTotpService());

        Assert.AreEqual(account.Issuer, sut.Issuer);
        Assert.AreEqual(account.AccountName, sut.AccountName);
    }
}
