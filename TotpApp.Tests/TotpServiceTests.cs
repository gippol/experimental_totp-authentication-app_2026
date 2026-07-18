using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TotpApp.Models;
using TotpApp.Services;

namespace TotpApp.Tests;

[TestClass]
public class TotpServiceTests
{
    private readonly TotpService _sut = new();

    private static Account CreateAccount(int periodSeconds = 30, int digits = 6) => new()
    {
        Issuer = "TestIssuer",
        AccountName = "test@example.com",
        SecretBase32 = "JBSWY3DPEHPK3PXP",
        Digits = digits,
        PeriodSeconds = periodSeconds,
        Algorithm = "SHA1",
    };

    [TestMethod]
    public void ComputeCode_ReturnsStringOfConfiguredDigitLength()
    {
        var account = CreateAccount(digits: 6);

        var code = _sut.ComputeCode(account, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        Assert.AreEqual(6, code.Length);
        Assert.IsTrue(long.TryParse(code, out _), "コードは数字のみで構成されるべき");
    }

    [TestMethod]
    public void ComputeCode_IsDeterministic_ForSameTimestamp()
    {
        var account = CreateAccount();
        var timestamp = new DateTime(2026, 1, 1, 0, 0, 10, DateTimeKind.Utc);

        var code1 = _sut.ComputeCode(account, timestamp);
        var code2 = _sut.ComputeCode(account, timestamp);

        Assert.AreEqual(code1, code2);
    }

    [TestMethod]
    public void ComputeCode_IsSame_WithinSamePeriodWindow()
    {
        var account = CreateAccount(periodSeconds: 30);

        // 同じ30秒枠(0〜29秒)内であればコードは変わらないはず
        var codeAtStart = _sut.ComputeCode(account, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var codeNearEnd = _sut.ComputeCode(account, new DateTime(2026, 1, 1, 0, 0, 29, DateTimeKind.Utc));

        Assert.AreEqual(codeAtStart, codeNearEnd);
    }

    [TestMethod]
    public void ComputeCode_ChangesAcrossPeriodBoundary()
    {
        var account = CreateAccount(periodSeconds: 30);

        var codeBeforeBoundary = _sut.ComputeCode(account, new DateTime(2026, 1, 1, 0, 0, 29, DateTimeKind.Utc));
        var codeAfterBoundary = _sut.ComputeCode(account, new DateTime(2026, 1, 1, 0, 0, 30, DateTimeKind.Utc));

        Assert.AreNotEqual(codeBeforeBoundary, codeAfterBoundary);
    }

    [TestMethod]
    public void GetSecondsRemaining_AtStartOfPeriod_IsCloseToFullPeriod()
    {
        var account = CreateAccount(periodSeconds: 30);

        var remaining = _sut.GetSecondsRemaining(account, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        Assert.AreEqual(30, remaining, delta: 1.0);
    }

    [TestMethod]
    public void GetSecondsRemaining_NearEndOfPeriod_IsSmall()
    {
        var account = CreateAccount(periodSeconds: 30);

        var remaining = _sut.GetSecondsRemaining(account, new DateTime(2026, 1, 1, 0, 0, 28, DateTimeKind.Utc));

        Assert.IsTrue(remaining <= 2.0, $"期待: 2秒以下, 実際: {remaining}");
    }
}
