using System;
using Xunit;
using Skyapi.Api;
using Skyapi.Client;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Skyapi.Model;

namespace Sample.Unit.Tests
{
    public class UnitTest1
    {
        private string testMode = Environment.GetEnvironmentVariable("TESTMODE") ?? "stable";
        private string coin = Environment.GetEnvironmentVariable("COIN") ?? "skycoin";
        private bool useCsrf = Convert.ToBoolean(Environment.GetEnvironmentVariable("USE_CSRF") ?? "false");
        private string nodeAddress = Environment.GetEnvironmentVariable("SKYCOIN_NODE_HOST") ?? "http://localhost:6420";

        private bool liveDisableNetworking =
            Convert.ToBoolean(Environment.GetEnvironmentVariable("LIVE_DISABLE_NETWORKING") ?? "false");


        private struct Progress
        {
            public int Current { get; set; }
            public int Highest { get; set; }
            public string[] Peer { get; set; }
        }

        private struct Head
        {
            public int Seq { get; set; }
            public string Block_Hash { get; set; }
            public string Previous_Block_Hash { get; set; }
            public long Timestamp { get; set; }
            public long Fee { get; set; }
            public int Version { get; set; }
            public string Tx_Body_Hash { get; set; }
            public string Ux_Hash { get; set; }
        }

        private struct BlockchainMetadata
        {
            public Head Head { get; set; }
            public int Unspents { get; set; }
            public int Unconfirmed { get; set; }
            public string Time_Since_Last_Block { get; set; }
        }

        private struct Health
        {
            public BlockchainMetadata Blockchain { get; set; }
            public InlineResponse2005 Version { get; set; }
            public string Coin { get; set; }
            public string User_Agent { get; set; }
            public int Open_Connections { get; set; }
            public int Outgoing_Connections { get; set; }
            public int Incoming_Connections { get; set; }
            public string Uptime { get; set; }
            public bool CSRF_Enabled { get; set; }
            public bool Header_Check_Enabled { get; set; }
            public bool Csp_Enabled { get; set; }
            public bool Wallet_API_Enabled { get; set; }
            public bool GUI_Enabled { get; set; }
            public object User_Verify_Transaction { get; set; }
            public object Unconfirmed_Verify_Transaction { get; set; }
            public long Started_At { get; set; }
        }

        private struct Balance
        {
            public Dictionary<string, BalancePair> Addresses { get; set; }
            public Confirm Confirmed { get; set; }
            public Predict Predicted { get; set; }
        }

        private struct BalancePair
        {
            public Confirm Confirmed { get; set; }
            public Predict Predicted { get; set; }
        }

        private struct Confirm
        {
            public long coins { get; set; }
            public long hours { get; set; }
        }

        private struct Predict
        {
            public long coins { get; set; }
            public long hours { get; set; }
        }

