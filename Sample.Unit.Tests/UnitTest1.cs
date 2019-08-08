using System;
using Xunit;
using Skyapi.Api;
using Skyapi.Client;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollector.InProcDataCollector;

namespace Sample.Unit.Tests
{
    public class UnitTest1
    {
        private string basePath = "http://localhost:6420";

        private struct Progress
        {
            public int current;
            public int highest;
            public string[] peer;
        }

        [Fact]
        public void TestVersion()
        {
            Configuration.Default.BasePath = basePath;
            var apiInstance = new DefaultApi(Configuration.Default);
            var result = apiInstance.Version();
            Assert.Equal("v0.26.0", result.Branch);
            Assert.Equal("ff754084df0912bc0d151529e2893ca86618fb3f", result.Commit);
            Assert.Equal("0.26.0", result.Version);
        }

        [Fact]
        public void TestWalletSeedVerify()
        {
            var apiInstance = new DefaultApi(Configuration.Default);
            apiInstance.Configuration.AddApiKeyPrefix("X-CSRF-TOKEN", GetCsrf());
            apiInstance.Configuration.AddDefaultHeader("Content-Type", "application/json");
            //Test with correct seed
            var result =
                apiInstance.WalletSeedVerify(
                    "nut wife logic sample addict shop before tobacco crisp bleak lawsuit affair");
            Assert.NotNull(result);
            //test with incorrect seed
            Assert.Throws<ApiException>(() => apiInstance.WalletSeedVerify("nut"));
        }

        [Fact]
        public void TestBalanceGet()
        {
            BalanceGetStable();
        }

        [Fact]
        public void TestAddressCount()
        {
            AddressCountStable();
        }

        [Fact]
        public void TestBlock()
        {
            BlockStable();
        }

        [Fact]
        public void TestBlockchainMetadata()
        {
            BlockchainMetadataStable();
        }

        [Fact]
        public void TestAddressUxouts()
        {
            AddressUxoutsStable();
        }

        [Fact]
        public void TestBlockchainProgress()
        {
            BlockChainProgressStable();
        }

        [Fact]
        public void TestBlocks()
        {
            BlocksStable();
        }

        [Fact]
        public void TestCoinSupply()
        {
            CoinSupplyStable();
        }

        [Fact]
        public void TestTransactionsGet()
        {
            TransactionsGetStable();
        }

        private void BalanceGetStable()
        {
            var apiInstance = new DefaultApi(basePath);
            var testCase = new[]
            {
                new
                {
                    name = "unknown address",
                    addrs = new[] {"prRXwTcDK24hs6AFxj69UuWae3LzhrsPW9"},
                    file = "balance-noaddrs.golden"
                },
                new
                {
                    name = "one address",
                    addrs = new[] {"2THDupTBEo7UqB6dsVizkYUvkKq82Qn4gjf"},
                    file = "balance-2THDupTBEo7UqB6dsVizkYUvkKq82Qn4gjf.golden"
                },
                new
                {
                    name = "duplicate address",
                    addrs = new[] {"2THDupTBEo7UqB6dsVizkYUvkKq82Qn4gjf", "2THDupTBEo7UqB6dsVizkYUvkKq82Qn4gjf"},
                    file = "balance-2THDupTBEo7UqB6dsVizkYUvkKq82Qn4gjf.golden"
                },
                new
                {
                    name = "two address",
                    addrs = new[] {"2THDupTBEo7UqB6dsVizkYUvkKq82Qn4gjf", "212mwY3Dmey6vwnWpiph99zzCmopXTqeVEN"},
                    file = "balance-two-addrs.golden"
                }
            };
            foreach (var tc in testCase)
            {
                var result = apiInstance.BalanceGet(string.Join(",", tc.addrs));
                CheckGoldenFile(tc.file, result);
            }
        }

        private void AddressCountStable()
        {
            var apiInstance = new DefaultApi(basePath);
            var result = apiInstance.AddressCount();
            Assert.Equal(result.Count, 155);
        }

