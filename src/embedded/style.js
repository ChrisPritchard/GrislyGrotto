
document.querySelectorAll(".style_selector").forEach(item =>
    item.addEventListener("click", e => {
        let style = e.target.getAttribute("data-style");
        fetch('/style/' + style).then(_ => window.location.reload());
    }));
