﻿using System;
using System.Web;
using Moq;
using NUnit.Framework;

namespace Canonicalize.Tests
{
    class CanonicalizeRouteTests
    {
        [Test]
        public void route_without_rules_not_routed()
        {
            var route = new CanonicalizeRoute();
            var inputUrl = new Uri("http://example.com");
            var context = CreateFakeHttpContext(inputUrl);
            
            var routeData = route.GetRouteData(context);

            Assert.Null(routeData);
        }

        [Test]
        public void route_rules_are_invoked()
        {
            var filter = new Mock<IRule>();
            var inputUri = new UriBuilder("http://example.com");
            filter.Setup(x => x.Apply(inputUri)).Verifiable();

            var route = new CanonicalizeRoute();
            route.Rules.Add(filter.Object);

            var context = CreateFakeHttpContext(inputUri.Uri);

            route.GetRouteData(context);

            filter.Verify();
        }

        [Test]
        public void route_with_nonchanging_rule_not_routed()
        {
            var filter = new Mock<IRule>();
            var inputUri = new UriBuilder("http://example.com");
            filter.Setup(x => x.Apply(inputUri));

            var route = new CanonicalizeRoute();
            route.Rules.Add(filter.Object);

            var context = CreateFakeHttpContext(inputUri.Uri);
            
            var routeData = route.GetRouteData(context);

            Assert.Null(routeData);
        }

        [Test]
        public void route_with_changing_rule_routed()
        {
            var filter = new Mock<IRule>();
            var inputUri = new UriBuilder("http://example.com");
            filter.Setup(x => x.Apply(inputUri)).Callback<UriBuilder>(x => x.Scheme = "https");

            var route = new CanonicalizeRoute();
            route.Rules.Add(filter.Object);

            var context = CreateFakeHttpContext(inputUri.Uri);
            var routeData = route.GetRouteData(context);

            Assert.NotNull(routeData);
            Assert.NotNull(routeData.RouteHandler);
            Assert.IsInstanceOf<RedirectHandler>(routeData.RouteHandler);
        }

        private static HttpContextBase CreateFakeHttpContext(Uri url)
        {
            return Mock.Of<HttpContextBase>(x =>
                x.Request == Mock.Of<HttpRequestBase>(y =>
                    y.Url == url
                ) &&
                x.Response == Mock.Of<HttpResponseBase>()
            );
        }
    }
}