        private void BlockStable()
        {
            var apiInstance = new DefaultApi(basePath);
            var testCases = new[]
            {
                new
                {
                    name = "unknown hash",
                    golden = "",
                    hash = "80744ec25e6233f40074d35bf0bfdbddfac777869b954a96833cb89f44204444",
                    seq = -1,
                    errCode = 404,
                    errMsg = "Error calling Block: 404 Not Found\n"
                },
                new
                {
                    name = "valid hash",
                    golden = "block-hash.golden",
                    hash = "70584db7fb8ab88b8dbcfed72ddc42a1aeb8c4882266dbb78439ba3efcd0458d",
                    seq = -1,
                    errCode = 200,
                    errMsg = ""
                },
                new
                {
                    name = "genesis hash",
                    golden = "block-hash-genesis.golden",
                    hash = "0551a1e5af999fe8fff529f6f2ab341e1e33db95135eef1b2be44fe6981349f3",
                    seq = -1,
                    errCode = 200,
                    errMsg = ""
                },
                new
                {
                    name = "genesis seq",
                    golden = "block-seq-0.golden",
                    hash = "",
                    seq = 0,
                    errCode = 200,
                    errMsg = ""
                },
                new
                {
                    name = "seq 1",
                    golden = "block-seq-1.golden",
                    hash = "",
                    seq = 1,
                    errCode = 200,
                    errMsg = ""
                },
                new
                {
                    name = "seq 100",
                    golden = "block-seq-100.golden",
                    hash = "",
                    seq = 100,
                    errCode = 200,
                    errMsg = ""
                },
                new
                {
                    name = "unknown seq",
                    golden = "",
                    hash = "",
                    seq = 999999999,
                    errCode = 404,
                    errMsg = "Error calling Block: 404 Not Found\n"
                }
            };
            foreach (var tc in testCases)
            {
                if (tc.errCode != 200)
                {
                    var err = Assert.Throws<ApiException>(() =>
                    {
                        if (tc.seq >= 0)
                        {
                            apiInstance.Block(seq: tc.seq);
                        }
                        else
                        {
                            apiInstance.Block(hash: tc.hash);
                        }
                    });
                    Assert.Equal(err.ErrorCode, tc.errCode);
                    Assert.Equal(err.Message, tc.errMsg);
                }
                else
                {
                    if (tc.seq >= 0)
                    {
                        var result = apiInstance.Block(seq: tc.seq);
                        CheckGoldenFile(tc.golden, result);
                    }
                    else
                    {
                        var result = apiInstance.Block(hash: tc.hash);
                        CheckGoldenFile(tc.golden, result);
                    }
                }
            }
        }

        private void BlockchainMetadataStable()
        {
            var apiInstance = new DefaultApi(basePath);
            var file = "blockchain-metadata.golden";
            var result = apiInstance.BlockchainMetadata();
            CheckGoldenFile(file, result);
        }

        private void AddressUxoutsStable()
        {
            var apiInstance = new DefaultApi(basePath);
            var testCases = new[]
            {
                new
                {
                    name = "no addresses",
                    errCode = 400,
                    errMsg = "Error calling AddressUxouts: 400 Bad Request - address is empty\n",
                    golden = "",
                    address = ""
                },
                new
                {
                    name = "unknown address",
                    errCode = 200,
                    errMsg = "",
                    golden = "uxout-noaddr.golden",
                    address = "prRXwTcDK24hs6AFxj69UuWae3LzhrsPW9"
                },
                new
                {
                    name = "one address",
                    errCode = 200,
                    errMsg = "",
                    golden = "uxout-addr.golden",
                    address = "2THDupTBEo7UqB6dsVizkYUvkKq82Qn4gjf"
                }
            };

            foreach (var tc in testCases)
            {
                if (tc.errCode != 200)
                {
                    var err = Assert.Throws<ApiException>(() => apiInstance.AddressUxouts(tc.address));
                    Assert.Equal(err.ErrorCode, tc.errCode);
                    Assert.Equal(err.Message, tc.errMsg);
                }
                else
                {
                    var result = apiInstance.AddressUxouts(tc.address);
                    CheckGoldenFile(tc.golden, JsonConvert.SerializeObject(result, Formatting.Indented));
                }
            }
        }

        private void BlockChainProgressStable()
        {
            var apiInstance = new DefaultApi(basePath);
            var result = apiInstance.BlockchainProgress();
            CheckGoldenFile("blockchain-progress.golden", result);
        }

