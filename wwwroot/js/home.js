const main = document.querySelector("main");
const skillsDescription = document.getElementById("skills__description");
const skillsLabelsTitle = document.querySelector("#skills__description-labels > h3");
const skillsLabelsDesc = document.querySelector("#skills__description-labels > p:first-of-type");
const skillsLabelsInfos = document.querySelector("#skills__description-labels > p:last-of-type");
let currentTargetLogo;
let currentWidthWindow = window.innerWidth;

main.style.display = "none";

let skillsData;
fetch("/data/skills.json?v=3").then(value => value.json()).then(data => {
    skillsData = data;
    if (window.location.hash) {
        showPresentation();
    }
});

function showPresentation() {
    nav.classList.toggle("nav--open", false);
    document.body.classList.add("presentation");
    main.style.display = "block";
    
    setTimeout(() => {
        onLogoClick(document.querySelector("#skills__logos > :first-child"));
        adjustHeightProjects();
    }, 1000);
}

window.addEventListener('resize', function () {
    setTimeout(() => {
        if (currentWidthWindow !== window.innerWidth) {
            currentWidthWindow = window.innerWidth;
            onLogoClick(currentTargetLogo);
        }

        adjustHeightProjects();
    }, 300);
});

function onLogoClick(target) {
    if (currentTargetLogo !== target && currentTargetLogo)
        deselectLogo(currentTargetLogo);

    target.classList.toggle("skills__logos--target", true);
    let positionX = target.offsetLeft - skillsDescription.offsetLeft - 80;
    let positionY = skillsDescription.offsetTop - target.offsetTop + 80;

    let item = skillsData.find(value => value.id === target.id);
    skillsLabelsTitle.innerHTML = item.name;
    skillsLabelsDesc.innerHTML = item.description;
    skillsLabelsInfos.innerHTML = item.infos;

    target.style.transform = `translateX(${-positionX}px) translateY(${positionY}px) scale(3)`;
    currentTargetLogo = target;
}

function deselectLogo(target) {
    target.classList.toggle("skills__logos--target", false);
    target.style.transform = "none";
}

function adjustHeightProjects() {
    const element = document.querySelector('#news');
    const multiple = 250;
    element.style.minHeight = ``;
    const currentHeight = element.offsetHeight;
    const newHeight = Math.ceil(currentHeight / multiple) * multiple;
    element.style.minHeight = `${newHeight}px`;
}