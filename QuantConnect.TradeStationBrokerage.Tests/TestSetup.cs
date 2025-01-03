﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Collections;
using QuantConnect.Logging;
using QuantConnect.Securities;
using QuantConnect.Configuration;
using static QuantConnect.Brokerages.TradeStation.Tests.TradeStationBrokerageTests;

namespace QuantConnect.Brokerages.TradeStation.Tests
{
    [TestFixture]
    public class TestSetup
    {
        [Test, TestCaseSource(nameof(TestParameters))]
        public void TestSetupCase()
        {
        }

        public static TradeStationBrokerageTest CreateBrokerage(IOrderProvider orderProvider, ISecurityProvider securityProvider)
        {
            var clientId = Config.Get("trade-station-client-id");
            var clientSecret = Config.Get("trade-station-client-secret");
            var restApiUrl = Config.Get("trade-station-api-url");
            var accountType = Config.Get("trade-station-account-type");
            var refreshToken = Config.Get("trade-station-refresh-token");

            if (string.IsNullOrEmpty(refreshToken))
            {
                var redirectUrl = Config.Get("trade-station-redirect-url");
                var authorizationCode = Config.Get("trade-station-authorization-code");

                if (new string[] { redirectUrl, authorizationCode }.Any(string.IsNullOrEmpty))
                {
                    throw new ArgumentException("RedirectUrl or AuthorizationCode cannot be empty or null. Please ensure these values are correctly set in the configuration file.");
                }

                return new TradeStationBrokerageTest(clientId, clientSecret, restApiUrl, redirectUrl, authorizationCode, string.Empty,
                    accountType, orderProvider, securityProvider);
            }

            return new TradeStationBrokerageTest(clientId, clientSecret, restApiUrl, string.Empty, string.Empty, refreshToken, accountType, orderProvider, securityProvider);
        }

        public static void ReloadConfiguration()
        {
            // nunit 3 sets the current folder to a temp folder we need it to be the test bin output folder
            var dir = TestContext.CurrentContext.TestDirectory;
            Environment.CurrentDirectory = dir;
            Directory.SetCurrentDirectory(dir);
            // reload config from current path
            Config.Reset();

            var environment = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry entry in environment)
            {
                var envKey = entry.Key.ToString();
                var value = entry.Value.ToString();

                if (envKey.StartsWith("QC_"))
                {
                    var key = envKey.Substring(3).Replace("_", "-").ToLower();

                    Log.Trace($"TestSetup(): Updating config setting '{key}' from environment var '{envKey}'");
                    Config.Set(key, value);
                }
            }

            // resets the version among other things
            Globals.Reset();
        }

        private static void SetUp()
        {
            Log.LogHandler = new CompositeLogHandler();
            Log.Trace("TestSetup(): starting...");
            ReloadConfiguration();
            Log.DebuggingEnabled = Config.GetBool("debug-mode");
        }

        private static TestCaseData[] TestParameters
        {
            get
            {
                SetUp();
                return new[] { new TestCaseData() };
            }
        }
    }
}
