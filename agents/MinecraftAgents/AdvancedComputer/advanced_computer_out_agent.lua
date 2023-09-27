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

function redstone_handler()
  os.pullEvent("redstone")
  local value = redstone.getInput("front")

  print("Redstone set to: " .. tostring(value))
  local body = [[
    {
      "type":"bool",
      "contact":"]] .. rce2_contacts.outs.trigger1 .. [[",
      "payload":{
        "data":]] .. tostring(value) .. [[
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

-- Program starts here
while(true) do
  redstone_handler()
end
