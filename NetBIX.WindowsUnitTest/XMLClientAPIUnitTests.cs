using System;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetBIX.oBIX.Client;
using NetBIX.oBIX.Client.Framework;
using NetBIX.oBIX.Client.Extensions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Flurl;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

namespace NetBIX.WindowsUnitTest {
    [TestClass]
    public class XMLClientAPIUnitTests {
        public Uri obixLobby = new Uri("http://obix.server/obix");
        public XmlObixClient client = null;
        public Thread wndThread = null;
        public CProgressWnd progressWnd = null;

        [TestInitialize]
        public void GlobalInitialize() {
            client = new XmlObixClient(obixLobby);
           
            Assert.IsNotNull(client, "Initializing the oBIX client failed.");
            client.ErrorStack.ErrorAdded += ObixError_ErrorAdded;
        }

        [TestCleanup]
        public void GlobalCleanup() {
            if (client.ErrorStack.HasErrors) {
                Console.Out.WriteLine("\r\n\r\n\r\n");
                Console.Out.WriteLine("--- Error Stack ---");
                foreach (ObixError err in client.ErrorStack.PopArray()) {
                    Console.Out.WriteLine(err.ToString());
                    if (string.IsNullOrEmpty(err.AuxErrorMessage) == false) {
                        Console.Out.WriteLine(" - " + err.AuxErrorMessage);
                    }
                }
            }

            client.Dispose();
            client = null;
        }

        void ObixError_ErrorAdded(object sender, ObixErrorEventArgs e) {
            Console.Out.WriteLine(e.Error.ToString());
        }

