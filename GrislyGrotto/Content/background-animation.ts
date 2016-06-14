 
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

    enum TriangleType {
        left, right, up, down
    }
    
    class WanderingTriangle {

        size = 15;
        chanceOfJump = 0.01;
        changeOfSecondaryColour = 0.25;
        point: IPoint; // point direction of current triangle
        type: TriangleType;
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
            this.point = {
                x: Math.random() * this.canvas.width,
                y: Math.random() * this.canvas.height
            };
            this.type = Math.floor(Math.random() * 4);
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
            this.nextTriangle(context);
            context.closePath();

            if (Math.random() > 0.5)
                context.stroke();
            else
                context.fill();
        }

        nextTriangle(context: CanvasRenderingContext2D) {
            const random = Math.random();

            if (this.type === TriangleType.up) {
                if (random < 0.2)
                    this.rightTriangle(context);
                else if (random < 0.4)
                    this.leftTriangle(context)
                else {
                    this.point = { x: this.point.x, y: this.point.y + this.size * 2 };
                    this.downTriangle(context);
                }
                return;
            }

            if (this.type === TriangleType.down) {
                if (random < 0.2) {
                    this.point = { x: this.point.x, y: this.point.y - this.size * 2 };
                    this.upTriangle(context);
                } else if(random < 0.6)
                    this.leftTriangle(context);
                else
                    this.rightTriangle(context);
                return;
            }

            if (this.type === TriangleType.left) {
                if (random < 0.1) {
                    this.point = { x: this.point.x + this.size, y: this.point.y - this.size };
                    this.rightTriangle(context);
                } else if (random < 0.4) {
                    this.point = { x: this.point.x + this.size, y: this.point.y + this.size };
                    this.rightTriangle(context);
                } else if (random < 0.7) {
                    this.point = { x: this.point.x + this.size * 2, y: this.point.y };
                    this.rightTriangle(context);
                } else
                    this.upTriangle(context);
                return;
            }

            if (random < 0.1) {
                this.point = { x: this.point.x - this.size, y: this.point.y - this.size };
                this.leftTriangle(context);
            } else if (random < 0.4) {
                this.point = { x: this.point.x - this.size, y: this.point.y + this.size };
                this.leftTriangle(context);
            } else if (random < 0.7) {
                this.point = { x: this.point.x - this.size * 2, y: this.point.y };
                this.leftTriangle(context);
            } else
                this.upTriangle(context);
        }

        upTriangle(context: CanvasRenderingContext2D) {
            var p = this.point; var s = this.size;
            context.moveTo(p.x, p.y);
            context.lineTo(p.x - s, p.y + s);
            context.lineTo(p.x + s, p.y + s);
            context.lineTo(p.x, p.y);
            this.type = TriangleType.up;
        }

        downTriangle(context: CanvasRenderingContext2D) {
            var p = this.point; var s = this.size;
            context.moveTo(p.x, p.y);
            context.lineTo(p.x - s, p.y - s);
            context.lineTo(p.x + s, p.y - s);
            context.lineTo(p.x, p.y);
            this.type = TriangleType.down;
        }

        leftTriangle(context: CanvasRenderingContext2D) {
            var p = this.point; var s = this.size;
            context.moveTo(p.x, p.y);
            context.lineTo(p.x + s, p.y - s);
            context.lineTo(p.x + s, p.y + s);
            context.lineTo(p.x, p.y);
            this.type = TriangleType.left;
        }

        rightTriangle(context: CanvasRenderingContext2D) {
            var p = this.point; var s = this.size;
            context.moveTo(p.x, p.y);
            context.lineTo(p.x - s, p.y - s);
            context.lineTo(p.x - s, p.y + s);
            context.lineTo(p.x, p.y);
            this.type = TriangleType.right;
        }
    }
}