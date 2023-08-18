using CESDK;
using CheatEngineAgent.Rce2;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net;

namespace CheatEngineAgent
{
    public class Main : CESDKPluginClass
    {
        private static Guid AgentId = Guid.NewGuid();
        private static string Address = $"https://localhost:7113/api/agent/{AgentId}";

        public override bool DisablePlugin()
        {
            return true;
        }

        public override bool EnablePlugin()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            MessageBox.Show("Cheat Engine Agent Running");
            _ = Task.Run(FeedHandler);

            return true;
        }

        public override string GetPluginName()
        {
            return "Cheat Engien Agent";
        }

        private async Task FeedHandler()
        {
            using (var httpClient = new HttpClient())
            {
                while (true)
                {
                    try
                    {
                        var feed = await httpClient.GetAsync(Address);
                        var content = await feed.Content.ReadAsStringAsync();
                        var rce2Messages = JsonConvert.DeserializeObject<List<Rce2Message>>(content);
                        foreach (var rce2Message in rce2Messages)
                        {
                            switch (rce2Message.Contact)
                            {
                                case Rce2Contacts.Ins.ReadAddressList:
                                    await TryRun(() => HandleReadAddressList(rce2Message.Payload));
                                    break;

                                case Rce2Contacts.Ins.Toggle:
                                    await TryRun(() => HandleToggle(rce2Message.Payload));
                                    break;

                                case Rce2Contacts.Ins.Get:
                                    await TryRun(() => HandleGet(rce2Message.Payload));
                                    break;

                                case Rce2Contacts.Ins.Set:
                                    await TryRun(() => HandleSet(rce2Message.Payload));
                                    break;

                                default:
                                    if (rce2Message.Type == Rce2Types.WhoIs)
                                    {
                                        await TryRun(HandleWhoIsMessage);
                                    }
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        await Task.Delay(1000);
                        // ignore
                    }
                }
            }
        }

        private async Task HandleReadAddressList(JToken payload)
        {
            _sdk.lua.DoString(@"
                local addressList = getAddressList()
                addressListTable = {}
                table.insert(addressListTable, addressList.Count)
                for i = 0, addressList.Count - 1, 1 do
                    table.insert(addressListTable, addressList[i].Description)
                    table.insert(addressListTable, addressList[i].Value)
                end
            ");

            _sdk.lua.GetGlobal("addressListTable");
            _sdk.lua.PushInteger(1);
            _sdk.lua.GetTable(-2);
            var addressListTableCount = _sdk.lua.ToInteger(-1);
            _sdk.lua.Pop(1);

            var addressList = new List<string>();
            for (var i = 1; i <= addressListTableCount; i++)
            {
                _sdk.lua.PushInteger(i * 2);
                _sdk.lua.GetTable(-2);
                var description = _sdk.lua.ToString(-1);
                _sdk.lua.Pop(1);

                _sdk.lua.PushInteger(i * 2 + 1);
                _sdk.lua.GetTable(-2);
                var value = _sdk.lua.ToString(-1);
                _sdk.lua.Pop(1);

                addressList.Add($"{description} = {value}");
            }

            _sdk.lua.Pop(1);

            using (var httpClient = new HttpClient())
            {
                await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
                {
                    Type = Rce2Types.String,
                    Contact = Rce2Contacts.Outs.AddressList,
                    Payload = JObject.FromObject(new { data = addressList })
                }), Encoding.UTF8, "application/json"));
            }
        }

        private async Task HandleToggle(JToken payload)
        {
            _sdk.lua.DoString($@"
                local addressList = getAddressList()
                local memoryRecord = addressList.getMemoryRecord({payload["data"].ToObject<int>()})
                local isActive = memoryRecord.Active
                if isActive == true then
                    memoryRecord.Active = false
                else
                    memoryRecord.Active = true
                end
            ");
        }

        private async Task HandleGet(JToken payload)
        {
            _sdk.lua.DoString($@"
                local addressList = getAddressList()
                local memoryRecord = addressList.getMemoryRecord({payload["data"].ToObject<int>()})
                memoryRecordValue = memoryRecord.Value
            ");

            _sdk.lua.GetGlobal("memoryRecordValue");
            var memoryRecordValue = _sdk.lua.ToString(-1);
            _sdk.lua.Pop(1);

            using (var httpClient = new HttpClient())
            {
                await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
                {
                    Type = Rce2Types.String,
                    Contact = Rce2Contacts.Outs.Value,
                    Payload = JObject.FromObject(new { data = memoryRecordValue })
                }), Encoding.UTF8, "application/json"));
            }
        }

        private async Task HandleSet(JToken payload)
        {
            _sdk.lua.DoString($@"
                local addressList = getAddressList()
                local memoryRecord = addressList.getMemoryRecord({payload["data"].ToObject<List<string>>()[0]})
                memoryRecord.Value = {FormatValue(payload["data"].ToObject<List<string>>()[1])}
            ");
        }

        private string FormatValue(string value)
        {
            var isNumber = double.TryParse(value, out _);
            if (isNumber)
            {
                return value;
            }
            else
            {
                return $"\"{value}\"";
            }
        }

        private static async Task HandleWhoIsMessage()
        {
            using (var httpClient = new HttpClient())
            {
                await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
                {
                    Type = Rce2Types.WhoIs,
                    Payload = JObject.FromObject(new Rce2Agent
                    {
                        Id = AgentId,
                        Name = "Cheat Engine",
                        Ins = new Dictionary<string, string>
                        {
                            { Rce2Contacts.Ins.ReadAddressList, Rce2Types.Void },
                            { Rce2Contacts.Ins.Toggle, Rce2Types.Number },
                            { Rce2Contacts.Ins.Get, Rce2Types.Number },
                            { Rce2Contacts.Ins.Set, Rce2Types.StringList },
                        },
                        Outs = new Dictionary<string, string>
                        {
                            { Rce2Contacts.Outs.AddressList, Rce2Types.StringList },
                            { Rce2Contacts.Outs.Value, Rce2Types.String },
                        }
                    }),
                }), Encoding.UTF8, "application/json"));
            }
        }

        private static async Task TryRun(Func<Task> taskFunc)
        {
            try
            {
                await taskFunc();
            }
            catch (Exception e)
            {
                // Ignore
            }
        }
    }
}