        private void BlocksStable()
        {
            var apiInstance = new DefaultApi(basePath);
            Progress p = JsonConvert.DeserializeObject<Progress>(
                apiInstance.BlockchainProgress().ToString());

            var testCases = new[]
            {
                new
                {
                    name = "multiple sequences",
                    golden = "blocks-3-5-7.golden",
                    start = 0,
                    end = 0,
                    seqs = new List<int?> {3, 5, 7},
                    errCode = 200,
                    isRange = false,
                    errMsg = ""
                },
                new
                {
                    name = "block seq not found",
                    golden = "",
                    start = 0,
                    end = 0,
                    seqs = new List<int?> {3, 5, 7, 99999},
                    errCode = 404,
                    isRange = false,
                    errMsg = "Error calling Blocks: 404 Not Found - block does not exist seq=99999\n"
                },
                new
                {
                    name = "first 10",
                    golden = "blocks-first-10.golden",
                    start = 1,
                    end = 10,
                    seqs = new List<int?>(),
                    errCode = 200,
                    isRange = true,
                    errMsg = ""
                },
                new
                {
                    name = "last 10",
                    golden = "blocks-last-10.golden",
                    start = p.current - 10,
                    end = p.current,
                    seqs = new List<int?>(),
                    errCode = 200,
                    isRange = true,
                    errMsg = ""
                },
                new
                {
                    name = "first block",
                    golden = "blocks-first-1.golden",
                    start = 1,
                    end = 1,
                    seqs = new List<int?>(),
                    errCode = 200,
                    isRange = true,
                    errMsg = ""
                },
                new
                {
                    name = "all blocks",
                    golden = "blocks-all.golden",
                    start = 0,
                    end = p.current,
                    seqs = new List<int?>(),
                    errCode = 200,
                    isRange = true,
                    errMsg = ""
                },
                new
                {
                    name = "start > end",
                    golden = "blocks-end-less-than-start.golden",
                    start = 10,
                    end = 9,
                    seqs = new List<int?>(),
                    errCode = 200,
                    isRange = true,
                    errMsg = ""
                }
            };
            foreach (var tc in testCases)
            {
                if (tc.errCode != 200)
                {
                    var err = Assert.Throws<ApiException>(() => apiInstance.Blocks(seqs: tc.seqs));
                    Assert.Equal(err.ErrorCode, tc.errCode);
                    Assert.Equal(err.Message, tc.errMsg);
                }
                else
                {
                    if (tc.isRange)
                    {
                        var result = apiInstance.Blocks(start: tc.start, end: tc.end);
                        CheckGoldenFile(tc.golden, JsonConvert.SerializeObject(result, Formatting.Indented));
                    }
                    else
                    {
                        var result = apiInstance.Blocks(seqs: tc.seqs);
                        CheckGoldenFile(tc.golden, JsonConvert.SerializeObject(result, Formatting.Indented));
                    }
                }
            }
        }

        private void CoinSupplyStable()
        {
            var apiInstance = new DefaultApi(basePath);
            CheckGoldenFile("coinsupply.golden", apiInstance.CoinSupply().ToJson());
        }

