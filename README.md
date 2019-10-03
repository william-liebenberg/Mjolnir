# Mjolnir

Mjolnir is a Azure Durable Functions app that can target a given URL with concurrent requests.

I used it to quickly and easily generate a bunch of traffic on a site.

# Usage

To use Mjolnir is quite easy.

Using postman you can `POST` a JSON payload to the `/api/Hammer` endpoint:

```
{
	"attempts": 100,
	"targetUrl": "https://thesite.you.wanna.test.com",
	"cookies": "cookie1=value1; cookie2=value2"
}
```

## TODO's

- [ ] Multiple URLs
- [ ] Muiltiple Payloads
- [ ] Attempt Delays
- [ ] Better logging