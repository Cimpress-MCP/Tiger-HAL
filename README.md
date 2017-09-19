# Tiger.Hal

## What It Is

Tiger.Hal is an ASP.NET Core library for defining declarative transformations from values to their HAL+JSON representation. It allows these transfomed values to be served when a client includes the content type `application/hal+json` in a request's `Accepts` header. This library conforms to a convervative subset of the JSON Hypertext Application Language [draft specification][].

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
    "self": "https://example.invalid/printJobs/c06f1945-a68a-4a6f-a7a9-5c35a0966abc",
    "https://relations.example.invalid/item": {
      "href": "https://another-team.example.invalid/item/23b73042-9dec-4587-a21f-4c4c25d8df3c"
    }
  },
  "composition": "vertical",
  "size": 42
}
```

Huh, look at that. Another Team uses singular resource names in their URIs. But because the entire URI is available to be followed without manipulation, clients of their API don't need to know that. (Actually, Another Team uses singular resource names when accessing a single resource, but plural resource names when accessing an index. Tricky.)

When API responses contain link relations, following the chain of data becomes as easy as clicking links on a web page.

## Operations

In order to set up a HAL+JSON transformation, this library provides four operations. For developers familiar with the specification, some of these operations may be surprising.

- `Link`
- `Embed`
- `Hoist`
- `Ignore`

There are no operations that correspond to, represent, or manipulate the `curies` link relation. This functionality is not supported in this library. The specification is very confused when it comes to CURIEs and link relations, and it's better off pretending that they don't exist.

<!--
Specifically, the specification gets two things wrong with respect to link relations. First, link relations mostly cannot be barewords. The bareword link relations (such as "index", "next", and "prev") are limited in number and curated in a global repository. The specification uses a made-up bareword every time it wants to return an array from an endpoint. Second, CURIEs must, accoring to their spec, be surrounded by square brackets so that they are not confused with normal, non-compact links. They should look like this: `[fen:pool]`. Including these square brackets confused every client I tried this out with, so it's better to just leave the whole feature aside.
-->

Please refer to this sample document for examples for the remainder of the documentation:

```csharp
/// <summary>Defines HAL+JSON transformations for the application.</summary>
public sealed class PrintJobHalProfile
    : IHalProfile
{
    static readonly Uri _pool = new Uri("https://relations.fen.cimpress.io/pool", Absolute);
    static readonly Uri _watchers = new Uri("https://relations.fen.cimpress.io/watchers", Absolute);
    static readonly Uri _events = new Uri("https://relations.fen.cimpress.io/events", Absolute);
    static readonly Uri _items = new Uri("https://relations.fen.cimpress.io/items", Absolute);
    static readonly Uri _assets = new Uri("https://relations.fen.cimpress.io/assets", Absolute);

    /// <inheritdoc/>
    public void OnTransformationMapCreating(TransformationMap transformationMap)
    {
        transformationMap
            .Self<PrintJob>(pj => Route("GetByPrintJobId", new { pj.Id }))
            .Link(_pool, pj => Const(pj.Pool))
            .Link(_watchers, pj => pj.Watchers, w => new Constant(w.Uri) { Name = w.Name })
            .Link(_events, pj => Route("GetEventsForPrintJob", new { pj.Id }))
            .Embed(_items, pj => pj.Items, pj => Route("GetItemsForPrintJob", new { pj.Id }))
            .Link(_assets, pj => pj.Assets, a => new Constant(a.Uri) { Title = a.Role })
            .Ignore(pj => pj.Id, pj => pj.Pool, pj => pj.Watchers, pj => pj.Events, pj => pj.Assets);

        transformationMap
            .Self<PrintJobEventCollection>(pjec => Route("GetEventsForPrintJob", new { id = pjec.PrintJobId }))
            .Link("up", pjec => Route("GetByPrintJobId", new { id = pjec.PrintJobId }))
            .Hoist(pjec => pjec.Count);

        transformationMap
            .Self<PrintJobEvent>(e => Route("GetEvent", new { e.Id, e.PrintJobId }))
            .Link("index", e => Route("GetEventsForPrintJob", new { id = e.PrintJobId }))
            .Link("up", e => Route("GetByPrintJobId", new { id = e.PrintJobId }))
            .Ignore(pj => pj.Id, pj => pj.PrintJobId);
    }
}
```

### Link

The most important and most basic operation is `Link`. This operation creates a "link relation" between the current resource and another resource. The relation is defined by the value of the `rel` key, and the ID of the other resource is represented by the value of the `href` key. This functions identically to the HTML 5 elements `<a rel="" href="">` and `<link rel="" href="">`, as well as a lot of other web linked data formats.

There are three ways to instruct this library on how to generate a link from a value, and each way has a simple method and a complex method.

The first is the method `Const` and the type `Constant`. If a property on the value to be transformed is a URI or represents a URI, it can be passed directly to the method `Const`. This will directly produce a link relation with the specified relation and with `href` set to the specified value. This can be seen on line 16 of the example.

```csharp
.Link(_pool, pj => Const(pj.Pool))
```

If other link properties are required, fall back to calling the constructor for `Constant` and set the desired properties directly, as seen on line 17 of the example.

```csharp
.Link(_watchers, pj => pj.Watchers, w => new Constant(w.Uri) { Name = w.Name })
```

Second is the method `Route` and the type `Routed`. This will cause the link to a resource to be generated by ASP.NET Core. The arguments to the `Route` method are identical to those of the method `IUrlHelper.Link(string, object)`. <small>(Which is usually available on the property `Url` in controller classes.)</small>
This can be seen on line 18 of the example.

```csharp
.Link(_events, pj => Route("GetEventsForPrintJob", new { pj.Id }))
```

As before, if other link properties are required, fall back to calling the constructor for `Routed` and set the desired properties directly.

Third is the method `Template` and the type `Templated`. This will create a templated link in the HAL+JSON serialization, which is a link with values to be filled in by the client in order to produce a followable link. This is not a common operation, but is most frequently used at the top level of an API. If a client only has access to a bare ID value – for example, from a search field on a website – it can seek a templated link with a known `rel` value and fill the value in. URL templates are defined to conform to [RFC 6570][].

[RFC 6570]: https://tools.ietf.org/html/rfc6570

As before, if other link properties are required, fall back to calling the constructor for `Templated` and set the desired properties directly. Keep in mind, though, that the value for the key `templated` is automatically set to `true` by selecting `Template` or `Templated`.

#### Self

A special kind of link operation is the creation of a value's `self` link. No other configuration of transformation can be done until a type's `self` link generation is created. The link relation `self` is thus always present and always a singular value.

Note that on line 12 of the example, the transformation map is passed as a parameter to the method.

```csharp
public void OnTransformationMapCreating(TransformationMap transformationMap)
```

There are no methods on the type `TransformationMap` other than `Self`. All further transformations are available on the type that `Self` returns. In this library, a `self` link is non-optional.

#### Linking Collections

`Link` has overloads with special handling for types implementing `IEnumerable<T>` or `IDictionary<TKey, TValue>`. Line 17 of the example shows this in use.

```csharp
.Link(_watchers, pj => pj.Watchers, w => new Constant(w.Uri) { Name = w.Name })
```

For this API, the value associated with the link relation `https://relations.fen.cimpress.io/watchers` should be a collection of links. This is different from line 18, which creates only one link.

