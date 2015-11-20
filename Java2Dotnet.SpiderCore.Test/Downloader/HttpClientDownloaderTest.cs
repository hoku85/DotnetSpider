using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using Java2Dotnet.Spider.Core.Downloader;
using Java2Dotnet.Spider.Core.Processor;
using Java2Dotnet.Spider.Core.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Java2Dotnet.Spider.Core.Test.Downloader
{
    /// <summary>
    /// HttoClientDownloaderTest 的摘要说明
    /// </summary>
    [TestClass]
    public class HttpClientDownloaderTest
    {
        public HttpClientDownloaderTest()
        {
            //
            //TODO:  在此处添加构造函数逻辑
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，该上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试特性
        //
        // 编写测试时，可以使用以下附加特性: 
        //
        // 在运行类中的第一个测试之前使用 ClassInitialize 运行代码
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // 在类中的所有测试都已运行之后使用 ClassCleanup 运行代码
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // 在运行每个测试之前，使用 TestInitialize 来运行代码
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // 在每个测试运行完之后，使用 TestCleanup 来运行代码
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestDownload()
        {
            var spider = Spider.Create(new SimplePageProcessor("http://www.oschina.net/", "http://www.oschina.net/*")).SetThreadNum(1);
            var pager = new HttpClientDownloader().Download(new Request("http://www.oschina.net/", 1, null), spider);
            Assert.AreEqual(pager.GetStatusCode(), 200);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(pager.GetRawText()));
        }

        [TestMethod]
        public void TestStatusAccept()
        {
            var acceptStatusCode = new List<int>() { 200, 301, 302 };
            var statusCode = 200;
            Assert.IsTrue((bool)PO.Invoke("StatusAccept", acceptStatusCode, statusCode));
        }
        [TestMethod]
        public void TestGetHttpWebRequest()
        {
            var request = new Request("http://www.oschina.net/", 1, null);
            var spider = Spider.Create(new SimplePageProcessor("http://www.oschina.net/", "http://www.oschina.net/*")).SetThreadNum(1);
            var site = spider.Site;
            var headers = site.GetHeaders();
            var webRequest = PO.Invoke("GetHttpWebRequest", request, site, headers);
            Assert.IsTrue(webRequest != null);
        }

        public PrivateObject PO { get; set; } = new PrivateObject(new HttpClientDownloader());

        [TestMethod]
        public void TestGeneratorCookie()
        {
            var request = new Request("http://www.oschina.net/", 1, null);
            var spider = Spider.Create(new SimplePageProcessor("http://www.oschina.net/", "http://www.oschina.net/*")).SetThreadNum(1);
            var site = spider.Site;
            var headers = site.GetHeaders();
            var webRequest = PO.Invoke("GetHttpWebRequest", request, site, headers);
            var cookieWebRequest = (HttpWebRequest)PO.Invoke("GeneratorCookie", webRequest, site);
            Assert.IsTrue(cookieWebRequest.CookieContainer != null);
        }

        [TestMethod]
        public void TestSelectRequestMethod()
        {
            var request = new Request("http://www.oschina.net/", 1, null);
            request.Method = "GET";
            var webRequest = PO.Invoke("SelectRequestMethod", request);
            Assert.AreEqual(request.Method, HttpConstant.Method.Get);
        }

        [TestMethod]
        public void TestHandleResponse()
        {
            var request = new Request("http://www.oschina.net/", 1, null);
            var spider = Spider.Create(new SimplePageProcessor("http://www.oschina.net/", "http://www.oschina.net/*")).SetThreadNum(1);
            var site = spider.Site;
            var headers = site.GetHeaders();
            var charset = site.Encoding;
            var webRequest = (HttpWebRequest)PO.Invoke("GetHttpWebRequest", request, site, headers);
            var response = (HttpWebResponse)webRequest.GetResponse();
            var statusCode = (int)response.StatusCode;
            var pager = (Page)PO.Invoke("HandleResponse", request, charset, response, statusCode);
            Assert.AreEqual(pager.GetStatusCode(), 200);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(pager.GetRawText()));
        }

        [TestMethod]
        public void TestGetContent()
        {
            var request = new Request("http://www.oschina.net/", 1, null);
            var spider = Spider.Create(new SimplePageProcessor("http://www.oschina.net/", "http://www.oschina.net/*")).SetThreadNum(1);
            var site = spider.Site;
            var headers = site.GetHeaders();
            var charset = site.Encoding;
            var webRequest = (HttpWebRequest)PO.Invoke("GetHttpWebRequest", request, site, headers);
            var response = (HttpWebResponse)webRequest.GetResponse();
            var result = (string)PO.Invoke("GetContent", charset, response);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(result));
        }

        [TestMethod]
        public void TestGetContentBytes()
        {
            var request = new Request("http://www.oschina.net/", 1, null);
            var spider = Spider.Create(new SimplePageProcessor("http://www.oschina.net/", "http://www.oschina.net/*")).SetThreadNum(1);
            var site = spider.Site;
            var headers = site.GetHeaders();
            var charset = site.Encoding;
            var webRequest = (HttpWebRequest)PO.Invoke("GetHttpWebRequest", request, site, headers);
            var response = (HttpWebResponse)webRequest.GetResponse();
            var responseBytes = (byte[])PO.Invoke("GetContentBytes", response);
            Assert.IsTrue(responseBytes.Length > 0);
        }

        [TestMethod]
        public void TestGetHtmlCharset()
        {
            var request = new Request("http://www.oschina.net/", 1, null);
            var spider = Spider.Create(new SimplePageProcessor("http://www.oschina.net/", "http://www.oschina.net/*")).SetThreadNum(1);
            var site = spider.Site;
            var headers = site.GetHeaders();
            var charset = site.Encoding;
            var webRequest = (HttpWebRequest)PO.Invoke("GetHttpWebRequest", request, site, headers);
            var response = (HttpWebResponse)webRequest.GetResponse();
            var responseBytes = (byte[])PO.Invoke("GetContentBytes", response);
            var encoding = PO.Invoke("GetHtmlCharset", response.ContentType, responseBytes);
            Assert.IsTrue(encoding==Encoding.UTF8);
        }

    }
}
