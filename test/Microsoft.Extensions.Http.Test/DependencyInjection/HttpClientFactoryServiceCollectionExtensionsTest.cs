﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    // These are mostly integration tests that verify the configuration experience.
    public class HttpClientFactoryServiceCollectionExtensionsTest
    {
        [Fact] // Verifies that AddHttpClient is enough to get the factory and make clients.
        public void AddHttpClient_IsSelfContained_CanCreateClient()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Act1
            serviceCollection.AddHttpClient(); 

            var services = serviceCollection.BuildServiceProvider();
            var options = services.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>();

            var factory = services.GetRequiredService<IHttpClientFactory>();

            // Act2
            var client = factory.CreateClient();

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void AddHttpClient_WithDefaultName_ConfiguresDefaultClient()
        {
            var serviceCollection = new ServiceCollection();

            // Act1
            serviceCollection.AddHttpClient(Options.Options.DefaultName, c => c.BaseAddress = new Uri("http://example.com/"));

            var services = serviceCollection.BuildServiceProvider();
            var options = services.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>();

            var factory = services.GetRequiredService<IHttpClientFactory>();

            // Act2
            var client = factory.CreateClient();

            // Assert
            Assert.NotNull(client);
            Assert.Equal("http://example.com/", client.BaseAddress.AbsoluteUri);
        }

        [Fact]
        public void AddHttpClient_WithName_ConfiguresNamedClient()
        {
            var serviceCollection = new ServiceCollection();

            // Act1
            serviceCollection.AddHttpClient("example.com", c => c.BaseAddress = new Uri("http://example.com/"));

            var services = serviceCollection.BuildServiceProvider();
            var options = services.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>();

            var factory = services.GetRequiredService<IHttpClientFactory>();

            // Act2
            var client = factory.CreateClient("example.com");

            // Assert
            Assert.NotNull(client);
            Assert.Equal("http://example.com/", client.BaseAddress.AbsoluteUri);
        }

        [Fact]
        public void AddHttpMessageHandler_WithName_NewHandlerIsSurroundedByLogging()
        {
            var serviceCollection = new ServiceCollection();

            HttpMessageHandlerBuilder builder = null;

            // Act1
            serviceCollection.AddHttpClient("example.com").AddHttpMessageHandlerBuilderOptions(b =>
            {
                builder = b;

                b.AdditionalHandlers.Add(Mock.Of<DelegatingHandler>());
            });

            var services = serviceCollection.BuildServiceProvider();
            var options = services.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>();

            var factory = services.GetRequiredService<IHttpClientFactory>();

            // Act2
            var client = factory.CreateClient("example.com");

            // Assert
            Assert.NotNull(client);

            Assert.Collection(
                builder.AdditionalHandlers,
                h => Assert.IsType<LoggingScopeHttpMessageHandler>(h),
                h => Assert.NotNull(h),
                h => Assert.IsType<LoggingHttpMessageHandler>(h));
        }
    }
}
