// generated by go generate and embedStatic.go; DO NOT EDIT
// 2020-06-25T17:47:07+12:00
package main

func loadStatics() {
	embeddedStatics["favicon.png"] = `iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAMAUExURQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAALMw9IgAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjEuNWRHWFIAAAATSURBVDhPYxgFo2AUjAKsgIEBAAQgAAFC1XO+AAAAAElFTkSuQmCC`
	embeddedStatics["site.css"] = `body {
    font-family: 'Open Sans','Helvetica Neue',Helvetica,Arial,sans-serif;
    line-height: 1.3;
    background-color: black;
    padding: 0;
    margin: 0;
}


canvas.background-animation {
    position: fixed; 
    top: 0;
    left: 0;
    z-index:-1;
}

.site-container {
    margin-top: 0;
    margin-left: auto;
    margin-right: auto;
    max-width: 1000px;
    padding: 20px;

    background-color: #1e2125;
    color: #fff;
}

@media only screen and (max-width: 1000px) {
    body {
        margin-left: 25px;
        margin-right: 25px;
    }
}

nav {
    margin-left: 5px;
}

label {
    display: block;
    font-weight: 700;
    margin-bottom: 5px;
}

a, a:visited {
    color: #9acc14;
    font-weight: 700;
    text-decoration: none;
}

button, input, select, textarea {
    font-family: inherit;
    font-size: inherit;
    line-height: inherit;
    background-color: inherit;
    color: inherit;
    padding: 6px 12px;
}

input[type=text], input[type=password], textarea {
    display: block;
    border: 1px solid #9acc14;
    border-radius: 4px;
    background-color: #292e33;
    box-shadow: inset 0 1px 1px rgba(0,0,0,.075);
    transition: border-color .15s ease-in-out,box-shadow .15s ease-in-out;
    margin-bottom: 8px;
}

input[type=text]:focus, input[type=password]:focus, textarea:focus {
    border-color:#66afe9;
    outline:0;
    box-shadow:inset 0 1px 1px rgba(0,0,0,.075),0 0 8px rgba(102,175,233,.6)
}

input[type=submit], input[type=button] {
    transition: all .3s ease;
    font-weight: 400;
    text-align: center;
    cursor: pointer;
    background-color: inherit;
    border: 1px solid #9acc14;
    border-radius: 4px;
    padding: 6px 12px;
    font-size: 14px;
    line-height: 1.6em;
    user-select: none;
}

input[type=submit]:hover, input[type=button]:hover {
    background-color: rgba(154,204,20,.1);
}

footer {
    clear: both;
    text-align: center;
}

.site-title {
    margin-top: 0;
    margin-bottom: 0;
    font-size: 2.5em;
    text-transform: uppercase;
}

.post-title {
    margin-bottom: 0;
    font-size: 1.5em;
    text-transform: uppercase;
}

.post-summary {
    font-weight: 700;
    margin-bottom: 0.5em;
}

.post-content {
    text-align: justify;
}

.prev-link {
    margin-top: 1em;
    float: left;
}

.next-link {
    margin-top: 1em;
    float: right;
}

ul.years {
    list-style: none;
    padding: 0;
}

ul.years > li {
    float: left;
    margin-bottom: 2em;
}

ul.years > li > h3 {
    margin-bottom: 0;
}

ul.months {
    list-style: none;
    padding: 0;
}

ul.months li {
    margin-right: 2em;
}

div.stories-container {
    float: right;
    max-width: 500px;
}

ul.stories {
    list-style: none;
    padding: 0;
}

ul.stories h3 {
    margin-bottom: 0;
}

div.page-form {
    min-height: 1000px;
}

div.search-result h4 {
    margin-bottom: 0;
    text-transform: uppercase;
}

div.post-controls {
    margin-top: 20px;
    margin-bottom: 20px;
}

div.post-controls form {
    display: inline;
    margin-right: 10px;
}

.comment-summary {
    font-size: 0.8em;
    margin-top: 15px;
    margin-bottom: 4px;
}

.comment-delete input {
    padding: 2px;
    font-size: 0.8em;
    margin-top: 5px;
    margin-bottom: 5px;
}

.create-comment-container {
    margin-top: 20px;
}

div.post-controls form input {
    width: 65px;
}

div.checkbox {
    margin-bottom: 6px;
}

div.checkbox input, div.checkbox label {
    display: inline;
    vertical-align: center;
}

div.editor-help {
    float: right;
    width: 80%;
}

div.post-submit {
    clear: both;
    margin-top: 20px;
    margin-bottom: 20px;
}

span.timer {
    opacity: 50%;
    margin-top: -20px;
    position: absolute;
}

.error {
    color: red;
}

.vis-colours {
    display: none;
}

.control-panel {
    position: fixed;
    top: 10px;
    right: 10px;
}

.control-panel .controls {
    position: relative;
    float: right;
    font-size: 0.6em;
    opacity: 40%;
    text-align: right;
}

.control-panel .controls input, .control-panel .controls select {
    vertical-align: middle;
    height: 2em;
    font-size: 1em;
    padding: 0 5px 0 5px;
    margin-bottom: 5px;
    margin-top: 0;
}

@media only screen and (max-width: 1000px) {
    .control-panel {
        position: relative;
        margin-left: 14px;
    }
    
    .control-panel .controls {
        color: white;
        font-size: 0.8em;
        float: none;
        text-align: left;
        opacity: 100%;
    }
    
    .control-panel .controls input, .control-panel .controls select {
        vertical-align: middle;
    }
}`
	embeddedStatics["site.js"] = `// background animation setup

// - ensure that the canvas always fills the window

function resizeCanvas() {
    let canvas = document.getElementById('background-animation');
    canvas.width = (window.innerWidth-1000)/2;
    canvas.height = window.innerHeight;
};
resizeCanvas();
window.onresize = function () { resizeCanvas(); };

// - trigger the animation

function getColour(elem) {
    return window.getComputedStyle(elem, null).getPropertyValue("background-color");
}

animationCanvas = document.getElementById('background-animation');

wanderingTriangles.settings.entityCount = 50;
wanderingTriangles.init(
    animationCanvas, 
    getColour(document.getElementById('vis-background-colour')), 
    getColour(document.getElementById('vis-primary-colour')), 
    getColour(document.getElementById('vis-secondary-colour')));

// visualisation control panel

document.getElementById('vis-enabled').onchange = function() {
    wanderingTriangles.enabled = this.checked;
}
document.getElementById('site-theme').onchange = function() {
    document.getElementById('current-theme').value = this.value;
}
document.getElementById('site-theme').value = document.getElementById('current-theme').value;

// Confirm elements, e.g. deleting a comment

confirmElems = document.getElementsByClassName("confirm-click");
for (var i = 0; i < confirmElems.length; i++) {
    confirmElems[i].addEventListener("click", function () {
        return confirm("Are you sure? This action is irreversible!");
    });
}

// Dirty flag and confirm leave on editor page, to stop 'accidents'

let title = document.getElementById('title');
let content = document.getElementById('content');
if (title && content) { // both these being present means the user is on the editor page
    let dirty = false
    title.onchange = function () { dirty = true; }
    content.onchange = function () { dirty = true; }
    document.getElementById('submit').onclick = function () {
        dirty = false;
    }
    window.onbeforeunload = function () {
        if (dirty) {
            // note: most browsers ignore this and just detect that I return anything at all
            // if I do (e.g. the below 'confirm' object), then they present their own version of the below
            // I could return 1 here, or true, or even false, and it would trigger a prompt
            return confirm('are you sure you want to leave?')
        }
        // by returning nothing here, there is no prompt. note that returning false here WOULD trigger a prompt
    }
}

// cookie expiry countdown

if (title && content) {

    function updateTimer(timeRemaining) {
        minutes = Math.floor(timeRemaining / 60);
        labelClass = "timer";
        if (minutes < 10) {
            labelClass += " error";
        }
        seconds = Math.floor(timeRemaining % 60);

        let timer = document.getElementById('timer');
        if (!timer) {
            timer = document.createElement('span');
            timer.id = 'timer';
            document.querySelector('h2').insertAdjacentElement('afterend', timer);
        }
        if (minutes > 0) {
            plural = minutes == 1 ? "" : "s";
            timer.innerText = minutes+" minute"+plural+" remaining";
        } else {
            plural = seconds == 1 ? "" : "s";
            timer.innerText = seconds+" second"+plural+" remaining";
        }
        timer.className = labelClass;
    }

    start = new Date().getTime ();
    cookieTime = 60*60*1000;
    interval = setInterval(function () {
        timeRemaining = (cookieTime-((new Date().getTime())-start))/1000;
        if (timeRemaining <= 0) {
            alert('Your auth cookie has now expired, and you can no longer post this blog.\nPlease copy your content to your clipboard and re-login.');
            document.getElementById('timer').innerText = 'Cookie has expired';
            document.getElementById('submit').style.display = 'none';
            clearInterval(interval);
        } else {
            updateTimer(timeRemaining);
        }
    }, 1000);
}`
	embeddedStatics["theme-gold-black.css"] = `body {
    background-color: black;
}

.site-container {
    background-color: #1e2125;
    color: #fff;
}

a, a:visited {
    color: gold;
}

input[type=text], input[type=password], textarea {
    border: 1px solid gold;
    background-color: #292e33;
    box-shadow: inset 0 1px 1px rgba(0,0,0,.075);
}

input[type=text]:focus, input[type=password]:focus, textarea:focus {
    border-color:#66afe9;
    box-shadow:inset 0 1px 1px rgba(0,0,0,.075),0 0 8px rgba(102,175,233,.6)
}

input[type=submit], input[type=button] {
    border: 1px solid gold;
}

input[type=submit]:hover, input[type=button]:hover {
    background-color: rgba(154,204,20,.1);
}

#vis-background-colour {
    background-color: black;
}

#vis-primary-colour {
    background-color: gold;
}

#vis-secondary-colour {
    background-color: gray;
}`
	embeddedStatics["theme-green-black.css"] = `body {
    background-color: black;
}

.site-container {
    background-color: #1e2125;
    color: #fff;
}

a, a:visited {
    color: #9acc14;
}

input[type=text], input[type=password], textarea {
    border: 1px solid #9acc14;
    background-color: #292e33;
    box-shadow: inset 0 1px 1px rgba(0,0,0,.075);
}

input[type=text]:focus, input[type=password]:focus, textarea:focus {
    border-color:#66afe9;
    box-shadow:inset 0 1px 1px rgba(0,0,0,.075),0 0 8px rgba(102,175,233,.6)
}

input[type=submit], input[type=button] {
    border: 1px solid #9acc14;
}

input[type=submit]:hover, input[type=button]:hover {
    background-color: rgba(154,204,20,.1);
}

#vis-background-colour {
    background-color: black;
}

#vis-primary-colour {
    background-color: #9acc14;
}

#vis-secondary-colour {
    background-color: gray;
}`
	embeddedStatics["theme-grey-white.css"] = `body {
    background-color: white;
}

.site-container {
    background-color: whitesmoke;
    color: black;
}

a, a:visited {
    color: gray;
}

input[type=text], input[type=password], textarea {
    border: 1px solid black;
    background-color: gray;
    color: white;
    box-shadow: inset 0 1px 1px rgba(0,0,0,.075);
}

input[type=text]:focus, input[type=password]:focus, textarea:focus {
    border-color:#66afe9;
    box-shadow:inset 0 1px 1px rgba(0,0,0,.075),0 0 8px rgba(102,175,233,.6)
}

input[type=submit], input[type=button] {
    border: 1px solid gray;
}

input[type=submit]:hover, input[type=button]:hover {
    background-color: rgba(154,204,20,.1);
}

.control-panel {
    color: black;
}

@media only screen and (max-width: 1000px) {
    .control-panel .controls {
        color: black;
    }
}

#vis-background-colour {
    background-color: white;
}

#vis-primary-colour {
    background-color: whitesmoke;
}

#vis-secondary-colour {
    background-color: gray;
}`
	embeddedStatics["theme-purple-black.css"] = `body {
    background-color: black;
}

.site-container {
    background-color: #1e2125;
    color: #fff;
}

a, a:visited {
    color: SlateBlue;
}

input[type=text], input[type=password], textarea {
    border: 1px solid SlateBlue;
    background-color: #292e33;
    box-shadow: inset 0 1px 1px rgba(0,0,0,.075);
}

input[type=text]:focus, input[type=password]:focus, textarea:focus {
    border-color:#66afe9;
    box-shadow:inset 0 1px 1px rgba(0,0,0,.075),0 0 8px rgba(102,175,233,.6)
}

input[type=submit], input[type=button] {
    border: 1px solid SlateBlue;
}

input[type=submit]:hover, input[type=button]:hover {
    background-color: rgba(154,204,20,.1);
}

#vis-background-colour {
    background-color: black;
}

#vis-primary-colour {
    background-color: SlateBlue;
}

#vis-secondary-colour {
    background-color: gray;
}`
	embeddedStatics["theme-red-white.css"] = `body {
    background-color: white;
}

.site-container {
    background-color: whitesmoke;
    color: black;
}

a, a:visited {
    color: maroon;
}

input[type=text], input[type=password], textarea {
    border: 1px solid black;
    background-color: maroon;
    color: white;
    box-shadow: inset 0 1px 1px rgba(0,0,0,.075);
}

input[type=text]:focus, input[type=password]:focus, textarea:focus {
    border-color:#66afe9;
    box-shadow:inset 0 1px 1px rgba(0,0,0,.075),0 0 8px rgba(102,175,233,.6)
}

input[type=submit], input[type=button] {
    border: 1px solid maroon;
}

input[type=submit]:hover, input[type=button]:hover {
    background-color: rgba(154,204,20,.1);
}

.control-panel {
    color: black;
}

@media only screen and (max-width: 1000px) {
    .control-panel .controls {
        color: black;
    }
}

#vis-background-colour {
    background-color: white;
}

#vis-primary-colour {
    background-color: red;
}

#vis-secondary-colour {
    background-color: maroon;
}`
	embeddedStatics["theme-white-black.css"] = `body {
    background-color: black;
}

.site-container {
    background-color: #1e2125;
    color: #fff;
}

a, a:visited {
    color: white;
}

input[type=text], input[type=password], textarea {
    border: 1px solid white;
    background-color: #292e33;
    box-shadow: inset 0 1px 1px rgba(0,0,0,.075);
}

input[type=text]:focus, input[type=password]:focus, textarea:focus {
    border-color:#66afe9;
    box-shadow:inset 0 1px 1px rgba(0,0,0,.075),0 0 8px rgba(102,175,233,.6)
}

input[type=submit], input[type=button] {
    border: 1px solid white;
}

input[type=submit]:hover, input[type=button]:hover {
    background-color: rgba(154,204,20,.1);
}

#vis-background-colour {
    background-color: black;
}

#vis-primary-colour {
    background-color: white;
}

#vis-secondary-colour {
    background-color: #666;
}`
	embeddedStatics["wandering-triangles.js"] = `let wanderingTriangles = {}
wanderingTriangles.settings = {
    fadeAlpha: 0.1,
    framerate: 18,
    entityCount: 20,
    triangleSize: 15,
    chanceOfJump: 0.005,
    chanceOfSecondaryColour: 0.25,
    chanceOfFill: 0.5
};
// contains the current x/y/colour/shape of all triangles (can be serialised/deserialised easily)
wanderingTriangles.state = [];
// toggle this to false to stop drawning triangles (a 'pause', but with fading)
wanderingTriangles.enabled = true;
// used to track the internal draw loop
wanderingTriangles.interval = 0;

wanderingTriangles.init = function(canvas, backgroundColour, primaryColour, secondaryColour) {
    this.context = canvas.getContext("2d");
    this.settings.backgroundColour = backgroundColour;
    this.settings.primaryColour = primaryColour;
    this.settings.secondaryColour = secondaryColour;

    if (this.state.length === 0) {
        for (var i = 0; i < this.settings.entityCount; i++) {
            this.state.push({
                x: Math.random() * canvas.width,
                y: Math.random() * canvas.height,
                type: Math.floor(Math.random() * 4),
                colour: this.settings.primaryColour
            })
        }
    }

    var interval = 1000 / this.settings.framerate;
    clearInterval(this.interval);
    var self = this;
    this.interval = setInterval(function () { 
        self.draw()
    }, interval);
};

wanderingTriangles.draw = function() {
    // an alpha overdraw 'fades out' the triangles
    this.context.fillStyle = this.settings.backgroundColour;
    this.context.globalAlpha = this.settings.fadeAlpha;
    this.context.fillRect(0, 0, this.context.canvas.width, this.context.canvas.height);
    this.context.globalAlpha = 1;

    // only refresh/draw-new triangles if the animation is 'enabled'. this is how pausing works
    if (this.enabled) {
        for (var i = 0; i < this.state.length; i++) {
            this.state[i] = this.updateTriangle(this.state[i])
            this.drawTriangle(this.state[i])
        }
    }
};

wanderingTriangles.updateTriangle = function(triangle) {
    if (triangle.type === 0)
        triangle = this.nextTriangleFromUp(triangle);
    else if (triangle.type === 1)
        triangle = this.nextTriangleFromDown(triangle);
    else if (triangle.type === 2)
        triangle = this.nextTriangleFromLeft(triangle);
    else
        triangle = this.nextTriangleFromRight(triangle);

    let offscreen = 
        triangle.x < -this.settings.triangleSize ||
        triangle.y < -this.settings.triangleSize ||
        triangle.x > this.context.canvas.width + this.settings.triangleSize ||
        triangle.y > this.context.canvas.height + this.settings.triangleSize
    if (Math.random() > this.settings.chanceOfJump && !offscreen) 
        return triangle;
        
    triangle.x = Math.random() * this.context.canvas.width;
    triangle.y = Math.random() * this.context.canvas.height;
    triangle.type = Math.floor(Math.random() * 4);

    if (Math.random() < this.settings.chanceOfSecondaryColour)
        triangle.colour = this.settings.secondaryColour;
    else
        triangle.colour = this.settings.primaryColour;

    return triangle;
};

wanderingTriangles.drawTriangle = function(triangle) {
    this.context.fillStyle = this.context.strokeStyle = triangle.colour;
    this.context.beginPath();
    
    if (triangle.type === 0)
        this.upTrianglePath(triangle.x, triangle.y);
    else if (triangle.type === 1)
        this.downTrianglePath(triangle.x, triangle.y);
    else if (triangle.type === 2)
        this.leftTrianglePath(triangle.x, triangle.y);
    else
        this.rightTrianglePath(triangle.x, triangle.y);

    this.context.closePath();
    if (Math.random() > this.settings.chanceOfFill)
        this.context.stroke();
    else
        this.context.fill();
};

wanderingTriangles.nextTriangleFromUp = function(triangle) {
    var random = Math.random();
    if (random < 0.2)
        triangle.type = 3; // right
    else if (random < 0.4)
        triangle.type = 2; // left
    else {
        triangle.y += this.settings.triangleSize * 2;
        triangle.type = 1; // down
    }
    return triangle;
};

wanderingTriangles.nextTriangleFromDown = function(triangle) {
    var random = Math.random();
    if (random < 0.2) {
        triangle.y -= this.settings.triangleSize * 2;
        triangle.type = 0; // up
    }
    else if (random < 0.6)
        triangle.type = 2; // left
    else
        triangle.type = 3; // right
    return triangle;
};
           
wanderingTriangles.nextTriangleFromLeft = function(triangle) {
    var random = Math.random();
    if (random < 0.1) {
        triangle.x += this.settings.triangleSize;
        triangle.y -= this.settings.triangleSize;
        triangle.type = 3; // right
    }
    else if (random < 0.4) {
        triangle.x += this.settings.triangleSize;
        triangle.y += this.settings.triangleSize;
        triangle.type = 3; // right
    }
    else if (random < 0.7) {
        triangle.x += this.settings.triangleSize * 2;
        triangle.type = 3; // right
    }
    else
        triangle.type = 0; // up
    return triangle;
};

wanderingTriangles.nextTriangleFromRight = function(triangle) {
    var random = Math.random();
    if (random < 0.1) {
        triangle.x -= this.settings.triangleSize;
        triangle.y -= this.settings.triangleSize;
        triangle.type = 2; // left
    }
    else if (random < 0.4) {
        triangle.x -= this.settings.triangleSize;
        triangle.y += this.settings.triangleSize;
        triangle.type = 2; // left
    }
    else if (random < 0.7) {
        triangle.x -= this.settings.triangleSize * 2;
        triangle.type = 2; // left
    }
    else
        triangle.type = 0; // up
    return triangle;
};

wanderingTriangles.upTrianglePath = function (x, y) {
    var s = this.settings.triangleSize;
    this.context.moveTo(x, y);
    this.context.lineTo(x - s, y + s);
    this.context.lineTo(x + s, y + s);
    this.context.lineTo(x, y);
};

wanderingTriangles.downTrianglePath = function (x, y) {
    var s = this.settings.triangleSize;
    this.context.moveTo(x, y);
    this.context.lineTo(x - s, y - s);
    this.context.lineTo(x + s, y - s);
    this.context.lineTo(x, y);
};

wanderingTriangles.leftTrianglePath = function (x, y) {
    var s = this.settings.triangleSize;
    this.context.moveTo(x, y);
    this.context.lineTo(x + s, y - s);
    this.context.lineTo(x + s, y + s);
    this.context.lineTo(x, y);
};

wanderingTriangles.rightTrianglePath = function (x, y) {
    var s = this.settings.triangleSize;
    this.context.moveTo(x, y);
    this.context.lineTo(x - s, y - s);
    this.context.lineTo(x - s, y + s);
    this.context.lineTo(x, y);
};
`
}
