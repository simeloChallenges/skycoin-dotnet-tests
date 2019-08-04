using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using RestSharp;
using Xunit;
using Skyapi.Api;
using Skyapi.Model;
using Skyapi.Client;

namespace Sample.Unit.Tests
{
    public class UnitTest1
    {
        // private string errEndpointIsDisabled = "Endpoint is disabled";
        private string basePath = "http://localhost:6420";

        [Fact]
        public void Version()
        {
            Configuration.Default.BasePath = basePath;
            var apiInstance = new DefaultApi(Configuration.Default);
            var result = apiInstance.Version();
            Assert.Equal("v0.26.0", result.Branch);
            Assert.Equal("ff754084df0912bc0d151529e2893ca86618fb3f", result.Commit);
            Assert.Equal("0.26.0", result.Version);
            Console.WriteLine(result);
        }

        [Fact]
        public void WalletSeedVerify()
        {
//            "error": {
//                "message": "EOF",
//                "code": 400
//            }
            Configuration.Default.BasePath = basePath;
            var apiInstance = new DefaultApi(Configuration.Default);
            apiInstance.Configuration.AddApiKeyPrefix("X-CSRF-TOKEN", GetCsrf());
            apiInstance.Configuration.AddDefaultHeader("Content-Type", "application/json");
            string seed = "nut wife logic sample addict shop before tobacco crisp bleak lawsuit affair";
            var result = apiInstance.WalletSeedVerify(seed);
            Console.WriteLine(result);
        }

        [Fact]
        public void BalanceGet()
        {
            var apiInstance = new DefaultApi(basePath);
            //0 address
            try
            {
                apiInstance.BalanceGet("");
            }
            catch (ApiException e)
            {
                Console.WriteLine(e);
            }

            //1 address
            var result =
                apiInstance.BalanceGet("2THDupTBEo7UqB6dsVizkYUvkKq82Qn4gjf");
            Assert.IsType<JObject>(result);
            //2 address or more.
            var result1 =
                apiInstance.BalanceGet("2THDupTBEo7UqB6dsVizkYUvkKq82Qn4gjf,qxmeHkwgAMfwXyaQrwv9jq3qt228xMuoT5");
            Assert.IsType<JObject>(result1);
            Console.WriteLine(result1);
        }

        [Fact]
        public void AddressCount()
        {
            var apiInstance = new DefaultApi(basePath);
            var result = apiInstance.AddressCount();
            Assert.IsType<Int64>(result.Count);
            Console.WriteLine(apiInstance.AddressCount());
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

            var apiInstance = new DefaultApi(basePath);
            var result=apiInstance.Block("", 2760);
            
        }

        [Fact]
        public void BlockchainMetadata()
        {
            var apiInstance = new DefaultApi(basePath);
            var results = apiInstance.BlockchainMetadata();
            Console.WriteLine(results);
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
    }
}