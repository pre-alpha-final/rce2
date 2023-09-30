-- minecraft version 1.20.1
-- requires cc-tweaked mod (runs on advanced turtle)
-- requires turtlematic chest (for creative chest)
-- requires unlimitedperipheralworks (for universal scanner)

app_settings = {}
app_settings.agent_id = '11111111-2222-3333-4444-000000000001'
app_settings.address = 'http://localhost:5113/api/agent/' .. app_settings.agent_id

rce2_contacts = {}
rce2_contacts.ins = {}
rce2_contacts.ins.run_command = "run-command"

function handle_run_command(rce2_message)
  print(rce2_message.payload.data)
  local result = load(rce2_message.payload.data)()
  print(result)
end

function handle_whois()
  print("Responding to whois")
  local body = [[
    {
      "type":"whois",
      "payload":{
        "id":"]] .. app_settings.agent_id .. [[",
        "Name":"Turtle 3d Printer Agent",
        "ins":{
          "]] .. rce2_contacts.ins.run_command .. [[":"string"
        },
        "outs":{
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

  if response == nil then
    return
  end

  if response.getResponseCode() ~= 200 then
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

    if value.contact == rce2_contacts.ins.run_command then
      handle_run_command(value)
      --goto continue
    end

    if value.type == "whois" then
      handle_whois()
      --goto continue
    end

    --continue not available in this version of lua
    --::continue::
  end
end

-- Program starts here
while(true) do
  feed_handler()
end