        private void TransactionsGetStable()
        {
            var apiInstance = new DefaultApi(basePath);
            var testCases = new[]
            {
                //Simple
                new
                {
                    name = "invalid addr length",
                    addrs = new[] {"abcd"},
                    errorCode = 400,
                    errMsg = "Error calling TransactionsGet: 400 Bad Request - parse parameter: 'addrs'" +
                             " failed: address \"abcd\" is invalid: Invalid address length\n",
                    goldenFile = "",
                    confirmed = ""
                },
                new
                {
                    name = "invalid addr character",
                    addrs = new[] {"701d23fd513bad325938ba56869f9faba19384a8ec3dd41833aff147eac53947"},
                    errorCode = 400,
                    errMsg = "Error calling TransactionsGet: 400 Bad Request - parse parameter: 'addrs'" +
                             " failed: address \"701d23fd513bad325938ba56869f9faba19384a8ec3dd41833aff147eac53947\"" +
                             " is invalid: Invalid base58 character\n",
                    goldenFile = "",
                    confirmed = ""
                },
                new
                {
                    name = "invalid checksum",
                    addrs = new[] {"2kvLEyXwAYvHfJuFCkjnYNRTUfHPyWgVwKk"},
                    errorCode = 400,
                    errMsg = "Error calling TransactionsGet: 400 Bad Request - parse parameter: 'addrs'" +
                             " failed: address \"2kvLEyXwAYvHfJuFCkjnYNRTUfHPyWgVwKk\" is invalid: Invalid checksum\n",
                    goldenFile = "",
                    confirmed = ""
                },
                new
                {
                    name = "empty addrs",
                    addrs = new[] {""},
                    errorCode = 200,
                    errMsg = "",
                    goldenFile = "empty-addrs-transactions.golden",
                    confirmed = ""
                },
                new
                {
                    name = "single addr",
                    addrs = new[] {"2kvLEyXwAYvHfJuFCkjnYNRTUfHPyWgVwKt"},
                    errorCode = 200,
                    errMsg = "",
                    goldenFile = "single-addr-transactions.golden",
                    confirmed = ""
                },
                new
                {
                    name = "genesis",
                    addrs = new[] {"2jBbGxZRGoQG1mqhPBnXnLTxK6oxsTf8os6"},
                    errorCode = 200,
                    errMsg = "",
                    goldenFile = "genesis-addr-transactions.golden",
                    confirmed = ""
                },
                new
                {
                    name = "multiple addrs",
                    addrs = new[] {"2kvLEyXwAYvHfJuFCkjnYNRTUfHPyWgVwKt", "2JJ8pgq8EDAnrzf9xxBJapE2qkYLefW4uF8"},
                    errorCode = 200,
                    errMsg = "",
                    goldenFile = "multiple-addr-transactions.golden",
                    confirmed = ""
                },
                //Confirmed=true
                new
                {
                    name = "all confirmed",
                    addrs = new[] {""},
                    errorCode = 200,
                    errMsg = "",
                    goldenFile = "all-confirmed-transactions.golden",
                    confirmed = "true"
                },
                new
                {
                    name = "unconfirmed should be excluded",
                    addrs = new[] {"212mwY3Dmey6vwnWpiph99zzCmopXTqeVEN"},
                    errorCode = 200,
                    errMsg = "",
                    goldenFile = "unconfirmed-excluded-from-transactions.golden",
                    confirmed = "true"
                },
                //Confirmed=false
                new
                {
                    name = "all unconfirmed",
                    addrs = new[] {""},
                    errorCode = 200,
                    errMsg = "",
                    goldenFile = "all-unconfirmed-transactions.golden",
                    confirmed = "false"
                },
                new
                {
                    name = "confirmed should be excluded",
                    addrs = new[] {"212mwY3Dmey6vwnWpiph99zzCmopXTqeVEN"},
                    errorCode = 200,
                    errMsg = "",
                    goldenFile = "confirmed-excluded-from-transactions.golden",
                    confirmed = "false"
                },
            };
            foreach (var tc in testCases)
            {
                if (tc.errorCode != 200)
                {
                    var err = Assert.Throws<ApiException>(() => apiInstance.TransactionsGet(
                        addrs: string.Join(",", tc.addrs)));
                    Assert.Equal(err.ErrorCode, tc.errorCode);
                    Assert.Equal(err.Message, tc.errMsg);
                }
                else
                {
                    if (tc.confirmed.Length != 0)
                    {
                        var result = apiInstance.TransactionsGet(addrs: string.Join(",", tc.addrs),
                            confirmed: tc.confirmed);
                        CheckGoldenFile(tc.goldenFile, result);
                    }
                    else
                    {
                        var result = apiInstance.TransactionsGet(addrs: string.Join(",", tc.addrs));
                        CheckGoldenFile(tc.goldenFile, result);
                    }
                }
            }
        }

        private string GetCsrf()
        {
            string token;
            try
            {
                var api = new DefaultApi(basePath);
                token = api.Csrf().CsrfToken;
            }
            catch (Exception)
            {
                return "";
            }

            return token;
        }

        private void CheckGoldenFile(string file, object valRecive)
        {
            file = "../../../TestFile/" + file;
            var valSpected = LoadGoldenFile(file, valRecive);
            Assert.Equal(valRecive.ToString(), valSpected);
        }

        private object LoadGoldenFile(string file, object valRecive)
        {
            FileStream fs = new FileStream(file, FileMode.OpenOrCreate);
            if (fs.Length == 0)
            {
                fs.Write(Encoding.ASCII.GetBytes(valRecive.ToString()));
                fs.Flush();
                fs.Close();
                return null;
            }

            byte[] b = new byte[fs.Length];
            fs.Read(b);
            fs.Close();
            return Encoding.ASCII.GetString(b);
        }
    }
}