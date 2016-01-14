using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Java2Dotnet.Spider.Core;

namespace Java2Dotnet.Spider.Extension.Test
{
    [TestClass()]
    public class RequestTests
    {
        public static Request GetRequest()
        {
            var extras = new Dictionary<string, dynamic>();
            extras.Add("Test", "Forever");
            var request = new Request("www.taobao.com", 2, extras)
            {
                Method="get",
                Priority=1
            };
            return request;
        }

        [TestMethod()]
        public void RequestTest()
        {
            var request = GetRequest();
            Assert.AreEqual(request.Extras.Count,2);
            Assert.AreEqual(request.Extras["Test"], "Forever");
        }
        [TestMethod()]
        public void PutExtraTest()
        {
            var request = GetRequest();
            request.PutExtra(null,null);
            request.PutExtra("", null);
            request.PutExtra("","");
            request.PutExtra("", "");
            request.PutExtra("One", "One");
            request.PutExtra("One", "One");
            Assert.AreEqual(request.Extras.Count, 4);
            Assert.AreEqual(request.Extras["One"], "One");
            Assert.AreEqual(request.Extras[""], "");
        }

        [TestMethod()]
        public void GetExtraTest()
        {
            var request = GetRequest();
            request.PutExtra("One",new {Name= "John" });
            Assert.AreEqual(request.Extras["One"], new { Name = "John" });
            Assert.AreEqual(request.Extras["deep"], 2);
        }

        [TestMethod()]
        public void DisposeTest()
        {
            var request = GetRequest();
            Assert.AreEqual(request.Extras.Count, 2);
            request.Dispose();
            Assert.AreEqual(request.Extras.Count,0);
        }


        [TestMethod()]
        public void CloneTest()
        {
            var request = GetRequest();
            var clone =(Request)request.Clone();
            Assert.AreEqual(request.Extras.Count, clone.Extras.Count);
            Assert.AreEqual(request.Extras["deep"], clone.Extras["deep"]);
            Assert.AreEqual(request.Extras["Test"], clone.Extras["Test"]);
            Assert.AreEqual(request.Url,clone.Url);
            Assert.AreEqual(request.Method, clone.Method);
            Assert.AreEqual(request.Priority, clone.Priority);
        }

    }
}
