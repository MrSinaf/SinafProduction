let openList, closeList;

function pathfinding(start, target) {
    closeList = [];
    openList = [start];

    while (openList.length > 0) {
        let current = openList[0];
        current.calculateFCost();
        for (const node of openList) {
            node.calculateFCost();
            if (node.fCost < current.fCost || node.fCost === current.fCost && node.hCost < current.hCost)
                current = node;
        }

        closeList.push(current);
        openList.splice(openList.indexOf(current), 1);

        if (current === target) {
            let pathNode = target;
            let path = [];
            while (pathNode !== start) {
                path.push(pathNode);
                pathNode = pathNode.connection;
            }

            return path;
        }

        const neighbours = getNeighbours(current)
        for (const neighbour of neighbours) {
            if (neighbour.isCollide() || closeList.includes(neighbour))
                continue;

            const inOpenList = openList.includes(neighbour);
            const costToNeighbour = current.gCost + current.distanceTo(neighbour);
            if (!inOpenList || costToNeighbour < neighbour.gCost) {
                neighbour.gCost = costToNeighbour;
                neighbour.connection = current;

                if (!inOpenList) {
                    neighbour.hCost = neighbour.distanceTo(target);
                    openList.push(neighbour);
                }
            }
        }
    }

    const neighbours = getNeighbours(start);
    closeList = [];
    let path = start;
    for (const neighbour of neighbours) {
        let openList2 = [];
        let closeList2 = [];
        if (!neighbour.isCollide()) {
            openList2.push(neighbour);
            while (openList2.length > 0) {
                let current = openList2[0];
                openList2.splice(0, 1);
                closeList2.push(current);

                const neighbours = getNeighbours(current);
                for (const neighbour of neighbours) {
                    const inOpenList = openList2.includes(neighbour);
                    if (!neighbour.isCollide() && !closeList2.includes(neighbour) && !inOpenList) {
                        openList2.push(neighbour);
                    }
                }
            }

            if (closeList.length < closeList2.length) {
                closeList = closeList2;
                openList = openList2;
                path = neighbour;
            }
        }
    }

    if (path === start)
        loose();

    return [path];
}

function getNeighbours(tile) {
    let neighbours = [];
    const check = (x, y) => {
        x += tile.position.x;
        y += tile.position.y;
        if (x < 0 || y < 0 || x >= gridWidth || y >= gridWidth)
            return;

        let index = positionToIndex(x, y);
        neighbours.push(grid[index]);
    }

    check(-1, 0);
    check(1, 0);
    check(0, 1);
    check(0, -1);

    return neighbours;
}