```csharp
.Link(_events, pj => Route("GetEventsForPrintJob", new { pj.Id }))
```

In short, one way creates a link to a collection:

```json
{
  "_links": {
    "https://relations.fen.cimpress.io/events": {
      "href": "https://example.invalid/printJobs/4b1bea5e-13c0-41e6-9b05-a70587b47011/events"
    }
  }
}
```

…and one way creates a collection of links.

```json
{
  "_links": {
    "https://relations.fen.cimpress.io/watchers": [
      { "href": "https://example.invalid/watchers/44" },
      { "href": "https://example.invalid/watchers/55" },
      { "href": "https://example.invalid/watchers/66" }
    ]
  }
}
```

### Embed

The operation `Embed` enables what HAL+JSON calls the "hypertext cache pattern". A criticism of linked data schemes for APIs is that creating links for data that once was or could have been in the main body of the response causes the number of HTTP requests to explode. The "\_embedded" key seeks to overcome this.

Given this modified example from the specification:

```json
{
  "_links": {
    "self": { "href": "https://example.invalid/books/the-way-of-zen" },
    "author": { "href": "https://example.invalid/people/alan-watts" }
  }
}
```

We can see that retrieving the book resource (presumably with the title <cite>The Way of Zen</cite>), when represented like this, requires an additional HTTP request in order to retrieve the author (presumably named Alan Watts). This is not outright bad, given that a person is an independently addressable resource. Embedding the person resource looks like this:

