-- minecraft version 1.20.1
-- requires cc-tweaked mod (runs on advanced computer)

app_settings = {}
app_settings.agent_id = '11111111-2222-3333-4444-000000000000'
app_settings.address = 'http://localhost:5113/api/agent/' .. app_settings.agent_id

rce2_contacts = {}
rce2_contacts.ins = {}
rce2_contacts.ins.display_text = "display-text"
rce2_contacts.outs = {}
rce2_contacts.outs.trigger1 = "trigger1"

function handle_display_text(rce2_message)
  print(rce2_message.payload.data)
end

function handle_whois()
  print("Responding to whois")
  local body = [[
    {
      "type":"whois",
      "payload":{
        "id":"]] .. app_settings.agent_id .. [[",
        "Name":"Minecraft Agent",
        "ins":{
          "]] .. rce2_contacts.ins.display_text .. [[":"string"
        },
        "outs":{
          "]] .. rce2_contacts.outs.trigger1 .. [[":"bool"
        }
      }
    }
  ]]
  http.post(app_settings.address,
    body,
    {
      ["content-type"] = "application/json",
      ["content-length"] = tostring(#body)
    })
end

function feed_handler()
  local response = http.get(app_settings.address)

  if response == nil or response.getResponseCode() ~= 200 then
    print("Feed handler error")
    os.sleep(1)
    return
  end
  
  local responseJson = response.readAll()
  if responseJson == nil or responseJson == "" then
    print("Feed response error")
    os.sleep(1)
    return
  end

  local responseArray = textutils.unserializeJSON(responseJson)
  for _,value in pairs(responseArray) do

    if value.contact == rce2_contacts.ins.display_text then
      handle_display_text(value)
      --goto continue
    end

    if value.type == "whois" then
      handle_whois()
      --goto continue
    end

    --continue not available in this version of lua
    --::continue::
  end

  --yield
  os.sleep(0)
end

-- Program starts here
while(true) do
  feed_handler()
end
