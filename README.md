# Tiger.Hal

## What It Is

Tiger.Hal is an ASP.NET Core library for defining declarative transformations from values to their HAL+JSON representation. It allows these transformed values to be served when a client includes the content type `application/hal+json` in a request's `Accepts` header. This library conforms to a conservative subset of the JSON Hypertext Application Language [draft specification][].

[draft specification]: https://tools.ietf.org/html/draft-kelly-json-hal-08

<!-- Because the specification isn't very good. -->

Limitations will be called out explicitly in this document.

## Why You Want It

The web is built on hyperlinks. RESTful APIs are also built on hyperlinks. XML RESTful APIs have a common, frequently-used way to express relations between resources: [XLink][]. HAL+JSON brings the ability to link resources (and the ability to _define_ the nature of those links) to JSON.

[XLink]: https://en.wikipedia.org/wiki/XLink

Given a response from an API that looks like this:

```json
{
  "id": "c06f1945-a68a-4a6f-a7a9-5c35a0966abc",
  "itemId": "23b73042-9dec-4587-a21f-4c4c25d8df3c",
  "composition": "vertical",
  "size": 42
}
```

…how can further details on the "item" associated with this resource be retrieved? If this response body came from sending a request to `https://example.invalid/printJobs/c06f1945-a68a-4a6f-a7a9-5c35a0966abc`, then a fair guess is `https://example.invalid/items/23b73042-9dec-4587-a21f-4c4c25d8df3c`.

Or possibly `https://another-team.example.invalid/items/23b73042-9dec-4587-a21f-4c4c25d8df3c`…

Or possibly this item does belong to Another Team, but in a different environment…

Or possibly literally anywhere else on the entire web. There's no way to reverse-engineer, guess, or construct a URI to a resource given only the resource's ID. At least, not without external, fragile information.

A response that looks like this would be much better:

```json
{
  "_links": {
    "self": { "href": "https://example.invalid/printJobs/c06f1945-a68a-4a6f-a7a9-5c35a0966abc" },
    "https://relations.example.invalid/item": {
      "href": "https://another-team.example.invalid/item/23b73042-9dec-4587-a21f-4c4c25d8df3c"
    }
  },
  "composition": "vertical",
  "size": 42
}
```

Huh, look at that. Another Team uses singular resource names in their URIs. But because the entire URI is available to be followed without manipulation, clients of their API don't need to know that. (In fact, Another Team uses singular resource names when accessing a single resource, but plural resource names when accessing an index. Tricky.)

When API responses contain link relations, following the chain of data becomes as easy as clicking links on a web page.

## Operations

In order to set up a HAL+JSON transformation, this library provides four operations. For developers familiar with the specification, some of these operations may be surprising.

- [`Link`][]
- [`Embed`][]
- [`Hoist`][]
- [`Ignore`][]

[`Link`]: https://github.com/Cimpress-MCP/Tiger-HAL/wiki/Link
[`Embed`]: https://github.com/Cimpress-MCP/Tiger-HAL/wiki/Embed
[`Hoist`]: https://github.com/Cimpress-MCP/Tiger-HAL/wiki/Hoist
[`Ignore`]: https://github.com/Cimpress-MCP/Tiger-HAL/wiki/Ignore

There are no operations that correspond to, represent, or manipulate the `curies` link relation. This functionality is not supported in this library. The specification is very confused when it comes to CURIEs and link relations, and it's better off pretending that they don't exist.

<!-- Specifically, the specification gets two things wrong with respect to link relations. First, link relations mostly cannot be barewords. The bareword link relations (such as "index", "next", and "prev") are limited in number and curated in a global repository. The specification uses a made-up bareword every time it wants to return an array from an endpoint. Second, CURIEs must, accoring to their spec, be surrounded by square brackets so that they are not confused with normal, non-compact links. They should look like this: `[fen:pool]`. Including these square brackets confused every client I tried this out with, so it's better to just leave the whole feature aside. -->

## Use

Once the profile for transformation has been created, the transformations can be made active in the `ConfigureServices` method of an ASP.NET Core application after a call to `AddMvc`.

```csharp
services.AddMvc().AddHalJson<HalProfile>();
```

## Going Further

This is only an overview of the operations available in Tiger.Hal. The public methods of the library are extensively annotated with documentation comments, so they should provide finer detail on each operation and it overloads.

## Thank You

Seriously, though. Thank you for using this software. The author hopes it performs admirably for you.
