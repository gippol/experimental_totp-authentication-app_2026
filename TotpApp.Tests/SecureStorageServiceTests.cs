using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TotpApp.Models;
using TotpApp.Services;

namespace TotpApp.Tests;

[TestClass]
public class SecureStorageServiceTests
{
    private readonly FakeDataProtector _protector = new();

    [TestMethod]
    public void SerializeThenDeserialize_RoundTrips_AllFields()
    {
        var accounts = new List<Account>
        {
            new()
            {
                Issuer = "GitHub",
                AccountName = "elf-engineer@example.com",
                SecretBase32 = "JBSWY3DPEHPK3PXP",
                Digits = 6,
                PeriodSeconds = 30,
                Algorithm = "SHA1",
            },
            new()
            {
                Issuer = "AWS",
                AccountName = "elf-engineer@example.com",
                SecretBase32 = "KRSXG5CTMVRXEZLU",
                Digits = 8,
                PeriodSeconds = 60,
                Algorithm = "SHA256",
            },
        };

        var protectedBytes = SecureStorageService.SerializeAccounts(accounts, _protector);
        var restored = SecureStorageService.DeserializeAccounts(protectedBytes, _protector);

        Assert.AreEqual(accounts.Count, restored.Count);
        for (var i = 0; i < accounts.Count; i++)
        {
            Assert.AreEqual(accounts[i].Issuer, restored[i].Issuer);
            Assert.AreEqual(accounts[i].AccountName, restored[i].AccountName);
            Assert.AreEqual(accounts[i].SecretBase32, restored[i].SecretBase32);
            Assert.AreEqual(accounts[i].Digits, restored[i].Digits);
            Assert.AreEqual(accounts[i].PeriodSeconds, restored[i].PeriodSeconds);
            Assert.AreEqual(accounts[i].Algorithm, restored[i].Algorithm);
        }
    }

    [TestMethod]
    public void DeserializeAccounts_EmptyList_ReturnsEmptyList()
    {
        var protectedBytes = SecureStorageService.SerializeAccounts(new List<Account>(), _protector);

        var restored = SecureStorageService.DeserializeAccounts(protectedBytes, _protector);

        Assert.AreEqual(0, restored.Count);
    }

    [TestMethod]
    public void SerializeAccounts_CallsProtector_NotRawJsonBytes()
    {
        // FakeDataProtectorは素通しだが、実装がProtectを呼んでいることを別のprotectorで確認する。
        var recordingProtector = new RecordingDataProtector();
        var accounts = new List<Account> { new() { Issuer = "X", SecretBase32 = "AAAAAAAA" } };

        SecureStorageService.SerializeAccounts(accounts, recordingProtector);

        Assert.IsTrue(recordingProtector.ProtectWasCalled);
    }

    private sealed class RecordingDataProtector : IDataProtector
    {
        public bool ProtectWasCalled { get; private set; }

        public byte[] Protect(byte[] plainBytes)
        {
            ProtectWasCalled = true;
            return plainBytes;
        }

        public byte[] Unprotect(byte[] protectedBytes) => protectedBytes;
    }
}
