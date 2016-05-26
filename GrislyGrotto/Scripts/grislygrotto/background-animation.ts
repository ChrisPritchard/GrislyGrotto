/// <reference path="../typings/jquery/jquery.d.ts" />
 
module GrislyGrotto {

    interface IEntity {
        draw(context: CanvasRenderingContext2D);
    }

    interface IPoint {
        x: number;
        y: number;
    }

    export class BackgroundAnimation {

        baseColour = 'black';
        triangleColour = 'goldenrod';
        fadeAlpha = 0.1;
        framerate = 20;
        entityCount = 20;
        heightMultiplier = 1000;
        context: CanvasRenderingContext2D;

        entitites: IEntity[] = [];

        initialise(canvas: HTMLCanvasElement, baseColour?: string, triangleColour?: string, framerate?: number) {
            this.context = canvas.getContext('2d');
            if (baseColour) this.baseColour = baseColour;
            if (triangleColour) this.triangleColour = triangleColour;
            if (framerate) this.framerate = framerate;

            var interval = 1000 / this.framerate;
            setInterval($.proxy(this.draw, this), interval)

            this.resizeCanvas();
            $(window).resize($.proxy(this.resizeCanvas, this));

            this.entityCount = this.entityCount * (this.context.canvas.height / this.heightMultiplier);
            for (var i = 0; i < this.entityCount; i++)
                this.entitites.push(new WanderingTriangle(this.context.canvas));
        }

        resizeCanvas() {
            this.context.canvas.width = document.body.clientWidth;
            this.context.canvas.height = document.body.clientHeight;
        }

        draw() {
            this.context.fillStyle = this.baseColour;
            this.context.globalAlpha = this.fadeAlpha;
            this.context.fillRect(0, 0, this.context.canvas.width, this.context.canvas.height);

            this.context.globalAlpha = 1;
            this.context.fillStyle = this.context.strokeStyle = this.triangleColour;

            for (var i = 0; i < this.entitites.length; i++)
                this.entitites[i].draw(this.context);
        }
    }

    class WanderingTriangle {

        size = 15;
        chanceOfJump = 0.01;
        position: IPoint;
        canvas: HTMLCanvasElement;

        constructor(canvas: HTMLCanvasElement) {
            this.canvas = canvas;
            this.jump();
        }

        jump() {
            this.position = {
                x: Math.random() * this.canvas.width,
                y: Math.random() * this.canvas.height
            };
        }

        draw(context: CanvasRenderingContext2D) {
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
        }

        rightTriangle(context: CanvasRenderingContext2D, position: IPoint, size: number) {
            context.lineTo(position.x + size, position.y - size);
            context.lineTo(position.x + size, position.y + size);
            context.lineTo(position.x, position.y);
            this.position.x += size; this.position.y += size; // bottom right
        }

        downTriangle(context: CanvasRenderingContext2D, position: IPoint, size: number) {
            context.lineTo(position.x + size, position.y + size);
            context.lineTo(position.x - size, position.y + size);
            context.lineTo(position.x, position.y);
            this.position.x += size; this.position.y += size; // bottom right
        }

        leftTriangle(context: CanvasRenderingContext2D, position: IPoint, size: number) {
            context.lineTo(position.x - size, position.y + size);
            context.lineTo(position.x - size, position.y - size);
            context.lineTo(position.x, position.y);
            this.position.x -= size; this.position.y += size; // bottom left
        }

        upTriangle(context: CanvasRenderingContext2D, position: IPoint, size: number) {
            context.lineTo(position.x - size, position.y - size);
            context.lineTo(position.x + size, position.y - size);
            context.lineTo(position.x, position.y);
            this.position.x -= size; this.position.y -= size; // upper left
        }
    }
}