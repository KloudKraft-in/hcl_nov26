# Test payloads

- `valid_order.json`: valid message body for an end-to-end processing test.
- `invalid_order.json`: syntactically valid JSON that fails the Function's business validation and should be retried before being moved to `orders-poison`.

The Web App sends JSON using Base64 message encoding through `Azure.Storage.Queues`. If inserting a message manually for the poison-message exercise, encode the JSON as Base64 first because the Function queue extension is configured with `messageEncoding: base64`.
