var canvas = document.getElementById('canvas');
canvas.width = window.innerWidth;
canvas.height = window.innerHeight;
var context = canvas.getContext("2d");

const triangleSize = 50;
const baseX = -triangleSize/2;
const baseY = -triangleSize/2;
const frameRate = 30;
const triangleMoveSpeed = Math.PI/frameRate/2;
const pointRadius = triangleSize/6;

lines = [];

for (var y = 0; y < context.canvas.height/triangleSize+2; y++) {
    line = [];
    for (var x = 0; x < context.canvas.width/triangleSize+2; x++) {
        var point = { x: baseX+x*triangleSize, y:baseY+y*triangleSize };
        point.radians = Math.random() * (Math.PI*2);
        point.colour = Math.floor(Math.random() * 256);
        point.fadeDir = Math.random() > 0.5 ? 1 : -1;
        line.push(point);
    }
    lines.push(line);
}

function update() {
    for (var y = 0; y < lines.length; y++) {
        for (var x = 0; x < lines[y].length; x++) {
            if (x % 2 == 1) {
                let newVal = lines[y][x].radians + triangleMoveSpeed;
                lines[y][x].radians = newVal%(Math.PI*2);
            } else {
                let newVal = lines[y][x].radians - triangleMoveSpeed;
                lines[y][x].radians = newVal < 0 ? Math.PI*2 : newVal;
            }
            if (lines[y][x].fadeDir == 1) {
                lines[y][x].colour += 1;
                if (lines[y][x].colour == 255) {
                    lines[y][x].fadeDir = -1;
                }
            } else {
                lines[y][x].colour -= 1;
                if (lines[y][x].colour == 0) {
                    lines[y][x].fadeDir = 1;
                }
            }
        }
    }
}

function draw() {
    for (var y = 0; y < lines.length - 1; y++) {
        for (var x = 0; x < lines[y].length - 1; x++) {
            if (y % 2 == 1) {
                var triangle1 = [lines[y][x], lines[y][x+1], lines[y+1][x]];
                drawShape(triangle1, 0);
                var triangle2 = [lines[y][x+1], lines[y+1][x], lines[y+1][x+1]];
                drawShape(triangle2, 2);
            } else {
                var triangle1 = [lines[y][x], lines[y][x+1], lines[y+1][x+1]];
                drawShape(triangle1, 0);
                var triangle2 = [lines[y][x], lines[y+1][x], lines[y+1][x+1]];
                drawShape(triangle2, 2);
            }
        }
    }
}

function drawShape(points, colourPoint) {
    context.beginPath();
    let start = adjusted(points[0]);
    context.moveTo(start.x, start.y);
    for (var i = 1; i < points.length; i++) {
        let next = adjusted(points[i]);
        context.lineTo(next.x, next.y);
    }
    context.closePath();
    var gray = points[colourPoint].colour.toString(16).padStart(2, '0');
    context.fillStyle = '#' + gray + gray + gray; 
    context.fill();
    context.stroke();
}

function adjusted(point) {
    let x = point.x + Math.cos(point.radians) * pointRadius;
    let y = point.y + Math.sin(point.radians) * pointRadius;
    return {x:x, y:y}
}

setInterval(function() {
    update();
    draw();
}, 1000/frameRate);

// calculate base points with base and adjustment, and neighbours
// run a loop
// on each draw, shift by a pixel, before or against base
// and on each draw, render lines and triangles