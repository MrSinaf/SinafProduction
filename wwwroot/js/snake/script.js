const terrain = document.getElementById("terrain");
const snake = document.getElementById("snake");
const apple = document.getElementById("apple");
const overlay = document.getElementById("overlay");
const infos = document.getElementById("infos");
const tileWidth = 40;
const gridWidth = 21;
let bodies = [];

class tile {
    // 0 = Rien, 1 = Body, 2 = Head, 3 = Apple
    constructor(x, y, type = 0) {
        this.position = {x: x, y: y};
        this.type = type;
        this.gCost = 0;
        this.hCost = 0;
        this.fCost = 0;
        this.connection = null;
    }

    calculateFCost() {
        this.fCost = this.gCost + this.hCost;
    }

    distanceTo(target) {
        return Math.floor(Math.pow(target.position.x - this.position.x, 2)
            + Math.pow(target.position.y - this.position.y, 2));
    }

    isCollide() {
        return this.type === 1 || this.type === 2;
    }
}

let grid;
let direction;
let currentDirection;
let snakeSpeed;
let moveIntervalId;
let auto = false;
document.addEventListener("keydown", event => {
    event.preventDefault();
    switch (event.key) {
        case "ArrowUp":
            if (currentDirection !== 2)
                direction = 0;
            break;
        case "ArrowRight":
            if (currentDirection !== 3)
                direction = 1;
            break;
        case "ArrowDown":
            if (currentDirection !== 0)
                direction = 2;
            break;
        case "ArrowLeft":
            if (currentDirection !== 1)
                direction = 3;
            break;
        case "Enter":
            launch();
            break;
        case "a":
            auto = !auto;
            break;
    }
})


function ActiveAuto() {
    const toggle = document.getElementById("auto");
    auto = toggle.checked;
}

function launch() {
    stopAnimation();
    snakeSpeed = 50;

    grid = [];
    for (let i = 0; i < 21 * 21; i++) {
        let position = indexToPosition(i);
        grid.push(new tile(position.x, position.y, 0))
    }

    snake.tileId = positionToIndex(10, 10);
    updatePosition(snake);
    spawnApple();
    direction = 0;

    for (const body of bodies) {
        body.remove();
    }
    bodies = [];

    addBody();
    startAnimation();
    overlay.style.display = "none";
}

function move() {
    // TODO : C'est juste pour déboguer
    const debug = document.getElementById("debug");
    let message = "";
    for (let y = 0; y < gridWidth; y++) {
        for (let x = 0; x < gridWidth; x++) {
            const type = grid[positionToIndex(x, y)].type;
            message += type === 0 ? "🟩" : type === 1 ? "⬛" : type === 2 ? "🐍" : "🍎";
        }
        message += "<br>";
    }
    debug.innerHTML = message

    if (auto) {
        const path = pathfinding(grid[snake.tileId], grid[apple.tileId]);
        moveTo(path[path.length - 1].position.x, path[path.length - 1].position.y);

        // TODO : C'est juste pour déboguer
        const debug2 = document.getElementById("debug2");
        let message2 = "";
        for (let y = 0; y < gridWidth; y++) {
            for (let x = 0; x < gridWidth; x++) {
                let tile = grid[positionToIndex(x, y)];
                message2 += path.includes(tile) ? "🟩" : openList.includes(tile) ? "🟪" :
                    closeList.includes(tile) ? "🟥": "⬛";
            }
            message2 += "<br>";
        }
        debug2.innerHTML = message2;
    } else {
        currentDirection = direction;
        const tile = grid[snake.tileId];
        const moveDeltaTo = (x = 0, y = 0) => moveTo(tile.position.x + x, tile.position.y + y);
        switch (direction) {
            case 0:
                moveDeltaTo(0, -1);
                break;
            case 1:
                moveDeltaTo(1, 0);
                break;
            case 2:
                moveDeltaTo(0, 1);
                break;
            case 3:
                moveDeltaTo(-1, 0);
                break;
        }
    }
}

function moveTo(x, y) {
    if (x > 20 || x < 0 || y > 20 || y < 0) {
        loose();
        return;
    }

    let index = positionToIndex(x, y);
    let targetTile = grid[index];
    switch (targetTile.type) {
        case 1:
            loose();
            return;
        case 3:
            addBody();
            spawnApple();
            if (snakeSpeed > 100)
                snakeSpeed -= 5;
            startAnimation();
            break;
    }

    if (bodies.length > 0) {
        grid[bodies[bodies.length - 1].tileId].type = 0;
        for (let i = bodies.length - 1; i > 0; i--) {
            const currentBody = bodies[i];
            currentBody.tileId = bodies[i - 1].tileId;
            updatePosition(currentBody);
        }

        grid[snake.tileId].type = 1;
        bodies[0].tileId = snake.tileId;
        updatePosition(bodies[0]);
    }

    grid[index].type = 2;
    snake.tileId = index;
    updatePosition(snake, direction === 0 ? 180 : direction === 1 ? 270 : direction === 2 ? 360 : 90);
}

function addBody(count = 1) {
    for (let i = 0; i < count; i++) {
        const targetId = bodies.length > 0 ? bodies[bodies.length - 1].tileId : snake.tileId;
        const body = document.createElement("div");

        body.className = "body";
        body.tileId = targetId;

        updatePosition(body);
        terrain.appendChild(body);
        bodies.push(body);
    }
}

function updatePosition(target, rotate = 0) {
    const tile = grid[target.tileId];
    target.style.transform = `translate(${tile.position.x * tileWidth}px, ${tile.position.y * tileWidth}px)`;
    
    if (rotate)
        target.style.transform += ` rotate(${rotate}deg)`;
}

function spawnApple() {
    let index;
    do {
        index = Math.floor(Math.random() * (gridWidth * gridWidth));
    }
    while (grid[index].type !== 0)

    grid[index].type = 3;
    apple.tileId = index;
    updatePosition(apple);
    infos.innerText = `Longueur : ${bodies.length} | Vitesse : ${1 / snakeSpeed * 1000} case/s`
}

function loose() {
    stopAnimation();
    overlay.innerText = `< PERDU : ${bodies.length} de score ("Entrer" pour relancer) >`;
    overlay.style.display = "block";
}

function stopAnimation() {
    clearInterval(moveIntervalId);
}

function startAnimation() {
    stopAnimation();
    moveIntervalId = setInterval(move, snakeSpeed);
    snake.style.transition = `${snakeSpeed}ms linear`;
    for (const body of bodies) {
        body.style.transition = `${snakeSpeed}ms linear`;
    }
}

function indexToPosition(index) {
    return {x: Math.floor(index / gridWidth), y: index % gridWidth};
}

function positionToIndex(x, y) {
    return x * gridWidth + y;
}