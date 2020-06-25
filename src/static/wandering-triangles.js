let wanderingTriangles = {}
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
