using System;
using Xunit;
using Skyapi.Api;
using Skyapi.Model;
using Skyapi.Client;

namespace Sample.Unit.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Version()
        {
	    Configuration.Default.BasePath = "http://localhost:6420";
	    var apiInstance = new DefaultApi(Configuration.Default);

	    try
            {
                // Returns the total number of unique address that have coins.
                var result = apiInstance.Version();
		Assert.Equal("v0.26.0", result.Branch);
		Assert.Equal("ff754084df0912bc0d151529e2893ca86618fb3f", result.Commit);
		Assert.Equal("0.26.0", result.Version);
                Console.WriteLine(result);
            }
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling DefaultApi.AddressCount: " + e.Message );
                Console.WriteLine("Status Code: "+ e.ErrorCode);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