        [Fact]
        public void TestVersion()
        {
            Configuration.Default.BasePath = nodeAddress;
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
            if (useCsrf)
            {
                apiInstance.Configuration.AddApiKeyPrefix("X-CSRF-TOKEN", GetCsrf());
            }

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
        public void TestBalance()
        {
            if (testMode.Equals("stable"))
            {
                BalanceStable();
            }
            else
            {
                BalanceLive();
            }
        }

        [Fact]
        public void TestAddressCount()
        {
            if (testMode.Equals("stable"))
            {
                AddressCountStable();
            }
            else
            {
                AddressCountLive();
            }
        }

        [Fact]
        public void TestBlock()
        {
            if (testMode.Equals("stable"))
            {
                BlockStable();
            }
            else
            {
                BlockLive();
            }
        }

        [Fact]
        public void TestBlockchainMetadata()
        {
            if (testMode.Equals("stable"))
            {
                BlockchainMetadataStable();
            }
            else
            {
                BlockchainMetadataLive();
            }
        }

        [Fact]
        public void TestAddressUxouts()
        {
            if (testMode.Equals("stable"))
            {
                AddressUxoutsStable();
            }
            else
            {
                if (liveDisableNetworking)
                {
                    AddressUxoutsLive();
                }
                else
                {
                    Console.WriteLine("Skipping slow ux out tests when networking disabled");
                }
            }
        }

        [Fact]
        public void TestBlockchainProgress()
        {
            if (testMode.Equals("stable"))
            {
                BlockChainProgressStable();
            }
            else
            {
                BlockChainProgressLive();
            }
        }

        [Fact]
        public void TestBlocks()
        {
            if (testMode.Equals("stable"))
            {
                BlocksStable();
            }
            else
            {
                BlocksLive();
            }
        }

        [Fact]
        public void TestCoinSupply()
        {
            if (testMode.Equals("stable"))
            {
                CoinSupplyStable();
            }
            else
            {
                CoinSupplyLive();
            }
        }

        [Fact]
        public void TestTransactions()
        {
            if (testMode.Equals("stable"))
            {
                TransactionsStable();
            }
            else
            {
                TransactionsLive();
            }
        }

        [Fact]
        public void TestHealth()
        {
            if (testMode.Equals("stable"))
            {
                HealthStable();
            }
            else
            {
                HealthLive();
            }
        }

        [Fact]
        public void TestLastBlocks()
        {
            if (testMode.Equals("stable"))
            {
                LastBlocksStable();
            }
            else
            {
                LastBlockLive();
            }
        }

        [Fact]
        private void TestNetworkConnection()
        {
            if (testMode.Equals("stable"))
            {
                NetworkConnectionStable();
            }
            else
            {
                NetworkConnectionLive();
            }
        }

        [Fact]
        private void TestNetworkConnectionExchange()
        {
            if (testMode.Equals("stable"))
            {
                NetworkConnectionExchangeStable();
            }
            else
            {
                NetworkConnectionExchangeLive();
            }
        }

        [Fact]
        private void TestNetworkConnectionTrust()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var connections = apiInstance.NetworkConnectionsTrust();
            Assert.NotEmpty(connections);
            connections.Sort();
            CheckGoldenFile("network-trusted-peers.golden",
                JsonConvert.SerializeObject(connections, Formatting.Indented));
        }

        [Fact]
        private void TestNetworkDefaultConnection()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var connections = apiInstance.DefaultConnections();
            Assert.NotEmpty(connections);
            connections.Sort();
            CheckGoldenFile("network-default-peers.golden",
                JsonConvert.SerializeObject(connections, Formatting.Indented));
        }
        
