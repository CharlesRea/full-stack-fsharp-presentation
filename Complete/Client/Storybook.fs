module rec Storybook

open Fable.Core
open Fable.React

(*
Typings for JS calls of the form:

import { storiesof } from @storybook/react;

storiesOf('Some component examples, module)
  .add('A story', () => <SomeReactComponent foo="bar" />);
*)

type RenderFunction = unit -> ReactElement

type [<AllowNullLiteral>] Story =
    abstract add : storyName:string * render: RenderFunction -> Story

/// Access a reference to the Webpack 'module' global variable
let [<Emit("module")>] webpackModule<'T> : 'T = jsNative

[<Import("storiesOf", from="@storybook/react")>]
let storiesOf(name: string, ``module``: obj): Story = jsNative
