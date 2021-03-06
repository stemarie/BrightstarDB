﻿using System;
using System.Linq;
using System.Xml.Linq;
using BrightstarDB;
using BrightstarDB.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RestClientTester
{
    public class BasicOperations : RestClientTest
    {
        private readonly IBrightstarService _client;

        public BasicOperations(ConnectionString connectionString) : base(connectionString)
        {
            _client = BrightstarService.GetClient(connectionString);
        }

        public void TestListStores()
        {
            var stores = _client.ListStores();
            Assert.IsNotNull(stores);
        }

        public void TestCreateStore()
        {
            //var storeName = "TestCreate-" + DateTime.Now.Ticks;
            var storeName = Guid.NewGuid().ToString();
            _client.CreateStore(storeName);
            var stores = _client.ListStores();
            Assert.IsNotNull(stores);
            Assert.IsTrue(stores.Contains(storeName), "Did not find newly created store in call to ListStores");
        }

        public void TestDeleteStore()
        {
            var storeName = Guid.NewGuid().ToString();
            _client.CreateStore(storeName);
            var stores = _client.ListStores();
            Assert.IsNotNull(stores);
            Assert.IsTrue(stores.Contains(storeName), "Did not find newly created store in call to ListStores");

            _client.DeleteStore(storeName);
            // Delete processes asynchronously, so there is a delay between 
            stores = _client.ListStores();
            Assert.IsNotNull(stores);
            Assert.IsFalse(stores.Contains(storeName), "Expected to not find deleted store name in call to ListStores");

        }

        public void TestDoesStoreExist()
        {
            var storeName = Guid.NewGuid().ToString();
            _client.CreateStore(storeName);
            Assert.IsTrue(_client.DoesStoreExist(storeName), "Expected DoesStoreExist to return true after store created");
            Assert.IsFalse(_client.DoesStoreExist("foo" + storeName), "Expected DoesStoreExist to return false for invalid name");
        }

        public void TestTransactionJob()
        {
            var storeName = Guid.NewGuid().ToString();
            _client.CreateStore(storeName);
            var jobInfo = _client.ExecuteTransaction(storeName, "", "",
                                       "<http://example.org/np> <http://example.org/employs> <http://example.org/kal>.");
            Assert.IsNotNull(jobInfo);
            Assert.IsTrue(jobInfo.JobCompletedOk);
            Assert.IsNotNull(jobInfo.StatusMessage);
        }

        public void TestQuery()
        {
            var storeName = Guid.NewGuid().ToString();
            _client.CreateStore(storeName);
            XDocument responseDoc = XDocument.Load(_client.ExecuteQuery(storeName, "SELECT ?s ?p ?o WHERE { ?s ?p ?o }"));
            Assert.IsNotNull(responseDoc);
            Assert.IsNotNull(responseDoc.Root);
            Assert.AreEqual("http://www.w3.org/2005/sparql-results#", responseDoc.Root.Name.Namespace);
            Assert.AreEqual("sparql", responseDoc.Root.Name.LocalName);
        }
    }
}
