
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.TestingHost;
using TestExtensions;
using UnitTests.StreamingTests;
using Xunit;
using Xunit.Abstractions;

namespace Tester.StreamingTests
{
    public class SMSClientStreamTests : TestClusterPerTest
    {
        private const string SMSStreamProviderName = StreamTestsConstants.SMS_STREAM_PROVIDER_NAME;
        private const string StreamNamespace = "SMSDeactivationTestsNamespace";
        private readonly ITestOutputHelper output;
        private ClientStreamTestRunner runner;

        public SMSClientStreamTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            runner = new ClientStreamTestRunner(this.HostedCluster);
        }

        protected override void ConfigureTestCluster(TestClusterBuilder builder)
        {
            builder.Options.InitialSilosCount = 1;
            builder.AddClientBuilderConfigurator<ClientConfiguretor>();
            builder.AddSiloBuilderConfigurator<SiloConfigurator>();
        }

        public class SiloConfigurator : ISiloConfigurator
        {
            public void Configure(ISiloBuilder hostBuilder)
            {
                hostBuilder.AddSimpleMessageStreamProvider(SMSStreamProviderName)
                     .AddMemoryGrainStorage("PubSubStore")
                     .Configure<SiloMessagingOptions>(options => options.ClientDropTimeout = TimeSpan.FromSeconds(5));
            }
        }
        public class ClientConfiguretor : IClientBuilderConfigurator
        {
            public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
            {
                clientBuilder.AddSimpleMessageStreamProvider(SMSStreamProviderName);
            }
        }

        [Fact, TestCategory("SlowBVT"), TestCategory("Functional"), TestCategory("Streaming")]
        public async Task SMSStreamProducerOnDroppedClientTest()
        {
            logger.LogInformation("************************ SMSStreamProducerOnDroppedClientTest *********************************");
            await runner.StreamProducerOnDroppedClientTest(SMSStreamProviderName, StreamNamespace);
        }

        [Fact, TestCategory("SlowBVT"), TestCategory("Functional"), TestCategory("Streaming")]
        public async Task SMSStreamConsumerOnDroppedClientTest()
        {
            logger.LogInformation("************************ SMSStreamConsumerOnDroppedClientTest *********************************");
            await runner.StreamConsumerOnDroppedClientTest(SMSStreamProviderName, StreamNamespace, output);
        }
    }
}
