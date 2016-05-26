 
module GrislyGrotto {

    interface IEntity {
        draw(context: CanvasRenderingContext2D);
    }

    interface IPoint {
        x: number;
        y: number;
    }

    export class BackgroundAnimation {

        private backgroundColour: string;
        private fadeAlpha = 0.1;
        private framerate = 20;
        private entityCount = 20;
        private heightMultiplier = 1000;
        private context: CanvasRenderingContext2D;

        private entitites: IEntity[] = [];

        initialise(canvas: HTMLCanvasElement, backgroundColour: string, primaryColour: string, secondaryColour: string) {
            this.context = canvas.getContext("2d");
            this.backgroundColour = backgroundColour;

            const interval = 1000 / this.framerate;
            setInterval(() => { this.draw(); }, interval);

            this.resizeCanvas();
            const self = this;
            window.onresize = () => { self.resizeCanvas(); };

            this.entityCount = this.entityCount * (this.context.canvas.height / this.heightMultiplier);
            for (let i = 0; i < this.entityCount; i++)
                this.entitites.push(new WanderingTriangle(this.context.canvas, primaryColour, secondaryColour));
        }

        resizeCanvas() {
            this.context.canvas.width = document.body.clientWidth;
            this.context.canvas.height = document.body.clientHeight;
        }

        draw() {
            this.context.fillStyle = this.backgroundColour;
            this.context.globalAlpha = this.fadeAlpha;
            this.context.fillRect(0, 0, this.context.canvas.width, this.context.canvas.height);

            this.context.globalAlpha = 1;

            for (let i = 0; i < this.entitites.length; i++)
                this.entitites[i].draw(this.context);
        }
    }

    class WanderingTriangle {

        size = 15;
        chanceOfJump = 0.01;
        changeOfSecondaryColour = 0.25;
        position: IPoint;
        canvas: HTMLCanvasElement;

        primaryColour: string;
        secondaryColour: string;
        colour: string;

        constructor(canvas: HTMLCanvasElement, primaryColour: string, secondaryColour: string) {
            this.canvas = canvas;
            this.colour = this.primaryColour = primaryColour;
            this.secondaryColour = secondaryColour;
            this.jump();
        }

        jump() {
            this.position = {
                x: Math.random() * this.canvas.width,
                y: Math.random() * this.canvas.height
            };
            if (Math.random() < this.changeOfSecondaryColour)
                this.colour = this.secondaryColour;
            else
                this.colour = this.primaryColour;
        }

        draw(context: CanvasRenderingContext2D) {
            if (Math.random() < this.chanceOfJump)
                this.jump();

            context.fillStyle = context.strokeStyle = this.colour;

            context.beginPath();
            context.moveTo(this.position.x, this.position.y);

            const random = Math.random();
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