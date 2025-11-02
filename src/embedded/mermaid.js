var style = "default";
if (document.querySelector("#dark-style")) {
    style = "dark";
}

mermaid.initialize({
    theme: style,
    startOnLoad: true
});