        private void BalanceStable()
        {
            var apiInstance = new DefaultApi(nodeAddress);
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
                var resultg = apiInstance.BalanceGet(string.Join(",", tc.addrs));
                CheckGoldenFile(tc.file, resultg);
                if (useCsrf)
                {
                    apiInstance.Configuration.AddApiKeyPrefix("X-CSRF-TOKEN", GetCsrf());
                }

                var resultp = apiInstance.BalancePost(string.Join(",", tc.addrs));
                Assert.Equal(resultg, resultp);
            }
        }

        private void BalanceLive()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            // Genesis address check, should not have a balance
            var result =
                JsonConvert.DeserializeObject<Balance>(apiInstance.BalanceGet("2jBbGxZRGoQG1mqhPBnXnLTxK6oxsTf8os6")
                    .ToString());
            Assert.Equal(result, new Balance
            {
                Addresses = new Dictionary<string, BalancePair>
                {
                    ["2jBbGxZRGoQG1mqhPBnXnLTxK6oxsTf8os6"] = new BalancePair()
                }
            });
            // Balance of final distribution address. Should have the same coins balance
            // for the next 15-20 years.
            result = JsonConvert.DeserializeObject<Balance>(apiInstance
                .BalanceGet("ejJjiCwp86ykmFr5iTJ8LxQXJ2wJPTYmkm")
                .ToString());
            Assert.Equal(result.Confirmed.coins, result.Predicted.coins);
            Assert.Equal(result.Confirmed.hours, result.Predicted.hours);
            Assert.NotEqual(0, result.Confirmed.hours);

            // Add 1e4 because someone sent 0.01 coins to it
            decimal expectedBalance = decimal.Parse("1E6", NumberStyles.Any) * decimal.Parse("1E6", NumberStyles.Any) +
                                      decimal.Parse("1E4", NumberStyles.Any);
            Assert.Equal(expectedBalance, result.Confirmed.coins);
            // Check that the balance is queryable for addresses known to be affected
            // by the coinhour overflow problem
            var address = new string[]
            {
                "n7AR1VMW1pK7F9TxhYdnr3HoXEQ3g9iTNP",
                "2aTzmXi9jyiq45oTRFCP9Y7dcvnT6Rsp7u",
                "FjFLnus2ePxuaPTXFXfpw6cVAE5owT1t3P",
                "KT9vosieyWhn9yWdY8w7UZ6tk31KH4NAQK"
            };
            Assert.All(address, s => apiInstance.BalanceGet(s));
            apiInstance.BalanceGet(string.Join(",", address));
        }

        private void AddressCountStable()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var result = apiInstance.AddressCount();
            Assert.Equal(155, result.Count);
        }

        private void AddressCountLive()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var result = apiInstance.AddressCount();
            // 5296 addresses as of 2018-03-06, the count could decrease but is unlikely to
            Assert.True(result.Count > 5000);
        }

        private void BlockStable()
        {
            var apiInstance = new DefaultApi(nodeAddress);
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

        private void BlockLive()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            BlockStable();
            var knownBadBlockSeqs = new int[]
            {
                // coinhour fee calculation mistake, related to distribution addresses:
                297,
                741,
                743,
                749,
                796,
                4956,
                10125,
                // coinhour overflow related:
                11685,
                11707,
                11710,
                11709,
                11705,
                11708,
                11711,
                11706,
                11699,
                13277
            };

            foreach (var seq in knownBadBlockSeqs)
            {
                var b = apiInstance.Block(seq: seq);
                Assert.Equal(seq, b.Header.Seq);
            }
        }

        private void BlockchainMetadataStable()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var result = apiInstance.BlockchainMetadata();
            CheckGoldenFile("blockchain-metadata.golden", result);
        }

        private void BlockchainMetadataLive()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var result = JsonConvert.DeserializeObject<BlockchainMetadata>(apiInstance.BlockchainMetadata().ToString());
            Assert.NotEqual(0, result.Head.Seq);
        }

        private void AddressUxoutsStable()
        {
            var apiInstance = new DefaultApi(nodeAddress);
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
                if (tc.errCode != 200 && tc.errCode != 0)
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

        private void AddressUxoutsLive()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var testCases = new[]
            {
                new
                {
                    name = "no addresses",
                    errCode = 400,
                    errMsg = "Error calling AddressUxouts: 400 Bad Request - address is empty\n",
                    address = ""
                },
                new
                {
                    name = "invalid address length",
                    errCode = 400,
                    errMsg = "Error calling AddressUxouts: 400 Bad Request - Invalid address length\n",
                    address = "prRXwTcDK24hs6AFxj"
                },
                new
                {
                    name = "unknown address",
                    errCode = 200,
                    errMsg = "",
                    address = "prRXwTcDK24hs6AFxj69UuWae3LzhrsPW9"
                },
                new
                {
                    name = "one address",
                    errCode = 200,
                    errMsg = "",
                    address = "2THDupTBEo7UqB6dsVizkYUvkKq82Qn4gjf"
                }
            };
            foreach (var tc in testCases)
            {
                if (tc.errCode != 0 && tc.errCode != 200)
                {
                    var err = Assert.Throws<ApiException>(() => apiInstance.AddressUxouts(tc.address));
                    Assert.Equal(err.ErrorCode, tc.errCode);
                    Assert.Equal(err.Message, tc.errMsg);
                }
                else
                {
                    var result = apiInstance.AddressUxouts(tc.address);
                    Assert.Empty(result);
                }
            }
        }

        private void BlockChainProgressStable()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var result = apiInstance.BlockchainProgress();
            CheckGoldenFile("blockchain-progress.golden", result);
        }

        private void BlockChainProgressLive()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var result = JsonConvert.DeserializeObject<Progress>(apiInstance.BlockchainProgress().ToString());
            Assert.NotEqual(0, result.Current);

            if (liveDisableNetworking)
            {
                Assert.Empty(result.Peer);
                Assert.Equal(result.Current, result.Highest);
            }
            else
            {
                Assert.NotEmpty(result.Peer);
                Assert.True(result.Highest >= result.Current);
            }
        }

        private void BlocksStable()
        {
            var apiInstance = new DefaultApi(nodeAddress);
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
                    start = p.Current - 10,
                    end = p.Current,
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
                    end = p.Current,
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
                        BlockInRangeTest(start: tc.start, end: tc.end);
                        var result = apiInstance.Blocks(start: tc.start, end: tc.end);
                        CheckGoldenFile(tc.golden, JsonConvert.SerializeObject(result, Formatting.Indented));
                    }
                    else
                    {
                        BlocksTest(tc.seqs);
                        var result = apiInstance.Blocks(seqs: tc.seqs);
                        CheckGoldenFile(tc.golden, JsonConvert.SerializeObject(result, Formatting.Indented));
                    }
                }
            }
        }

        private void BlocksLive()
        {
            BlocksTest(new List<int?> {3, 5, 7});
            BlockInRangeTest(1, 10);
        }

        private void BlockInRangeTest(int? start = null, int? end = null)
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var result = apiInstance.Blocks(start: start, end: end);
            if (start > end)
            {
                Assert.Empty(result.Blocks);
            }
            else
            {
                Assert.Equal(end - start + 1, result.Blocks.Count);
            }

            BlockSchema prevblock = null;
            result.Blocks.ForEach(b =>
            {
                if (prevblock != null)
                {
                    Assert.Equal(prevblock.Header.BlockHash, b.Header.PreviousBlockHash);
                }

                var bh = apiInstance.Block(hash: b.Header.BlockHash);
                Assert.Equal(result.Blocks.FindIndex(block => Equals(block, b)) + start, b.Header.Seq);
                Assert.NotNull(bh);
                Assert.Equal(bh.ToJson(), b.ToJson());
                prevblock = b;
            });
        }


        private void BlocksTest(List<int?> seqs)
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var result = apiInstance.Blocks(seqs: seqs);
            Assert.Equal(seqs.Count, result.Blocks.Count);
            var seqsMap = new Dictionary<int?, BlockVerboseSchemaHeader>();
            seqs.ForEach(s => { seqsMap[s] = null; });
            result.Blocks.ForEach(b =>
            {
                Assert.True(seqsMap.ContainsKey(b.Header.Seq));
                seqsMap.Remove(b.Header.Seq);
                var bh = apiInstance.Block(b.Header.BlockHash);
                Assert.NotNull(bh);
                Assert.Equal(b.ToJson(), bh.ToJson());
            });
            Assert.Empty(seqsMap);
        }

        private void CoinSupplyStable()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            CheckGoldenFile("coinsupply.golden", apiInstance.CoinSupply().ToJson());
        }

        private void CoinSupplyLive()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var cs = apiInstance.CoinSupply();
            Assert.NotEmpty(cs.CurrentSupply);
            Assert.NotEmpty(cs.TotalSupply);
            Assert.NotEmpty(cs.MaxSupply);
            Assert.Equal("100000000.000000", cs.MaxSupply);
            Assert.NotEmpty(cs.CurrentCoinhourSupply);
            Assert.NotEmpty(cs.TotalCoinhourSupply);
            Assert.Equal(100, cs.UnlockedDistributionAddresses.Count + cs.LockedDistributionAddresses.Count);
        }

        private void TransactionsStable()
        {
            var apiInstance = new DefaultApi(nodeAddress);
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
                }
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
                    if (!tc.confirmed.Equals(""))
                    {
                        var result = apiInstance.TransactionsGet(addrs: string.Join(",", tc.addrs),
                            confirmed: tc.confirmed);
                        CheckGoldenFile(tc.goldenFile, result);
                        if (useCsrf)
                        {
                            apiInstance.Configuration.AddApiKeyPrefix("X-CSRF-TOKEN", GetCsrf());
                        }

                        //EndPoint /api/v1/transactions with method post always return all address.
                        var resultp = apiInstance.TransactionsPost(string.Join(",", tc.addrs));
                        CheckGoldenFile("empty-addrs-transactions.golden", resultp);
                    }
                    else
                    {
                        var result = apiInstance.TransactionsGet(addrs: string.Join(",", tc.addrs));
                        CheckGoldenFile(tc.goldenFile, result);
                        if (useCsrf)
                        {
                            apiInstance.Configuration.AddApiKeyPrefix("X-CSRF-TOKEN", GetCsrf());
                        }

                        //EndPoint /api/v1/transactions with method post always return all address.
                        var resultp = apiInstance.TransactionsPost(string.Join(",", tc.addrs));
                        CheckGoldenFile("empty-addrs-transactions.golden", resultp);
                    }
                }
            }
        }

        private void TransactionsLive()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var simpleaddrs = new[]
            {
                "2kvLEyXwAYvHfJuFCkjnYNRTUfHPyWgVwKt"
            };
            var sresult =
                JsonConvert.DeserializeObject<List<Transaction>>(apiInstance
                    .TransactionsGet(string.Join(",", simpleaddrs))
                    .ToString());
            Assert.True(sresult.Count >= 0);
            AssertNoTransactionsDupes(sresult);
            var multiaddrs = new[]
            {
                "7cpQ7t3PZZXvjTst8G7Uvs7XH4LeM8fBPD",
                "2K6NuLBBapWndAssUtkxKfCtyjDQDHrEhhT"
            };
            var mresult =
                JsonConvert.DeserializeObject<List<Transaction>>(apiInstance
                    .TransactionsGet(string.Join(",", multiaddrs))
                    .ToString());
            Assert.True(mresult.Count >= 4);
            AssertNoTransactionsDupes(mresult);
            //Unconfirmedtransactions
            sresult =
                JsonConvert.DeserializeObject<List<Transaction>>(apiInstance
                    .TransactionsGet(string.Join(",", simpleaddrs), confirmed: "false")
                    .ToString());
            Assert.True(sresult.Count >= 0);
            AssertNoTransactionsDupes(sresult);
            mresult =
                JsonConvert.DeserializeObject<List<Transaction>>(apiInstance
                    .TransactionsGet(string.Join(",", multiaddrs), confirmed: "false")
                    .ToString());
            Assert.True(mresult.Count >= 0);
            Assert.True(mresult.Count >= sresult.Count);
            AssertNoTransactionsDupes(mresult);
            //ConfirmedTransactions
            sresult =
                JsonConvert.DeserializeObject<List<Transaction>>(apiInstance
                    .TransactionsGet(string.Join(",", simpleaddrs), confirmed: "true")
                    .ToString());
            Assert.True(sresult.Count >= 0);
            AssertNoTransactionsDupes(sresult);
            mresult =
                JsonConvert.DeserializeObject<List<Transaction>>(apiInstance
                    .TransactionsGet(string.Join(",", multiaddrs), confirmed: "true")
                    .ToString());
            Assert.True(mresult.Count >= 0);
            Assert.True(mresult.Count >= sresult.Count);
            AssertNoTransactionsDupes(mresult);
        }

        private static void AssertNoTransactionsDupes(IEnumerable<Transaction> list)
        {
            var txids = new Dictionary<string, object>();
            foreach (var transaction in list)
            {
                Assert.False(txids.ContainsKey(transaction.Txn.InnerHash));
                txids[transaction.Txn.InnerHash] = new object();
            }
        }

        private void HealthStable()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var result = JsonConvert.DeserializeObject<Health>(apiInstance.Health().ToString());
            CheckHealthResponse(result);
            Assert.Equal(0, result.Open_Connections);
            Assert.Equal(0, result.Incoming_Connections);
            Assert.Equal(0, result.Outgoing_Connections);
            CompareTime(result.Blockchain.Time_Since_Last_Block);
            Assert.NotNull(result.Version.Commit);
            Assert.NotNull(result.Version.Branch);
            Assert.Equal("skycoin", result.Coin);
            Assert.Equal(string.Format("{0}:{1}", result.Coin, result.Version.Version), result.User_Agent);
            Assert.Equal(useCsrf, result.CSRF_Enabled);
            Assert.True(result.Csp_Enabled);
            Assert.True(result.Wallet_API_Enabled);
            Assert.False(result.GUI_Enabled);
        }

        private void HealthLive()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var result = JsonConvert.DeserializeObject<Health>(apiInstance.Health().ToString());
            CheckHealthResponse(result);
            if (liveDisableNetworking)
            {
                Assert.Equal(0, result.Open_Connections);
                Assert.Equal(0, result.Outgoing_Connections);
                Assert.Equal(0, result.Incoming_Connections);
            }
            else
            {
                Assert.NotEqual(0, result.Open_Connections);
            }

            Assert.Equal(result.Outgoing_Connections + result.Incoming_Connections, result.Open_Connections);
        }

        private static void CheckHealthResponse(Health h)
        {
            Assert.NotEqual(0, h.Blockchain.Unspents);
            Assert.NotEqual(0, h.Blockchain.Head.Seq);
            Assert.NotEqual(0, h.Blockchain.Head.Timestamp);
            Assert.NotNull(h.Version.Version);
            CompareTime(h.Uptime);
            Assert.NotNull(h.Coin);
            Assert.NotNull(h.User_Agent);
        }

        private void LastBlocksStable()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var result1 = JsonConvert.DeserializeObject<InlineResponse2001>(apiInstance.LastBlocks(1).ToString());
            CheckGoldenFile("block-last.golden", result1.ToJson());
            var result2 = JsonConvert.DeserializeObject<InlineResponse2001>(apiInstance.LastBlocks(10).ToString());
            Assert.Equal(10, result2.Blocks.Count);
            BlockSchema prevBlock = null;
            result2.Blocks.ForEach(block =>
            {
                if (prevBlock != null)
                {
                    Assert.NotEqual(prevBlock.Header.BlockHash, block.Header.BlockHash);
                }

                var bh = apiInstance.Block(hash: block.Header.BlockHash);
                Assert.NotNull(bh);
                Assert.Equal(block.ToJson(), bh.ToJson());
                prevBlock = block;
            });
        }

        private void LastBlockLive()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            BlockSchema prevBlock = null;
            var results = JsonConvert.DeserializeObject<InlineResponse2001>(apiInstance.LastBlocks(10).ToString());
            Assert.Equal(10, results.Blocks.Count);
            results.Blocks.ForEach(b =>
            {
                if (prevBlock != null)
                {
                    Assert.Equal(prevBlock.Header.BlockHash, b.Header.PreviousBlockHash);
                }

                var bh = apiInstance.Block(hash: b.Header.BlockHash);
                Assert.NotNull(bh);
                Assert.Equal(b.ToJson(), bh.ToJson());
                prevBlock = b;
            });
        }

        private void NetworkConnectionStable()
        {
            NetworkConnectionSchema connectionSchema = null;
            var apiInstance = new DefaultApi(nodeAddress);
            var connections = apiInstance.NetworkConnections();
            Assert.Empty(connections.Connections);
            var err404 =
                Assert.Throws<ApiException>(() => connectionSchema = apiInstance.NetworkConnection("127.0.0.1:4444"));
            Assert.Equal(404, err404.ErrorCode);
            Assert.Equal("Error calling NetworkConnection: 404 Not Found\n", err404.Message);
            Assert.Null(connectionSchema);
        }

        private void NetworkConnectionLive()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var connections = apiInstance.NetworkConnections();
            if (liveDisableNetworking)
            {
                Assert.Empty(connections.Connections);
                return;
            }

            Assert.NotEmpty(connections.Connections);
            var check = false;
            connections.Connections.ForEach(cc =>
            {
                NetworkConnectionSchema connection = null;
                try
                {
                    connection = apiInstance.NetworkConnection(cc.Address);
                }
                catch (ApiException err)
                {
                    if (err.ErrorCode == 404 || err.Message == "Error calling NetworkConnection: 404 Not Found\n")
                    {
                        return;
                    }
                }

                Assert.NotNull(cc.Address);
                Assert.Equal(cc.Address, connection?.Address);
                Assert.Equal(cc.Id, connection?.Id);
                Assert.Equal(cc.ListenPort, connection?.ListenPort);
                Assert.Equal(cc.Mirror, connection?.Mirror);
                switch (cc.State)
                {
                    case NetworkConnectionSchema.StateEnum.Introduced:
                        Assert.Equal(NetworkConnectionSchema.StateEnum.Introduced, connection?.State);
                        break;
                    case NetworkConnectionSchema.StateEnum.Connected:
                        Assert.NotEqual(NetworkConnectionSchema.StateEnum.Pending, connection?.State);
                        break;
                }

                if (cc.State == NetworkConnectionSchema.StateEnum.Pending)
                {
                    Assert.Equal(0, cc.Id);
                }
                else
                {
                    Assert.NotEqual(0, cc.Id);
                }

                Assert.Equal(cc.Outgoing, connection?.Outgoing);
                Assert.True(cc.LastReceived <= connection?.LastReceived);
                Assert.True(cc.LastSent <= connection.LastSent);
                Assert.Equal(cc.ConnectedAt, connection.ConnectedAt);
                check = true;
            });
            Assert.True(check,
                "Was not able to find any connection by address, despite finding connections when querying all");
            connections = apiInstance.NetworkConnections(states: "pending");
            connections.Connections.ForEach(
                cc => { Assert.Equal(NetworkConnectionSchema.StateEnum.Pending, cc.State); });
            connections = apiInstance.NetworkConnections(direction: "incoming");
            connections.Connections.ForEach(cc => { Assert.False(cc.Outgoing); });
        }

        private void NetworkConnectionExchangeStable()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            var conenctions = apiInstance.NetworkConnectionsExchange();
            CheckGoldenFile("network-exchanged-peers.golden",
                JsonConvert.SerializeObject(conenctions, Formatting.Indented));
        }

        private void NetworkConnectionExchangeLive()
        {
            var apiInstance = new DefaultApi(nodeAddress);
            apiInstance.NetworkConnectionsExchange();
        }

        private static void CompareTime(string time)
        {
            var x = Regex.Split(time, @"h|s|m").Reverse().ToArray();
            int s = (int) float.Parse(x.Length >= 2 ? (x[1] != "" ? x[1] : "0") : "0"),
                m = (int) float.Parse(x.Length >= 3 ? (x[2] != "" ? x[2] : "0") : "0"),
                h = (int) float.Parse(x.Length >= 4 ? (x[3] != "" ? x[3] : "0") : "0");
            Assert.True(new TimeSpan(h, m, s) > TimeSpan.Zero);
        }

        private string GetCsrf()
        {
            string token;
            try
            {
                var api = new DefaultApi(nodeAddress);
                token = api.Csrf().CsrfToken;
            }
            catch (Exception)
            {
                return "";
            }

            return token;
        }

        private static void CheckGoldenFile(string file, object valRecive)
        {
            file = "../../../TestFile/" + file;
            var valSpected = LoadGoldenFile(file, valRecive) ?? LoadGoldenFile(file, valRecive);
            Assert.Equal(valRecive.ToString(), valSpected);
        }

        private static object LoadGoldenFile(string file, object valRecive)
        {
            var fs = new FileStream(file, FileMode.OpenOrCreate);
            if (fs.Length == 0)
            {
                fs.Write(Encoding.ASCII.GetBytes(valRecive.ToString()));
                fs.Flush();
                fs.Close();
                return null;
            }

            var b = new byte[fs.Length];
            fs.Read(b);
            fs.Close();
            return Encoding.ASCII.GetString(b);
        }
    }
}