using Microsoft.VisualStudio.TestTools.UnitTesting;
using TotpApp.Models;

namespace TotpApp.Tests;

[TestClass]
public class OtpAuthUriParserTests
{
    [TestMethod]
    public void TryParse_ValidUri_QueryIssuerTakesPriorityOverLabel()
    {
        const string uri = "otpauth://totp/LabelIssuer:elf-engineer@example.com" +
                            "?secret=JBSWY3DPEHPK3PXP&issuer=QueryIssuer&digits=6&period=30&algorithm=SHA1";

        var success = OtpAuthUriParser.TryParse(uri, out var account);

        Assert.IsTrue(success);
        Assert.IsNotNull(account);
        Assert.AreEqual("QueryIssuer", account!.Issuer);
        Assert.AreEqual("elf-engineer@example.com", account.AccountName);
        Assert.AreEqual("JBSWY3DPEHPK3PXP", account.SecretBase32);
        Assert.AreEqual(6, account.Digits);
        Assert.AreEqual(30, account.PeriodSeconds);
        Assert.AreEqual("SHA1", account.Algorithm);
    }

    [TestMethod]
    public void TryParse_NoQueryIssuer_FallsBackToLabelIssuer()
    {
        const string uri = "otpauth://totp/GitHub:elf-engineer@example.com?secret=JBSWY3DPEHPK3PXP";

        var success = OtpAuthUriParser.TryParse(uri, out var account);

        Assert.IsTrue(success);
        Assert.AreEqual("GitHub", account!.Issuer);
        Assert.AreEqual("elf-engineer@example.com", account.AccountName);
    }

    [TestMethod]
    public void TryParse_NoIssuerAtAll_LeavesIssuerEmptyAndUsesFullLabelAsAccountName()
    {
        const string uri = "otpauth://totp/elf-engineer@example.com?secret=JBSWY3DPEHPK3PXP";

        var success = OtpAuthUriParser.TryParse(uri, out var account);

        Assert.IsTrue(success);
        Assert.AreEqual(string.Empty, account!.Issuer);
        Assert.AreEqual("elf-engineer@example.com", account.AccountName);
    }

    [TestMethod]
    public void TryParse_MissingSecret_ReturnsFalse()
    {
        const string uri = "otpauth://totp/GitHub:elf-engineer@example.com?issuer=GitHub";

        var success = OtpAuthUriParser.TryParse(uri, out var account);

        Assert.IsFalse(success);
        Assert.IsNull(account);
    }

    [TestMethod]
    public void TryParse_HotpScheme_ReturnsFalse()
    {
        const string uri = "otpauth://hotp/GitHub:elf-engineer@example.com?secret=JBSWY3DPEHPK3PXP&counter=0";

        var success = OtpAuthUriParser.TryParse(uri, out var account);

        Assert.IsFalse(success);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("not-a-uri")]
    [DataRow("https://example.com")]
    public void TryParse_InvalidInput_ReturnsFalse(string? input)
    {
        var success = OtpAuthUriParser.TryParse(input, out var account);

        Assert.IsFalse(success);
        Assert.IsNull(account);
    }

    [TestMethod]
    public void TryParse_MissingDigitsAndPeriod_UsesDefaults()
    {
        const string uri = "otpauth://totp/GitHub:elf-engineer@example.com?secret=JBSWY3DPEHPK3PXP";

        var success = OtpAuthUriParser.TryParse(uri, out var account);

        Assert.IsTrue(success);
        Assert.AreEqual(6, account!.Digits);
        Assert.AreEqual(30, account.PeriodSeconds);
        Assert.AreEqual("SHA1", account.Algorithm);
    }

    [TestMethod]
    public void TryParse_SecretWithSpacesAndLowercase_IsNormalized()
    {
        const string uri = "otpauth://totp/GitHub:elf-engineer@example.com?secret=jbsw%20y3dp%20ehpk%203pxp";

        var success = OtpAuthUriParser.TryParse(uri, out var account);

        Assert.IsTrue(success);
        Assert.AreEqual("JBSWY3DPEHPK3PXP", account!.SecretBase32);
    }
}
