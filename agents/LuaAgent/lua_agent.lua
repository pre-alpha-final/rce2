lunajson = require 'lunajson'
socket = require 'socket'
https = require 'ssl.https'
ltn12 = require 'ltn12'

app_settings = {}
app_settings.agent_id = '11111111-2222-3333-4444-555555555555'
app_settings.address = 'https://localhost:7113/api/agent/' .. app_settings.agent_id

rce2_contacts = {}
rce2_contacts.ins = {}
rce2_contacts.ins.input_text = "input-text"
rce2_contacts.outs = {}
rce2_contacts.outs.output_text = "output_text"

function handle_whois()
  local body = [[
    {
      "type":"whois",
      "payload":{
        "id":"]] .. app_settings.agent_id .. [[",
        "Name":"Lua Agent",
        "ins":{
          "]] .. rce2_contacts.ins.input_text .. [[":"string"
        },
        "outs":{
          "]] .. rce2_contacts.outs.output_text .. [[":"string"
        }
      }
   }
  ]]
  local _r, _c, _h, _s = https.request{
    method = "POST",
    url = app_settings.address,
    headers = {
      ["content-type"] = "application/json",
      ["content-length"] = tostring(#body)
    },
    source = ltn12.source.string(body)
  }
end

function handle_output_text(text)
  local body = [[
    {
      "type":"string",
      "contact":"]] .. rce2_contacts.outs.output_text .. [[",
      "payload":{
        "data":"]] .. text .. [["
      }
   }
  ]]
  local _r, _c, _h, _s = https.request{
    method = "POST",
    url = app_settings.address,
    headers = {
      ["content-type"] = "application/json",
      ["content-length"] = tostring(#body)
    },
    source = ltn12.source.string(body)
  }
end

function handle_input_text(rce2_message)
  local payload_data = rce2_message.payload.data
  handle_output_text(payload_data)
end

function feed_handler()
  local response = {}
  local _r, code, _h, _s = https.request{
    method = "GET",
    url = app_settings.address,
    sink = ltn12.sink.table(response)
  }

  if code ~= 200 then
    socket.sleep(1)
	return
  end

  local responseArray = lunajson.decode(table.concat(response))
  for _,value in pairs(responseArray) do
    
    if value.contact == rce2_contacts.ins.input_text then
      handle_input_text(value)
      goto continue
    end

    if value.type == "whois" then
      handle_whois()
      goto continue
    end

    ::continue::
  end
end

-- Program starts here
while(true)
do
  feed_handler()
end
