using AutoFixture;
using AutoFixture.NUnit3;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using NUnitTestBase;
using OrderService.API.Consumers;
using OrderService.API.DAL;
using OrderService.Contracts;
using Serilog;
using System;
using System.Threading.Tasks;

namespace API.UnitTests
{
    public class NewOrderCommandHandlerTests
    {
        private readonly Fixture fixture = new Fixture();
        private IOrderRepository mockOrderRepository;
        private IServiceProvider provider;
        private InMemoryTestHarness harness;

        [OneTimeSetUp]
        public async Task Setup()
        {
            this.mockOrderRepository = Substitute.For<IOrderRepository>();

            this.provider = new ServiceCollection()
                    .AddMassTransitInMemoryTestHarness(cfg =>
                    {
                        cfg.AddConsumer<NewOrderCommandHandler>();
                    })
                    .AddSerilogNunit()
                    .AddHttpClient()
                    .AddSingleton(mockOrderRepository)
                    .BuildServiceProvider(true);

            harness = provider.GetRequiredService<InMemoryTestHarness>();
            await harness.Start().ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task TearDownAsync()
        {
            await harness.Stop().ConfigureAwait(false);
        }

        [Test]
        [AutoData]
        public async Task typical_case(Order expectedNewOrder)
        {
            //Arrange
            mockOrderRepository.AddNewOrder(Arg.Any<NewOrderCommand>()).ReturnsForAnyArgs(expectedNewOrder);

            var bus = provider.GetRequiredService<IBus>();
            var requester = bus.CreateRequestClient<NewOrderCommand>();

            //Act
            var input = fixture.Create<NewOrderCommand>();
            Log.Logger.Information($"invoke a normal {{{nameof(NewOrderCommand)}}}", input);
            var response = await requester.GetResponse<Order>(input).ConfigureAwait(false);
            Order output = response.Message;
            Log.Logger.Information($"output should be the same as the mocked object {{{nameof(Order)}}}", output);

            //Assert
            Assert.That(output, Is.EqualTo(expectedNewOrder));
        }

        [Test]
        [AutoData]
        public async Task error_case(Order randomOrder)
        {
            //Arrange
            mockOrderRepository.AddNewOrder(Arg.Any<NewOrderCommand>()).ReturnsForAnyArgs(randomOrder);
            var bus = provider.GetRequiredService<IBus>();
            var requester = bus.CreateRequestClient<NewOrderCommand>();

            //Act
            var input = fixture.Build<NewOrderCommand>().With(p => p.StatusCode, -1).Create();
            Log.Logger.Information($"invoke an invalid statusCode {{{nameof(NewOrderCommand)}}}", input);

            var ex = Assert.ThrowsAsync<RequestFaultException>(async () =>
            {
                await requester.GetResponse<Order>(input).ConfigureAwait(false);
            });
            Assert.AreEqual(typeof(ArgumentException).FullName, ex.Fault.Exceptions[0].ExceptionType);
        }
    }
}
