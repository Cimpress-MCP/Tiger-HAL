### What's new in 7.0.0 (Released 2021-03-11)

* The library has grown support for more modern frameworks.
* The library has grown nullability annotations.
* Collections of types which already implement `ILinkData` can be mapped directly without a passthrough selector.

### What's new in 6.0.0 (Released 2019-07-04)

* The library has grown support for custom rules for producing links.
  * Standard rules go through this single entry point, so Tiger.HAL is implemented on the same base as your rules.
* The elements of collections can be embedded without linking to the collection itself.
  * Because this is the same results as the `EmbedElements` command on base collections, it has the same name.
* Debug Symbols and SourceLink are now published to NuGet.

### What's new in 5.0.0 (Released 2018-11-15)

* The library has grown special support for transformation maps of collection types.

### What's new in 4.0.0 (Released 2018-09-26)

* A companion library of analyzers has been created: Tiger.Hal.Analyzers.
* Properties may now be ignored automatically if they meet certain criteria.
* Dependencies have been greatly rationalized.
* Namespaces have been adjusted.
* More utility methods have been added.

### What's new in 3.0.0 (Released 2018-07-17)

* The library has been made ready for ASP.NET Core 2.1.

### What's new in 2.0.2 (Released 2018-03-19)

* ASP.NET Core compatibility has been widened.
* Self links created from parameterless routes have been made more convenient.

### What's new in 2.0.1 (Released 2017-01-16)

* `null` values intended to be used as links are ignored by the link creation.

### What's new in 2.0.0 (Released 2017-01-15)

* Quarantined due to a bug.

### What's new in 1.0.0 (Released 2016-11-27)

* Everything is new!
