$agentId = [guid]::NewGuid()
$address = "https://localhost:7113/api/agent/$agentId"

function GetFeed() {
    Write-Host Getting Feed
    $response = Invoke-WebRequest -UseBasicParsing -Uri $address
    return $response | ConvertFrom-Json
}

function HandleMessage($message) {
    if ($message) {
        switch ($message.contact) {
            "count" { StartCounting $message; break }
            default { SendWhoIs; break }
        }
    }
}

function SendWhoIs() {
    $payload = @{
        type = 'whois'
        payload = @{
            id = "$agentId"
            name = 'Powershell'
            ins = @{
                'count' = 'number-list'
            }
            outs = @{
            }
        }
    }
    $_ = Invoke-WebRequest -UseBasicParsing -Uri $address -Method Post -ContentType 'application/json' -Body ($payload | ConvertTo-Json -Depth 10)
}

function StartCounting($message) {
    $from = $message.payload.data[0]
    $to = $message.payload.data[1]
    for ($i = $from; $i -lt $to; $i++) {
        Write-Host $i
        Start-Sleep 1
    }
}

while ($true) {
    $feed = GetFeed
    foreach ($message in $feed) {
        HandleMessage $message
    }
}