```json
{
  "_links": {
    "self": { "href": "https://example.invalid/books/the-way-of-zen" },
    "author": { "href": "https://example.invalid/people/alan-watts" }
  },
  "_embedded": {
    "author": {
      "_links": { "self": { "href": "https://example.invalid/people/alan-watts" } },
      "name": "Alan Watts",
      "born": "January 6, 1915",
      "died": "November 16, 1973"
    }
  }
}
```

This use can be seen on line 18 of the example.

```csharp
.Embed(_items, pj => pj.Items, pj => Route("GetItemsForPrintJob", new { pj.Id }))
```

Embedding should be used judiciously, as one advantage of using linked data is that response bodies become significantly smaller once all pertinent data are linked. If every individually addressable sub-value on a value is moved to being embedded, the response body will become even larger, due to the overhead of links and recursive embeds.

When embedding, consider a holistic view of the data owned by the API in question. For example, if the "people" resource in the example above embedded all of the books written by that person, the list of books would contain <cite>The Way of Zen</cite>, the "\_embedded" section of which would include Alan Watts! In fact, every book would embed the same resource, wasting bytes and transmission time.

In short, link plenty and embed carefully.

#### Arrays

For developers unfamiliar with the HAL+JSON specification, its handling of arrays is likely to be surprising. Here is a modified example from the specification.

```json
{
  "_links": {
    "self": { "href": "https://examples.invalid/orders" },
    "next": { "href": "https://examples.invalid/orders?page=2" },
    "find": { "href": "https://examples.invalid/orders{/id}", "templated": true }
  },
  "_embedded": {
    "self": [
      {
        "_links": {
          "self": { "href": "https://examples.invalid/orders/123" },
          "basket": { "href": "https://examples.invalid/baskets/98712" },
          "customer": { "href": "https://examples.invalid/customers/7809" }
        },
        "total": 30.00,
        "currency": "USD",
        "status": "shipped",
      },
      {
        "_links": {
          "self": { "href": "https://examples.invalid/orders/124" },
          "basket": { "href": "https://examples.invalid/baskets/97213" },
          "customer": { "href": "https://examples.invalid/customers/12369" }
        },
        "total": 20.00,
        "currency": "USD",
        "status": "processing"
      }
    ]
  },
  "currentlyProcessing": 14,
  "shippedToday": 20
}
```

Because a HAL+JSON response hangs on the keys "\_links" and "\_embedded", the top-level value cannot be an array. <small>(Where would the keys and values go?)</small> For that reason, arrays are considered to be embedding themselves. In this library, this embedding has handled automatically, and arrays are always embedded under the "self" link relation.

### Hoist

Related to the self-embedding nature of arrays is the `Hoist` operation. Calculated properties of values that serialize to arrays can be attached to the parent object (the object with "\_links" and "\_embedded" keys). This is most commonly used for a "count" key, the value of which is the number of member of the array. This is exacly what line 26 of the example does.

```csharp
.Hoist(pjec => pjec.Count);
```

Further properties can relieve client burden when applied usefully. For example, a list of orders could hoist two keys: "count" and "shippedCount", representing the number of orders and the number of orders with status "shipped", respectively. For a client to determine whether all applicable orders have shipped, they can compare these two values for equality without having to retrieve the array, deserialize it, and loop over its elements.

The `Hoist` operation is only meaningful on types that JSON.NET will resolve to a `JsonArrayContract` – types that serialize to arrays in JSON. Types that serialize to objects already have their keys at the top level of themselves.

### Ignore

If a value is used for link creation – as `pj.Id` is on line 15 of the example – it's unlikely to be useful to a client in the body of the value.

```csharp
.Self<PrintJob>(pj => Route("GetByPrintJobId", new { pj.Id }))
```

Once a client has requested `application/hal+json`, they should no longer have a need to glue together `baseURI`s and `ID`s (or whatever) in order to synthesize a URI to a resource. The method `Ignore` will remove the selected property (key and value) from the serialization. Many properties can be specified in a single call to `Ignore`, as on line 21 of the example.

```csharp
.Ignore(pj => pj.Id, pj => pj.Pool, pj => pj.Watchers, pj => pj.Events, pj => pj.Assets);
```

In short, this is similar to JSON.NET's `JsonIgnoreAttribute`, but specific to the HAL+JSON serialization without coupling the data type to its serialized representations.

## Going Further

This is only an overview of the operations available in Tiger.Hal. The public methods of the library are extensively annotated with documentation comments, so they should provide finer detail on each operation and it overloads.

## Thank You

Seriously, though. Thank you for using this software. The author hopes it performs admirably for you.