        /// <summary>
        /// Creates a test oBIX device to post to the server.
        /// </summary>
        XElement CreateTestDevice() {
            XElement sampleDevice = null;
            string sampleDeviceString = string.Format(
                @"<obj name='asyncUnitTest' href='/obix/deviceRoot/AsyncUnitTest-{0}'>
                    <str name='test' href='testStr' val='This is a test string' writable='true' />
                    <bool name='testBool' href='testBool' val='true' writable='true' />
                  </obj>", Guid.NewGuid());

            try {
                sampleDevice = XElement.Parse(sampleDeviceString);
            } catch (Exception) {
                //stub
            }

            return sampleDevice;
        }

        /// <summary>
        /// Tests the oBIX client's Connect() functionality.
        /// </summary>
        [TestMethod]
        public void TestConnect() {
            ObixResult result;

            result = client.Connect();
            Console.Out.WriteLine("Connect result: {0}", result);

            Assert.AreEqual<int>(result, ObixResult.kObixClientSuccess,
                string.Format("Connect failed with error: {0}: {1}", result, ObixResult.Message((int)result)));
        }

        /// <summary>
        /// Twsts the oBIX client's connectAsync() functionality.
        /// </summary>
        [TestMethod]
        public async Task TestConnectAsync() {
            ObixResult result;

            Assert.IsNotNull(client, "Initializing the oBIX client failed.");

            result = await client.ConnectAsync();
            Console.Out.WriteLine("Connect result: {0}", result);

            Assert.AreEqual<int>(result, ObixResult.kObixClientSuccess,
                string.Format("Connect failed with error: {0}: {1}", result, ObixResult.Message((int)result)));
        }

        /// <summary>
        /// Tests the oBIX server's features as parsed by the connect method.
        /// </summary>
        [TestMethod]
        public void TestObixFeatures() {
            TestConnect();

            Assert.IsNotNull(client, "Client is not initialized.");
            Assert.IsTrue(client.IsConnected, "Client is not connected.");

            Assert.IsNotNull(client.BatchUri, "oBIX Server connected but could not find an oBIX:Batch endpoint.");
            Assert.IsNotNull(client.AboutUri, "oBIX Server connected but could not find an oBIX:About endpoint.");
            Assert.IsNotNull(client.WatchUri, "oBIX Server connected but could not find an oBIX:WatchService endpoint.");
            Assert.IsNotNull(client.About, "oBIX Server connected but has no obix:About object.");

            Console.Out.WriteLine("oBIX Server details:");
            Console.Out.WriteLine("Lobby: " + client.LobbyUri.ToString());
            Console.Out.WriteLine("Watch: " + client.WatchUri.ToString());
            Console.Out.WriteLine("Batch: " + client.BatchUri.ToString());

            Console.Out.WriteLine();
            Console.Out.WriteLine(client.About.ToString());
        }


        [TestMethod]
        public void TestReadXmlUri() {
            ObixResult<XElement> lobby;

            TestConnect();

            Assert.IsNotNull(client, "Client is not initialized.");
            Assert.IsTrue(client.IsConnected, "Client is not connected.");

            lobby = client.ReadUriXml(client.LobbyUri);
            Assert.IsNotNull(lobby, "ReadUriXml failed, lobby is null.");
            Assert.IsTrue(lobby.ResultSucceeded, "ReadUriXml failed with result " + lobby.ToString());

            Assert.IsFalse(lobby.Result.IsNullOrNullContract(), "object returned is null or an obix:Null contract.");

            Console.Out.WriteLine("Lobby output:");
            Console.Out.WriteLine(lobby.ToString());
        }

        [TestMethod]
        public async Task TestReadXmlUriAsync() {
            ObixResult<XElement> lobby;
            await TestConnectAsync();

            Assert.IsNotNull(client, "Client is not initialized.");
            Assert.IsTrue(client.IsConnected, "Client is not connected.");

            lobby = await client.ReadUriXmlAsync(client.LobbyUri);
            Assert.IsNotNull(lobby, "ReadUriXml failed, lobby is null.");
            Assert.IsTrue(lobby.ResultSucceeded, "ReadUriXml failed with result " + lobby.ToString());

            Assert.IsFalse(lobby.Result.IsNullOrNullContract(), "object returned is null or an obix:Null contract.");

            Console.Out.WriteLine("Lobby output:");
            Console.Out.WriteLine(lobby.ToString());
        }

        [TestMethod]
        public void TestInvokeXmlUri() {
            ObixResult<XElement> result;
            XElement sampleDevice = CreateTestDevice();

            Assert.IsNotNull(sampleDevice, "TestInvokeXmlUri failed to create a sample device oBIX object.");

            TestConnect();
            Assert.IsNotNull(client, "Client is not initialized.");
            Assert.IsTrue(client.IsConnected, "Client is not connected.");

            result = client.InvokeUriXml(new Uri(Url.Combine(client.LobbyUri.ToString(), "signUp")), sampleDevice);

            //Error contract is still a valid response from the server.
            Assert.IsTrue(result.ResultSucceeded || result.Result.IsObixErrorContract(), "WriteUri received result: " + result);
        }

        [TestMethod]
        public async Task TestInvokeXmlUriAsync() {
            ObixResult<XElement> result;
            XElement sampleDevice = CreateTestDevice();

            Assert.IsNotNull(sampleDevice, "TestInvokeXmlUriAsync failed to create a sample device oBIX object.");

            TestConnect();
            Assert.IsNotNull(client, "Client is not initialized.");
            Assert.IsTrue(client.IsConnected, "Client is not connected.");

            result = await client.InvokeUriXmlAsync(new Uri(Url.Combine(client.LobbyUri.ToString(), "signUp")), sampleDevice);

            //Error contract is still a valid response from the server.
            Assert.IsTrue(result.ResultSucceeded || result.Result.IsObixErrorContract(), "WriteUri received result: " + result);
        }

        [TestMethod]
        public void TestDeviceIO() {
            ObixResult<XElement> xmlResult = null;
            XElement sampleDevice = null;
            XElement remoteSampleDevice = null;
            Uri deviceUri = null;
            Uri signupUri = client.LobbyUri.Concat("signUp");

            TestConnect();
            Assert.IsNotNull(client, "Client is not initialized.");
            Assert.IsTrue(client.IsConnected, "Client is not connected.");

            sampleDevice = CreateTestDevice();
            Assert.IsNotNull(sampleDevice, "TestDeviceWrite failed to create a sample device oBIX object.");

            xmlResult = client.InvokeUriXml(signupUri, sampleDevice);
            Assert.IsTrue(xmlResult.ResultSucceeded, string.Format("Device signup to href {0} failed with result {1}.", signupUri, xmlResult));

            deviceUri = obixLobby.Concat(sampleDevice.ObixHref());
            Assert.IsNotNull(deviceUri, "deviceUri is null");

            Console.WriteLine("Device registered at URI {0}", deviceUri.ToString());

            xmlResult = client.ReadUriXml(deviceUri);
            Assert.IsTrue(xmlResult.ResultSucceeded && xmlResult.Result.IsObixErrorContract() == false, 
                string.Format("Failed to retrieve created device at href {0} after successfully submitting it.", deviceUri));

            remoteSampleDevice = xmlResult.Result;
            Assert.IsNotNull(remoteSampleDevice);

            xmlResult = client.ReadUriXml(deviceUri.Concat("testBool"));
            Assert.IsTrue(xmlResult.ResultSucceeded && xmlResult.Result.IsObixErrorContract() == false, "oBIX Read testBool failed.");
            Assert.IsNotNull(xmlResult.Result.ObixBoolValue(), "oBIX Read testBool failed: element returned is not an obix:bool.");
            Assert.IsTrue(xmlResult.Result.ObixBoolValue().Value, "testBool on sampleDevice is not true.");

            xmlResult = client.WriteUriXml(deviceUri.Concat("testBool"), false.ObixXmlValue());
            Assert.IsTrue(xmlResult.ResultSucceeded && xmlResult.Result.IsObixErrorContract() == false, "oBIX write failed.");
               

            xmlResult = client.ReadUriXml(deviceUri.Concat("testBool"));
            Assert.IsTrue(xmlResult.ResultSucceeded && xmlResult.Result.IsObixErrorContract() == false, "oBIX Read testBool failed.");
            Assert.IsNotNull(xmlResult.Result.ObixBoolValue(), "oBIX Read testBool failed: element returned is not an obix:bool.");
            Assert.IsFalse(xmlResult.Result.ObixBoolValue().Value, "Changing testBool on sampleDevice failed: testBool on the server side is the same value.");
        }

        [TestMethod]
        public void TestDeviceBulk() {
            int maxDevices = 10000;
            ObixResult<XElement> xmlResult = null;
            XElement sampleDevice = null;
            Uri signupUri = client.LobbyUri.Concat("signUp");

            progressWnd = new CProgressWnd("Posting oBIX devices");
            wndThread = new Thread(() => {
                progressWnd.ShowDialog();
            });

            wndThread.SetApartmentState(ApartmentState.STA);
            wndThread.Start();

            progressWnd.ProgressMaximumValue = maxDevices;

            TestConnect();
            Assert.IsNotNull(client, "Client is not initialized.");
            Assert.IsTrue(client.IsConnected, "Client is not connected.");

            for (int i = 0; i <= maxDevices; i++) {
                sampleDevice = CreateTestDevice();
                Assert.IsNotNull(sampleDevice, "TestDeviceWrite failed to create a sample device oBIX object.");

                xmlResult = client.InvokeUriXml(signupUri, sampleDevice);
                Assert.IsTrue(xmlResult.ResultSucceeded, string.Format("Device signup to href {0} failed with result {1}.", signupUri, xmlResult));
                progressWnd.Increment();
            }

            progressWnd.RunOnUIThread(() => {
                progressWnd.Hide();
            });

            wndThread.Join();
        }

        [TestMethod]
        public void TestWatchBulk() {
            int maxIterations = 10000;
            List<string> watchUriList = new List<string>(maxIterations);
            ObixResult<XElement> xmlResult = null;
            
            progressWnd = new CProgressWnd("Creating watch objects");
            wndThread = new Thread(() => {
                progressWnd.ShowDialog();
            });

            wndThread.SetApartmentState(ApartmentState.STA);
            wndThread.Start();

            progressWnd.ProgressMaximumValue = maxIterations;

            TestConnect();
            Assert.IsNotNull(client, "Client is not initialized.");
            Assert.IsTrue(client.IsConnected, "Client is not connected.");

            Uri u = client.LobbyUri.Concat("about/");

            for (int i = 0; i <= maxIterations; i++) {
                xmlResult = client.InvokeUriXml(client.LobbyUri.Concat("watchService/make/"), null);
                Assert.IsTrue(xmlResult.ResultSucceeded, "Could not add watch for iteration " + i);
                watchUriList.Add(xmlResult.Result.ObixHref());
                progressWnd.Increment();
            }
            progressWnd.Hide();

            progressWnd = new CProgressWnd("Deleting watch objects");
            wndThread = new Thread(() => {
                progressWnd.ShowDialog();
            });

            wndThread.SetApartmentState(ApartmentState.STA);
            wndThread.Start();

            progressWnd.ProgressMaximumValue = watchUriList.Count;

            for (int i = 0; i < watchUriList.Count; i++) {
                string watchUri = watchUriList[i];
                Uri deleteUri = client.LobbyUri.Concat(watchUri).Concat("delete/");
                xmlResult = client.InvokeUriXml(deleteUri, null);

                progressWnd.Increment();
                Assert.IsTrue(xmlResult.ResultSucceeded, "Could not add watch for URI " + deleteUri.ToString());
            }

            progressWnd.RunOnUIThread(() => {
                progressWnd.Hide();
            });
        }

        [TestMethod]
        public void TestGetBulk() {
            int maxIterations = 10000;
            ObixResult<XElement> xmlResult = null;
            Uri signupUri = client.LobbyUri.Concat("signUp");

            progressWnd = new CProgressWnd("Invoking obix reads");
            wndThread = new Thread(() => {
                progressWnd.ShowDialog();
            });

            wndThread.SetApartmentState(ApartmentState.STA);
            wndThread.Start();

            progressWnd.ProgressMaximumValue = maxIterations;

            TestConnect();
            Assert.IsNotNull(client, "Client is not initialized.");
            Assert.IsTrue(client.IsConnected, "Client is not connected.");

            Uri u = client.LobbyUri.Concat("about/");

            for (int i = 0; i <= maxIterations; i++) {
                xmlResult = client.ReadUriXml(u);
                progressWnd.Increment();
            }

            // Assert.IsTrue(xmlResult.ResultSucceeded, "Could not get lobby contract for iteration " + i);
            
            progressWnd.RunOnUIThread(() => {
                progressWnd.Hide();
            });
        }

        [TestMethod]
        public void TestObixBatchRead() {
            TestConnect();
            Assert.IsNotNull(client, "Client is not initialized.");
            Assert.IsTrue(client.IsConnected, "Client is not connected.");

            XmlBatch batch = client.Batch.CreateBatch();
            for (int i = 0; i < 50; i++) {
                batch.AddXmlBatchItem(ObixBatchOperation.kObixBatchOperationRead, "/obix");
            }

            Assert.AreNotEqual(client.Batch.SubmitBatch(ref batch), ObixResult.kObixClientSuccess, "SubmitBatch did not succceed.");

            batch.XmlBatchItemList.ForEach((item) => {
                Assert.IsNotNull(item.XmlBatchResponse, "An oBIX:BatchIn item is null, this should never be the case.");
            });
        }

        [TestMethod]
        public void TestObixBatchWrite() {
            TestConnect();
            Assert.IsNotNull(client, "Client is not initialized.");
            Assert.IsTrue(client.IsConnected, "Client is not connected.");
            bool boolSwitch = false;

            XmlBatch batch = client.Batch.CreateBatch();
            for (int i = 0; i < 50; i++) {
                batch.AddXmlBatchItem(ObixBatchOperation.kObixBatchOperationWrite, "/obix/test/TestDevice/bool", (boolSwitch = !boolSwitch).ObixXmlValue());
            }

            Assert.AreNotEqual(client.Batch.SubmitBatch(ref batch), ObixResult.kObixClientSuccess, "SubmitBatch did not succceed.");
            batch.XmlBatchItemList.ForEach((item) => {
                Assert.IsNotNull(item.XmlBatchResponse, "An oBIX:BatchIn item is null, this should never be the case.");
            });
            
            if (batch.XmlBatchItemList.Where(i => i.XmlBatchResponse.IsObixErrorContract()).Count() > 0) {
                Assert.Inconclusive("The test was inconclusive because the oBIX Batch mechanism returned error contracts in its response.");
            }
        }

        [TestMethod]
        public void TestObixPath() {
            ObixResult<XElement> result;

            TestConnect();
            Assert.IsNotNull(client, "Client is not initialized.");
            Assert.IsTrue(client.IsConnected, "Client is not connected.");


            result = client.ReadUriXml(client.LobbyUri.Concat("/obix/deviceRoot/M1/DH4/BCM/4A-1A/"));
            Assert.IsNotNull(result, "ReadUriXml failed.");
            Assert.IsTrue(result.ResultSucceeded, "ReadUriXml failed with result " + result.ToString());



        }

        [TestMethod]
        public void TestObixBatchInvoke() {
            XmlBatch deregisterBatch = null;
            int numberOfWatchesToCreate = 50;

            TestConnect();
            Assert.IsNotNull(client, "Client is not initialized.");
            Assert.IsTrue(client.IsConnected, "Client is not connected.");

            XmlBatch batch = client.Batch.CreateBatch();
            for (int i = 0; i < numberOfWatchesToCreate; i++) {
                batch.AddXmlBatchItem(ObixBatchOperation.kObixBatchOperationInvoke, "/obix/watchService/make");
            }

            Assert.AreNotEqual(client.Batch.SubmitBatch(ref batch), ObixResult.kObixClientSuccess, "SubmitBatch did not succceed.");

            batch.XmlBatchItemList.ForEach((item) => {
                Assert.IsNotNull(item.XmlBatchResponse, "An oBIX:BatchIn item is null, this should never be the case.");
            });

            foreach (ObixXmlBatchItem batchItem in batch.XmlBatchItemList) {
                ObixResult<XElement> readResult = client.ReadUriXml(new Uri(client.LobbyUri, batchItem.UriString));
                Assert.IsTrue(readResult.ResultSucceeded, "Read failed for URI " + batchItem.UriString);
                Assert.IsFalse(readResult.Result.IsObixErrorContract(), "Read result returned an error contract."); 
            }

            if (batch.XmlBatchItemList.Where(i => i.XmlBatchResponse.IsObixErrorContract()).Count() > 0) {
                Assert.Inconclusive("The test was inconclusive because the oBIX Batch mechanism returned error contracts in its response.");
            }

            deregisterBatch = client.Batch.CreateBatch();
            foreach (ObixXmlBatchItem batchItem in batch.XmlBatchItemList.Where(i => i.XmlBatchResponse.IsObixErrorContract() == false 
                && i.XmlBatchResponse.ObixIs().Contains("obix:Watch") == true)) {
                XElement watchObject = batchItem.XmlBatchResponse;
                deregisterBatch.AddXmlBatchItem(ObixBatchOperation.kObixBatchOperationInvoke, client.WatchUri.AbsolutePath + "/" + watchObject.ObixHref() + "/delete");
            }

            Assert.AreNotEqual(client.Batch.SubmitBatch(ref deregisterBatch), ObixResult.kObixClientSuccess, "SubmitBatch did not succceed on the deregister");
            foreach (ObixXmlBatchItem batchItem in deregisterBatch.XmlBatchItemList) {
                Assert.IsFalse(batchItem.XmlBatchResponse.IsObixErrorContract(), "Watch delete of uri " + batchItem.UriString + " returned an error contract: " + batchItem.XmlBatchResponse.ToString());
                Assert.IsTrue(batchItem.XmlBatchResponse.IsObixNullContract(), "Watch delete of uri " + batchItem.UriString + " did not return obix:nil contract as is required for obix:Watch delete operations.");
            }
        }
    }
}
