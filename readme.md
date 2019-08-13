# BlazorStyled

_CSS in Blazor Components_

## Docs

View the detailed [docs](https://blazorstyled.io) at https://blazorstyled.io.

## Install

[![NuGet Pre Release](https://img.shields.io/nuget/vpre/BlazorStyled.svg)](https://www.nuget.org/packages/BlazorStyled/)

## Why BlazorStyled?

* Maintain your css inside your component instead of a seperate file
* Eliminate all collisions - no need to use !important
* No need to worry about depoying css files - great for libraries
* css are C# strings - use variables instead of solutions like sass

### Insperation

* [Emotion](https://emotion.sh/docs/introduction)
* [Glamorous](https://glamorous.rocks/)
* [Glamor](https://github.com/threepointone/glamor)

## Short Example

```
<Styled @bind-Classname="@hover">
    label: hover-example;
    padding: 32px;
    background-color: hotpink;
    font-size: 24px;
    border-radius: 4px;
</Styled>

<Styled Classname="@hover" PseudoClass="PseudoClasses.Hover">
    color: @color;
</Styled>

<div class="@hover">
    Hover to change color.
</div>

@code {
    private string hover;
    private string color = "white";
}
```

See more in the [docs](https://chanan.github.io/BlazorStyled/) at https://chanan.github.io/BlazorStyled/.
