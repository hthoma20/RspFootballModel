# RspFootball Model

This repository defines the interaction model between the RspFootball server and client.

The model is defined in a custom subset of XML, located at `model.xml`. Using `dotnet run`,
both a Pydantic version and a Typescript version of the model are generated and placed in
the `generated` directory. Consumers of this model can then copy the relevant model.


## Model language

The RspFootball model contains several different primitives, each with required XML attributes,
and expected children.

### Types
Several difference structures require a type to be specified. This can be one of:
 - A simple identifier, which should refer to a declared Enum by its name
 - `<int/>`
 - `<string/>`
 - `<list> {some type} <\list>`
 - `<optional> {some type} </optional>`

### Enum

`<Enum>` tags represent a set of literals. Enum should have a `name` attribute and `<member>` children.

### Struct

A struct is a named type with key-value pairs. Each `<Struct>` must have a `name` attribute.
It should have `<member>` children. Each `<member>` should have a `<name>` and a `<type>`.

### TaggedUnion

A union is a named type which holds one of several different Structs. Each `<TaggedUnion>` must have
a `name` and `tagKey` attribute. The tagKey will be added as a member in each child struct.
The children must be `<Struct>` objects, and each must have an attribute `tag` which represents
the tag value.
