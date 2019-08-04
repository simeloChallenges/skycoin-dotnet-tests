using System;
using Newtonsoft.Json.Linq;
using Xunit;
using Skyapi.Api;
using Skyapi.Client;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sample.Unit.Tests
{
    public class UnitTest1
    {
        private string basePath = "http://localhost:6420";

        [Fact]
        public void Version()
        {
            Configuration.Default.BasePath = basePath;
            var apiInstance = new DefaultApi(Configuration.Default);
            try
            {
                var result = apiInstance.Version();
                Assert.Equal("v0.26.0", result.Branch);
                Assert.Equal("ff754084df0912bc0d151529e2893ca86618fb3f", result.Commit);
                Assert.Equal("0.26.0", result.Version);
                SaveTestInFile("Version", result.ToString());
            }
            catch (Exception e)
            {
                SaveTestInFile("Version", e.ToString());
                throw;
            }
        }

        [Fact]
        public void WalletSeedVerify()
        {
//            "error": {
//                "message": "EOF",
//                "code": 400
//            }
            var apiInstance = new DefaultApi(Configuration.Default);
            apiInstance.Configuration.AddApiKeyPrefix("X-CSRF-TOKEN", GetCsrf());
            apiInstance.Configuration.AddDefaultHeader("Content-Type", "application/json");
            string seed = "nut wife logic sample addict shop before tobacco crisp bleak lawsuit affair";
            try
            {
                var result = apiInstance.WalletSeedVerify(seed);
                SaveTestInFile("WalletSeedVerify", result.ToString());
            }
            catch (Exception e)
            {
                SaveTestInFile("WalletSeedVerify", e.ToString());

                throw;
            }
        }

        [Fact]
        public void BalanceGet()
        {
            var apiInstance = new DefaultApi(basePath);
            //0 address
            try
            {
                try
                {
                    apiInstance.BalanceGet("");
                }
                catch (ApiException e)
                {
                    SaveTestInFile("BalanceGet", string.Format("Wrong Case:{0}", e));
                }

                //1 address
                var result =
                    apiInstance.BalanceGet("2THDupTBEo7UqB6dsVizkYUvkKq82Qn4gjf");
                Assert.IsType<JObject>(result);
                SaveTestInFile("BalanceGet", string.Format("Right Case for 1 address:{0}", result));
                //2 address or more.
                var result1 =
                    apiInstance.BalanceGet("2THDupTBEo7UqB6dsVizkYUvkKq82Qn4gjf,qxmeHkwgAMfwXyaQrwv9jq3qt228xMuoT5");
                Assert.IsType<JObject>(result1);
                SaveTestInFile("BalanceGet", string.Format("Right Case for 2 address:{0}", result));
            }
            catch (Exception e)
            {
                SaveTestInFile("BalanceGet", e.ToString());
                throw;
            }
        }

        [Fact]
        public void AddressCount()
        {
            var apiInstance = new DefaultApi(basePath);
            try
            {
                var result = apiInstance.AddressCount();
                Assert.IsType<Int64>(result.Count);
                SaveTestInFile("AddessCount", result.ToString());
            }
            catch (Exception e)
            {
                SaveTestInFile("AddessCount", e.ToString());
                throw;
            }
        }

        [Fact]
        public void Block()
        {
            //!!!Prueba al metodo BlockWithHttpInfo de DefaultApi obj:
            //@result:FAIL.ERROR: Skyapi.Client.ApiException : Cannot deserialize the current JSON object
            //(e.g. {"name":"value"}) into type 'System.Collections.Generic.List`1[Skyapi.Model.BlockSchema]'
            //because the type requires a JSON array (e.g. [1,2,3]) to deserialize correctly.
            //To fix this error either change the JSON to a JSON array (e.g. [1,2,3]) or change the deserialized
            //type so that it is a normal .NET type (e.g. not a primitive type like integer, not a collection type
            //like an array or List<T>) that can be deserialized from a JSON object. JsonObjectAttribute can
            //also be added to the type to force it to deserialize from a JSON object.
            //  Path 'header', line 2, position 13.
            try
            {
                var apiInstance = new DefaultApi(basePath);
                var result = apiInstance.Block("", 0);
                SaveTestInFile("Block", result.ToString());
            }
            catch (Exception e)
            {
                SaveTestInFile("Block", e.ToString());
                throw;
            }
        }

        [Fact]
        public void BlockchainMetadata()
        {
            var apiInstance = new DefaultApi(basePath);
            try
            {
                var results = apiInstance.BlockchainMetadata();
                SaveTestInFile("BlockChainMetadata", results.ToString());
            }
            catch (Exception e)
            {
                SaveTestInFile("BlockChainMetadata", e.ToString());
                throw;
            }
        }

        [Fact]
        public void AddressUxouts()
        {
            var apiInstance = new DefaultApi(basePath);
            try
            {
                var result = apiInstance.AddressUxouts("6dkVxyKFbFKg9Vdg6HPg1UANLByYRqkrdY");
                SaveTestInFile("AddressUxouts", JsonConvert.SerializeObject(result));
            }

            catch (Exception e)
            {
                SaveTestInFile("AddressUxouts", e.ToString());
                throw;
            }
        }

        [Fact]
        public void BlockchainProgress()
        {
            var apiInstance = new DefaultApi(basePath);
            try
            {
                var result = apiInstance.BlockchainProgress();
                SaveTestInFile("BlockChainProgress", result.ToString());
            }

            catch (Exception e)
            {
                SaveTestInFile("BlockChainProgress", e.ToString());
                throw;
            }
        }

        [Fact]
        public void Blocks()
        {
            var apiInstance = new DefaultApi(basePath);
            try
            {
                //start=0
                //end=3
                var result = apiInstance.Blocks(start: 0, end: 3);
                SaveTestInFile("Blocks",
                    "Case start=0 end=3: \n" + JsonConvert.SerializeObject(result, Formatting.Indented));

                //end=10
                result = apiInstance.Blocks(end: 10);
                SaveTestInFile("Blocks", "Case end=10: \n" + JsonConvert.SerializeObject(result, Formatting.Indented));

                //list:0 , 2 , 5 , 13
                result = apiInstance.Blocks(seqs: new List<int?> {0, 2, 5, 13});

                SaveTestInFile("Blocks",
                    "Case list:0 , 2 , 5 , 13: \n" + JsonConvert.SerializeObject(result, Formatting.Indented));
            }

            catch (Exception e)
            {
                SaveTestInFile("Blocks", e.ToString());
                throw;
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

        private void SaveTestInFile(string namefile, string val)
        {
            FileStream fs;
            if (File.Exists("../../../TestFile/" + namefile + ".txt"))
            {
                fs = new FileStream("../../../TestFile/" + namefile + ".txt", FileMode.Append);
                fs.Write(Encoding.ASCII.GetBytes(string.Format("{0}:\n {1} \n", DateTime.Now, val)));
                fs.Flush();
            }
            else

            {
                fs = new FileStream("../../../TestFile/" + namefile + ".txt", FileMode.Create);
                fs.Write(Encoding.ASCII.GetBytes(string.Format("{0}:\n {1} \n", DateTime.Now, val)));
                fs.Flush();
                fs.Close();
            }
        }
    }
}