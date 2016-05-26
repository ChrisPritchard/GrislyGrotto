/// <reference path="../typings/jquery/jquery.d.ts" />
var GrislyGrotto;
(function (GrislyGrotto) {
    var BackgroundAnimation = (function () {
        function BackgroundAnimation() {
            this.baseColour = 'black';
            this.triangleColour = 'goldenrod';
            this.fadeAlpha = 0.1;
            this.framerate = 20;
            this.entityCount = 20;
            this.heightMultiplier = 1000;
            this.entitites = [];
        }
        BackgroundAnimation.prototype.initialise = function (canvas, baseColour, triangleColour, framerate) {
            this.context = canvas.getContext('2d');
            if (baseColour)
                this.baseColour = baseColour;
            if (triangleColour)
                this.triangleColour = triangleColour;
            if (framerate)
                this.framerate = framerate;
            var interval = 1000 / this.framerate;
            setInterval($.proxy(this.draw, this), interval);
            this.resizeCanvas();
            $(window).resize($.proxy(this.resizeCanvas, this));
            this.entityCount = this.entityCount * (this.context.canvas.height / this.heightMultiplier);
            for (var i = 0; i < this.entityCount; i++)
                this.entitites.push(new WanderingTriangle(this.context.canvas));
        };
        BackgroundAnimation.prototype.resizeCanvas = function () {
            this.context.canvas.width = document.body.clientWidth;
            this.context.canvas.height = document.body.clientHeight;
        };
        BackgroundAnimation.prototype.draw = function () {
            this.context.fillStyle = this.baseColour;
            this.context.globalAlpha = this.fadeAlpha;
            this.context.fillRect(0, 0, this.context.canvas.width, this.context.canvas.height);
            this.context.globalAlpha = 1;
            this.context.fillStyle = this.context.strokeStyle = this.triangleColour;
            for (var i = 0; i < this.entitites.length; i++)
                this.entitites[i].draw(this.context);
        };
        return BackgroundAnimation;
    }());
    GrislyGrotto.BackgroundAnimation = BackgroundAnimation;
    var WanderingTriangle = (function () {
        function WanderingTriangle(canvas) {
            this.size = 15;
            this.chanceOfJump = 0.01;
            this.canvas = canvas;
            this.jump();
        }
        WanderingTriangle.prototype.jump = function () {
            this.position = {
                x: Math.random() * this.canvas.width,
                y: Math.random() * this.canvas.height
            };
        };
        WanderingTriangle.prototype.draw = function (context) {
            if (Math.random() < this.chanceOfJump)
                this.jump();
            context.beginPath();
            context.moveTo(this.position.x, this.position.y);
            var random = Math.random();
            if (random < 0.25)
                this.rightTriangle(context, this.position, this.size);
            else if (random < 0.5)
                this.downTriangle(context, this.position, this.size);
            else if (random < 0.75)
                this.leftTriangle(context, this.position, this.size);
            else
                this.upTriangle(context, this.position, this.size);
            context.closePath();
            if (Math.random() > 0.5)
                context.stroke();
            else
                context.fill();
        };
        WanderingTriangle.prototype.rightTriangle = function (context, position, size) {
            context.lineTo(position.x + size, position.y - size);
            context.lineTo(position.x + size, position.y + size);
            context.lineTo(position.x, position.y);
            this.position.x += size;
            this.position.y += size; // bottom right
        };
        WanderingTriangle.prototype.downTriangle = function (context, position, size) {
            context.lineTo(position.x + size, position.y + size);
            context.lineTo(position.x - size, position.y + size);
            context.lineTo(position.x, position.y);
            this.position.x += size;
            this.position.y += size; // bottom right
        };
        WanderingTriangle.prototype.leftTriangle = function (context, position, size) {
            context.lineTo(position.x - size, position.y + size);
            context.lineTo(position.x - size, position.y - size);
            context.lineTo(position.x, position.y);
            this.position.x -= size;
            this.position.y += size; // bottom left
        };
        WanderingTriangle.prototype.upTriangle = function (context, position, size) {
            context.lineTo(position.x - size, position.y - size);
            context.lineTo(position.x + size, position.y - size);
            context.lineTo(position.x, position.y);
            this.position.x -= size;
            this.position.y -= size; // upper left
        };
        return WanderingTriangle;
    }());
})(GrislyGrotto || (GrislyGrotto = {}));
//# sourceMappingURL=background-animation.js.map