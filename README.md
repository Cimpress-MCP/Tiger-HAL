# Tiger.Hal

## What It Is

Tiger.Hal is an ASP.NET Core library for defining declarative transformations from values to their HAL+JSON representation. It allows these transfomed value to be served when a client includes the content type `application/hal+json` in a request's `Accepts` header. This library conforms to a convervative subset of the JSON Hypertext Application Language [draft specification][].

<!-- Because the specification isn't very good. -->

Limitations will be called out explicitly in this document.

[draft specification]: https://tools.ietf.org/html/draft-kelly-json-hal-08

## Why You Want It

## Operations

In order to set up a HAL+JSON transformation, this library provides four operations. If you are familiar with the specification, the operations may be surprising.

- `Link`
- `Embed`
- `Hoist`
- `Ignore`

There are no operations that correspond to, represent, or manipulate the "curies" link. This functionality is not supported in this library. The specification is very confused when it comes to CURIEs and link relations, and it's better off pretending that they don't exist.

<!--
Specifically, the specification gets two things wrong with respect to link relations. First, link relations mostly cannot be barewords. The bareword link relations (such as "index", "next", and "prev") are limited in number and curated in a global repository. The specification uses a made-up bareword every time it wants to return an array from an endpoint. Second, CURIEs must, accoring to their spec, be surrounded by square brackets so that they are not confused with normal, non-compact links. They should look like this: `[fen:pool]`. Including these square brackets confused every client I tried this out with, so it's better to just leave the whole feature aside.
-->

Please refer to this sample document for examples for the remainder of the doumentation:

```csharp
/// <summary>Defines HAL+JSON transformations for the application.</summary>
public sealed class PrintJobHalProfile
    : IHalProfile
{
    const string Pool = "https://relations.fen.cimpress.io/pool";
    const string Watchers = "https://relations.fen.cimpress.io/watchers";
    const string Events = "https://relations.fen.cimpress.io/events";
    const string Items = "https://relations.fen.cimpress.io/items";
    const string Assets = "https://relations.fen.cimpress.io/assets";

    /// <inheritdoc/>
    public void OnTransformationMapCreating(TransformationMap transformationMap)
    {
        transformationMap
            .Self<PrintJob>(pj => Route("GetByPrintJobId", new { pj.Id }))
            .Link(Pool, pj => Const(pj.Pool))
            .Link(Watchers, pj => pj.Watchers, w => new Constant(w.Uri) { Name = w.Name })
            .Link(Events, pj => Route("GetEventsForPrintJob", new { pj.Id }))
            .Embed(Items, pj => pj.Items, pj => Route("GetItemsForPrintJob", new { pj.Id }))
            .Link(Assets, pj => pj.Assets, a => new Constant(a.Uri) { Title = a.Role })
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

The `Link` method has a third, optional parameter called `retain`. This is `false` by default, so referenced properties are removed from the HAL+JSON serialization by default. See the section on `Ignore` for more details.

#### Self

One special kind of link operation is the creation of a value's `self` link. No other configuration of transformation can be done until a type's "self" link generation is created. The link relation "self" is thus always present and always a singular value.

Note that on line 12 of the example, the transformation map is passed as a parameter to the method. There are no methods on the type `TransformationMap` other than `Self`. All further transformations are available on the type that `Self` returns. In this library, a "self" link is non-optional.

#### LinkMany

`LinkMany` is `Link` with special handling for types implementing `IEnumerable<T>` or `IDictionary<TKey, TValue>`. Line 17 of the example shows this in use. For this API, the value associated with the link relation `https://relations.fen.cimpress.io/watchers` should be a collection of links. This is different from line 18, which creates only one link. In short, `Link` creates a link to a collection; `LinkMany` creates a collection of links.

```json
{
  "https://relations.fen.cimpress.io/watchers": [
    { "href": "https://example.invalid/watchers/44" },
    { "href": "https://example.invalid/watchers/55" },
    { "href": "https://example.invalid/watchers/66" }
  ]
}
```

```json
{
  "https://relations.fen.cimpress.io/events": {
    "href": "https://example.invalid/printJobs/4b1bea5e-13c0-41e6-9b05-a70587b47011/events"
  }
}
```

It should be noted that line 18 could have been written like this:

```csharp
.Link(Events, pj => pj.Events, pjec => Route("GetEventsForPrintJob", new { pjec.PrintJobId }))
```

…because `PrintJobEventCollection` happens to have the property `PrintJobId`, but it is written this way for the purposes of the example. If an "index" link is not supported (say, only every individual child item is addressable), then the collection is unlikely to keep track of its hierarchical parent, and the form from the example should be used.

### Embed

### Hoist

### Ignore

If a value is used for link creation -- as `pj.Watchers` is on line 18 -- then it's unlikely to be useful to a client in the body of the value. Once a client has requested `application/hal+json`, they should no longer have a need to glue together `baseURI`s and `ID`s in order to synthesize a URI to a resource. The method `Ignore` will remove the selected property (key and value) from the serialization.

However, explicitly calling `Ignore` is not required for every property. `Link` and `LinkMany` will, by default, remove the selected property from the serialization -- they call `Ignore` for you under the hood. This can be inhibited by passing the value `true` as the parameter `retain`.