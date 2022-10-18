# RspFootball Model

This repository defines the interaction model between the RspFootball server and client.

The model is defined in a custom subset of XML, located at `model.xml`. Using `dotnet run`,
both a Pydantic version and a Typescript version of the model are generated and placed in
the `generated` directory. Consumers of this model can then copy the relevant model.


## Model language

The RspFootball model contains several different primitives, each with required XML attributes,
and expected children.

### Enum

`<Enum>` tags represent a set of literals. Enum should have a `name` attribute and `<member>` children.

### Result

A result in RspFootball is a representation of something that happened to get the game into its current state.
Each Result must have a `name` and a `tag` attribute. It should have `<member>` children.
Each `<member>` should have a `<name>` and a `<type>`.

